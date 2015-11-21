// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.ComponentModel.Composition;

namespace Stylize.Engine.Rules
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class OrderStyleRuleAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the rule that this rule must be executed after (assuming both are enabled).
        /// </summary>
        public string After { get; set; }

        /// <summary>
        /// Gets or sets the name of the rule that this rule must be executed before (assuming both are enabled).
        /// </summary>
        public string Before { get; set; }
    }
}
