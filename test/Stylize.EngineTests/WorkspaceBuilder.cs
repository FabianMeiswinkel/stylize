// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Stylize.EngineTests
{
    sealed class WorkspaceBuilder : IDisposable
    {
        static readonly IReadOnlyList<MetadataReference> CommonReferences = new[]
        {
            // mscorlib.dll
            MetadataReference.CreateFromFile(typeof (object).Assembly.Location),

            // System.Core.dll
            MetadataReference.CreateFromFile(typeof (Enumerable).Assembly.Location)
        };

        int documentId;
        readonly ProjectId projectId;
        readonly AdhocWorkspace workspace;

        public WorkspaceBuilder(string projectName = "TestProject")
        {
            this.documentId = 0;

            this.workspace = new AdhocWorkspace();

            var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                name: projectName,
                assemblyName: projectName,
                language: LanguageNames.CSharp,
                metadataReferences: CommonReferences);
            Project project = this.workspace.AddProject(projectInfo);
            this.projectId = project.Id;
        }

        public Document AddClass(string className, string directoryPath = "C:\\")
        {
            string fileName = className + ".cs";
            string filePath = Path.Combine(directoryPath, fileName);

            var text = TextAndVersion.Create(
                SourceText.From($"public class {className} {{ }}"),
                VersionStamp.Default,
                filePath: filePath);

            var documentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(this.projectId),
                fileName,
                filePath: filePath,
                loader: TextLoader.From(text));

            return this.workspace.AddDocument(documentInfo);
        }

        public Document AddDocument(string text)
        {
            return this.workspace.AddDocument(
                this.projectId,
                String.Concat("Test", this.documentId++, ".cs"),
                SourceText.From(text));
        }

        public void ApplyOptions(Func<OptionSet, OptionSet> applyOptions)
        {
            this.workspace.Options = applyOptions(this.workspace.Options);
        }

        public Workspace BuildWorkspace()
        {
            return this.workspace;
        }

        public void Dispose()
        {
            this.workspace.Dispose();
        }
    }
}
