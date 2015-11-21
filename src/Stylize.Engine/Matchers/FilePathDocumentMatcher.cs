// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine.Matchers
{
    [ExportDocumentMatcher(Name)]
    public class FilePathDocumentMatcher : RegexDocumentMatcher
    {
        public const string Name = "FilePathDocumentMatcher";

        public FilePathDocumentMatcher()
            : base(FilePathRegex)
        {
        }

        [ExportStylizeOption]
        public static Option<string> FilePathRegex { get; } = new Option<string>(Name, "filePathRegex");

        protected override string SelectValue(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            return document.FilePath;
        }
    }
}
