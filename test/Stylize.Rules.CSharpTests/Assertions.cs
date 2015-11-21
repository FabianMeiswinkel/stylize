// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharpTests
{
    static class Assertions
    {
        static readonly IReadOnlyList<MetadataReference> CommonReferences = new[]
        {
            // mscorlib.dll
            MetadataReference.CreateFromFile(typeof (object).Assembly.Location),

            // System.Core.dll
            MetadataReference.CreateFromFile(typeof (Enumerable).Assembly.Location)
        };

        public static Task AssertNoEnforcementAsync(IStyleRule rule, string documentText)
        {
            return AssertNoEnforcementAsync(rule, documentText, options => options);
        }

        public static async Task AssertNoEnforcementAsync(
            IStyleRule rule, string documentText, Func<OptionSet, OptionSet> applyOptions)
        {
            using (var workspace = new AdhocWorkspace())
            {
                workspace.Options = applyOptions(workspace.Options);

                Project project = workspace.AddProject(BuildProject());
                Document document = workspace.AddDocument(project.Id, "TestFile.cs", SourceText.From(documentText));

                Solution enforcedSolution = await rule.EnforceAsync(document);
                Document enforcedDocument = enforcedSolution.GetDocument(document.Id);

                if (!document.Equals(enforcedDocument))
                {
                    List<TextChange> changes = (await enforcedDocument.GetTextChangesAsync(document)).ToList();
                    if (changes.Count == 0)
                    {
                        Assert.Fail("Solution mutated without document changes");
                    }

                    Console.WriteLine("Document changes:");
                    foreach (TextChange change in changes)
                    {
                        Console.WriteLine($"\t{change}");
                    }

                    Assert.Fail($"Enforced document has {changes.Count} changes; expected none");
                }
            }
        }

        public static Task AssertEnforcementAsync(IStyleRule rule, string originalText, string expectedText)
        {
            return AssertEnforcementAsync(rule, originalText, expectedText, options => options);
        }

        public static async Task AssertEnforcementAsync(
            IStyleRule rule, string originalText, string expectedText, Func<OptionSet, OptionSet> applyOptions)
        {
            using (var workspace = new AdhocWorkspace())
            {
                workspace.Options = applyOptions(workspace.Options);

                Project project = workspace.AddProject(BuildProject());
                Document document = workspace.AddDocument(project.Id, "TestFile.cs", SourceText.From(originalText));

                Solution enforcedSolution = await rule.EnforceAsync(document);
                Document enforcedDocument = enforcedSolution.GetDocument(document.Id);

                if (document.Equals(enforcedDocument))
                {
                    Assert.Fail("Expected enforcement, but no changes were made to the document");
                }

                SyntaxTree enforcedSyntax = await enforcedDocument.GetSyntaxTreeAsync();
                SyntaxTree expectedSyntax = SyntaxFactory.ParseCompilationUnit(expectedText).SyntaxTree;
                List<TextChange> unexpectedChanges = expectedSyntax.GetChanges(enforcedSyntax).ToList();
                if (unexpectedChanges.Count > 0)
                {
                    Console.WriteLine("Unexpected changes:");
                    List<TextChange> changes = (await enforcedDocument.GetTextChangesAsync(document)).ToList();
                    foreach (TextChange change in changes)
                    {
                        Console.WriteLine($"\t{change}");
                    }

                    Assert.Fail($"Enforced document has {changes.Count} unexpected changes");
                }
            }
        }

        static ProjectInfo BuildProject()
        {
            return ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                name: "TestProject",
                assemblyName: "TestProject",
                language: LanguageNames.CSharp,
                metadataReferences: CommonReferences);
        }
    }
}
