// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine.Configuration
{
    public interface IOptionApplier
    {
        OptionSet ApplyOptions(OptionSet options);
    }
}
