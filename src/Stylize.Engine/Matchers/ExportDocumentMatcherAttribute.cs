// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.ComponentModel.Composition;

namespace Stylize.Engine.Matchers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExportDocumentMatcherAttribute : ExportAttribute, INamedMetadata
    {
        public ExportDocumentMatcherAttribute(string name)
            : base(typeof(IDocumentMatcher))
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the document matcher.
        /// </summary>
        public string Name { get; }
    }
}
