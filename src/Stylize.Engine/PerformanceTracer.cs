// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Diagnostics;

namespace Stylize.Engine
{
    public sealed class PerformanceTracer : IDisposable
    {
        readonly string actionName;
        readonly string targetName;
        readonly Stopwatch watch;

        public PerformanceTracer(string actionName, string targetName)
        {
            this.actionName = actionName;
            this.targetName = targetName;
            this.watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            this.watch.Stop();

            Log.WriteVerbose($"Executed {this.actionName} on {this.targetName} in {this.watch.Elapsed}");
        }
    }
}
