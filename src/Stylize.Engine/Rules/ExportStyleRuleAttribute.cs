// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.ComponentModel.Composition;

namespace Stylize.Engine.Rules
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExportStyleRuleAttribute : ExportAttribute
    {
        public ExportStyleRuleAttribute(string name, string languageName)
            : base(typeof(IStyleRule))
        {
            this.Name = name;
            this.LanguageName = languageName;
        }

        /// <summary>
        /// Gets the name of the language (as registered with Roslyn) that the exported rule supports.
        /// </summary>
        public string LanguageName { get; }

        /// <summary>
        /// Gets the type of the rule (whose name is used to determine if the rule is active).
        /// </summary>
        public string Name { get; }
    }
}
