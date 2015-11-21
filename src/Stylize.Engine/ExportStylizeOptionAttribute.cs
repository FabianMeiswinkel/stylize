// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExportStylizeOptionAttribute : ExportAttribute
    {
        internal const string OptionContractName = "StylizeOption";

        public ExportStylizeOptionAttribute()
            : base(OptionContractName, typeof(IOption))
        {
        }
    }
}
