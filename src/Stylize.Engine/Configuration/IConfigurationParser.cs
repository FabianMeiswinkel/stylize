// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine.Configuration
{
    public interface IConfigurationParser
    {
        IReadOnlyList<string> ExportAssemblyNames { get; }

        StylizeConfiguration ParseConfiguration(IReadOnlyList<IOption> supportedOptions);
    }
}
