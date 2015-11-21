// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Stylize.Engine;

namespace Stylize
{
    class StylizeLogTraceListener : TraceListener
    {
        readonly SourceSwitch consoleSwitch;
        StreamWriter logFileWriter;

        public StylizeLogTraceListener()
        {
            this.consoleSwitch = new SourceSwitch("StylizeConsole")
            {
                Level = SourceLevels.Information
            };

            Log.Source.Listeners.Add(this);
        }

        public bool VerboseConsoleLogging
        {
            set { this.consoleSwitch.Level = value ? SourceLevels.All : SourceLevels.Information; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "logFileWriter",
            Justification = "CodeAnalysis does not understand the ?. operator.")]
        protected override void Dispose(bool disposing)
        {
            this.logFileWriter?.Dispose();
            base.Dispose(disposing);
        }

        public void EnableLogFile(string logFilePath)
        {
            this.logFileWriter?.Dispose();
            this.logFileWriter = new StreamWriter(logFilePath, append: false);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            this.WriteLine(eventType, data);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            this.WriteLine(eventType, String.Join(", ", data));
        }

        public override void TraceEvent(
            TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.WriteLine(eventType, message);
        }

        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            this.WriteLine(eventType, String.Format(format, args));
        }

        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }

        void WriteLine(TraceEventType eventType, object data)
        {
            this.logFileWriter?.WriteLine($"{DateTime.UtcNow},{eventType},{data}");

            if (!this.consoleSwitch.ShouldTrace(eventType))
            {
                return;
            }

            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    Console.Error.WriteLine(data);
                    return;

                default:
                    Console.Out.WriteLine(data);
                    return;
            }
        }
    }
}
