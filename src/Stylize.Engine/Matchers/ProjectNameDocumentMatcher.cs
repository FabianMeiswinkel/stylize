// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine.Matchers
{
    [ExportDocumentMatcher(Name)]
    public class ProjectNameDocumentMatcher : RegexDocumentMatcher
    {
        public const string Name = "ProjectNameDocumentMatcher";

        public ProjectNameDocumentMatcher()
            : base(ProjectNameRegex)
        {
        }

        [ExportStylizeOption]
        public static Option<string> ProjectNameRegex { get; } = new Option<string>(Name, "projectNameRegex");

        protected override string SelectValue(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            return document.Project.Name;
        }
    }
}
