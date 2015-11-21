// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.ComponentModel.Composition;

namespace Stylize.Engine.Repositories
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExportSourceRepositoryAttribute : ExportAttribute
    {
        public const string CurrentName = "Current";

        public ExportSourceRepositoryAttribute(string contractName)
            : base(contractName, typeof(ISourceRepository))
        {
        }
    }
}
