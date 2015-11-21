// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Diagnostics;

namespace Stylize.Engine
{
    public static class Log
    {
        const int DefaultEventId = 0;

        public static TraceSource Source { get; } = new TraceSource("StylizeLog")
        {
            Switch = { Level = SourceLevels.All }
        };

        public static void WriteError(string format, params object[] args)
        {
            Source.TraceEvent(TraceEventType.Error, DefaultEventId, format, args);
        }

        public static void WriteInformation(string format, params object[] args)
        {
            Source.TraceEvent(TraceEventType.Information, DefaultEventId, format, args);
        }

        public static void WriteVerbose(string format, params object[] args)
        {
            Source.TraceEvent(TraceEventType.Verbose, DefaultEventId, format, args);
        }

        public static void WriteWarning(string format, params object[] args)
        {
            Source.TraceEvent(TraceEventType.Warning, DefaultEventId, format, args);
        }
    }
}
