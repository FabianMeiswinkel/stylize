// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine
{
    public static class GlobalOptions
    {
        public const string Name = "globalOptions";

        [ExportStylizeOption]
        public static Option<string> NewLineText { get; } =
            new Option<string>(Name, "newLineText", defaultValue: "\r\n");
    }
}
