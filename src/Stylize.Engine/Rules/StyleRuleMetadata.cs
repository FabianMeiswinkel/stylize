// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Collections.Generic;
using System.Linq;

namespace Stylize.Engine.Rules
{
    public class StyleRuleMetadata : INamedMetadata
    {
        static readonly IReadOnlyList<string> DefaultNamesValue = new string[0];

        public StyleRuleMetadata(IDictionary<string, object> metadata)
        {
            this.AfterNames = metadata.GetValue("After", DefaultNamesValue).Where(n => n != null).ToList();
            this.BeforeNames = metadata.GetValue("Before", DefaultNamesValue).Where(n => n != null).ToList();
            this.LanguageName = (string)metadata["LanguageName"];
            this.Name = (string)metadata["Name"];
        }

        /// <summary>
        /// Gets the names of the rules which this rule must execute after.
        /// </summary>
        public IReadOnlyList<string> AfterNames { get; }

        /// <summary>
        /// Gets the names of the rules which this rule must execute before.
        /// </summary>
        public IReadOnlyList<string> BeforeNames { get; }

        /// <summary>
        /// Gets the name of the language (as registered with Roslyn) that the exported rule supports.
        /// </summary>
        public string LanguageName { get; }

        /// <summary>
        /// Gets the name of the rule.
        /// </summary>
        public string Name { get; }
    }
}
