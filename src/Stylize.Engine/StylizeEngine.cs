// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;
using Stylize.Engine.Configuration;
using Stylize.Engine.Matchers;
using Stylize.Engine.Repositories;
using Stylize.Engine.Rules;

namespace Stylize.Engine
{
    public sealed class StylizeEngine : IDisposable
    {
        readonly CompositionContainer container;
        readonly IReadOnlyList<Export<IDocumentMatcher, INamedMetadata>> exclusionMatchers;
        readonly IOptionApplier globalOptions;
        readonly IReadOnlyDictionary<string, IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>>> languageRuleMap;
        readonly ISourceRepository repository;

        public StylizeEngine(IConfigurationParser configurationParser)
        {
            if (configurationParser == null) { throw new ArgumentNullException(nameof(configurationParser)); }

            Log.WriteVerbose("Initializing MEF");
            var catalog = new AggregateCatalog(
                new AssemblyCatalog(Assembly.GetExecutingAssembly().Location));
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string exportAssemblyName in configurationParser.ExportAssemblyNames)
            {
                string exportAssemblyPath = Path.IsPathRooted(exportAssemblyName)
                    ? exportAssemblyName
                    : Path.Combine(currentDirectory, exportAssemblyName);
                catalog.Catalogs.Add(new AssemblyCatalog(exportAssemblyPath));
            }

            this.container = new CompositionContainer(catalog);
            IReadOnlyList<IOption> supportedOptions = this.container.GetExportedValues<IOption>(
                ExportStylizeOptionAttribute.OptionContractName).ToList();

            Log.WriteVerbose("Parsing configuration");
            StylizeConfiguration configuration = configurationParser.ParseConfiguration(supportedOptions);
            this.globalOptions = configuration.GlobalOptions;

            Log.WriteVerbose("Loading exports");
            if (!String.IsNullOrEmpty(configuration.RepositoryName))
            {
                this.repository = this.container.GetExportedValue<ISourceRepository>(configuration.RepositoryName);
                this.container.ComposeExportedValue(ExportSourceRepositoryAttribute.CurrentName, this.repository);
            }

            this.exclusionMatchers = this.container.GetExports<IDocumentMatcher, INamedMetadata>(
                configuration.ExclusionMatchers);

            IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>> rules =
                this.container.GetExports<IStyleRule, StyleRuleMetadata>(configuration.Rules);
            this.languageRuleMap = OrderAndMapRules(rules);

            Log.WriteVerbose("Engine initialized");
        }

        public void Dispose()
        {
            this.container.Dispose();
        }

        static IReadOnlyDictionary<string, IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>>> OrderAndMapRules(
            IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>> rules)
        {
            var languageRuleMap = new Dictionary<string, IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>>>();

            ILookup<string, Export<IStyleRule, StyleRuleMetadata>> languageRuleLookup =
                rules.ToLookup(r => r.Metadata.LanguageName);
            foreach (IGrouping<string, Export<IStyleRule, StyleRuleMetadata>> languageRules in languageRuleLookup)
            {
                var graph = new DirectedGraph<Export<IStyleRule, StyleRuleMetadata>>(languageRules);

                Dictionary<string, Export<IStyleRule, StyleRuleMetadata>> nameRuleMap =
                    languageRules.ToDictionary(r => r.Metadata.Name);
                foreach (Export<IStyleRule, StyleRuleMetadata> languageRule in languageRules)
                {
                    foreach (string afterName in languageRule.Metadata.AfterNames)
                    {
                        Export<IStyleRule, StyleRuleMetadata> afterRule;
                        if (nameRuleMap.TryGetValue(afterName, out afterRule))
                        {
                            graph.AddEdge(afterRule, languageRule);
                        }
                    }

                    foreach (string beforeName in languageRule.Metadata.BeforeNames)
                    {
                        Export<IStyleRule, StyleRuleMetadata> beforeRule;
                        if (nameRuleMap.TryGetValue(beforeName, out beforeRule))
                        {
                            graph.AddEdge(languageRule, beforeRule);
                        }
                    }
                }

                IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>> cycle = graph.FindCycle();
                if (cycle.Count > 0)
                {
                    var ruleNames = String.Join(", ", cycle.Select(r => r.Metadata.Name));
                    throw new InvalidOperationException($"Dependency cycle exists in rules {ruleNames}");
                }

                languageRuleMap.Add(languageRules.Key, graph.Sort());
            }

            return languageRuleMap;
        }

        public async Task RunAsync(string solutionFilePath)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                using (new PerformanceTracer("MSBuildWorkspace.OpenSolutionAsync", solutionFilePath))
                {
                    await workspace.OpenSolutionAsync(solutionFilePath);
                }

                await this.RunAsync(workspace);
            }
        }

        public async Task RunAsync(Workspace workspace)
        {
            if (workspace == null) { throw new ArgumentNullException(nameof(workspace)); }
            if (workspace.CurrentSolution == null)
            {
                throw new ArgumentException("Workspace does not have an open CurrentSolution", nameof(workspace));
            }

            // Apply global options
            workspace.Options = this.globalOptions.ApplyOptions(workspace.Options);

            Solution solution = workspace.CurrentSolution;

            foreach (DocumentId documentId in solution.Projects.SelectMany(p => p.DocumentIds))
            {
                Document document = solution.GetDocument(documentId);
                if (await this.ShouldExcludeAsync(document))
                {
                    continue;
                }

                IReadOnlyList<Export<IStyleRule, StyleRuleMetadata>> styleRules;
                if (this.languageRuleMap.TryGetValue(document.Project.Language, out styleRules))
                {
                    foreach (Export<IStyleRule, StyleRuleMetadata> styleRule in styleRules)
                    {
                        // Use GetDocument each time to ensure we get the corresponding document from the potentially
                        // mutated solution.
                        Document currentDocument = solution.GetDocument(documentId);

                        currentDocument.ApplyOptions(styleRule.OptionApplier);

                        using (new PerformanceTracer(styleRule.Metadata.Name, currentDocument.Name))
                        {
                            solution = await styleRule.Part.EnforceAsync(currentDocument);
                        }
                    }
                }
            }

            // If we have an ISourceRepository and we have made changes to the solution, checkout all changed documents
            // before applying the changes.
            if (this.repository != null)
            {
                SolutionChanges solutionChanges = solution.GetChanges(workspace.CurrentSolution);
                foreach (ProjectChanges projectChanges in solutionChanges.GetProjectChanges())
                {
                    foreach (DocumentId changedDocumentId in projectChanges.GetChangedDocuments())
                    {
                        Document changedDocument = solution.GetDocument(changedDocumentId);
                        if (!await this.repository.HasPendingChangeAsync(changedDocument))
                        {
                            await this.repository.CheckOutAsync(changedDocument);
                        }
                    }
                }
            }

            Log.WriteVerbose("Applying solution changes");
            if (!workspace.TryApplyChanges(solution))
            {
                Log.WriteError("Unable to apply the changes to the solution");
            }

            Log.WriteVerbose("Style formatting complete");
        }

        async Task<bool> ShouldExcludeAsync(Document document)
        {
            foreach (Export<IDocumentMatcher, INamedMetadata> exclusionMatcher in this.exclusionMatchers)
            {
                document.ApplyOptions(exclusionMatcher.OptionApplier);

                using (new PerformanceTracer(exclusionMatcher.Metadata.Name, document.Name))
                {
                    if (await exclusionMatcher.Part.IsMatchAsync(document))
                    {
                        Log.WriteVerbose(
                            $"Matcher {exclusionMatcher.Metadata.Name} excluded document {document.Name}");
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
