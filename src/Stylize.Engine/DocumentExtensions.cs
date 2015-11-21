// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Stylize.Engine.Configuration;

namespace Stylize.Engine
{
    public static class DocumentExtensions
    {
        public static void ApplyOptions(this TextDocument document, IOptionApplier optionApplier)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }
            if (optionApplier == null) { throw new ArgumentNullException(nameof(optionApplier)); }

            Workspace workspace = document.Project.Solution.Workspace;

            OptionSet oldOptions = workspace.Options;
            OptionSet newOptions = optionApplier.ApplyOptions(workspace.Options);

            if (oldOptions != newOptions)
            {
                workspace.Options = newOptions;
            }
        }

        public static T GetOption<T>(this TextDocument document, Option<T> option)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            return document.Project.Solution.Workspace.Options.GetOption(option);
        }
    }
}
