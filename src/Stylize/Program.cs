// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Stylize.Engine;
using Stylize.Engine.Configuration;

namespace Stylize
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var traceListener = new StylizeLogTraceListener())
            {
                try
                {
                    var options = new Options();
                    if (!Parser.Default.ParseArguments(args, options))
                    {
                        return;
                    }

                    if (!File.Exists(options.ConfigFilePath))
                    {
                        Log.WriteError($"Configuration file not found at: {options.ConfigFilePath}");
                        return;
                    }

                    if (!File.Exists(options.SolutionFilePath))
                    {
                        Log.WriteError($"Solution file not found at: {options.SolutionFilePath}");
                        return;
                    }

                    traceListener.VerboseConsoleLogging = options.VerboseConsoleLogging;
                    if (!String.IsNullOrEmpty(options.LogFilePath))
                    {
                        traceListener.EnableLogFile(options.LogFilePath);
                    }

                    var configParser = JsonConfigurationParser.FromFile(options.ConfigFilePath);
                    using (var engine = new StylizeEngine(configParser))
                    {
                        engine.RunAsync(options.SolutionFilePath).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteError($"Exception occurred: {ex}");
                    throw;
                }
            }
        }

        class Options
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Used by CommandLineParser through reflection.")]
            [Option('c', "config", Required = true, HelpText = "File path of the Stylize JSON configuration.")]
            public string ConfigFilePath { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Used by CommandLineParser through reflection.")]
            [Option('l', "log", HelpText = "File path of the output log.")]
            public string LogFilePath { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Used by CommandLineParser through reflection.")]
            [Option('s', "solution", Required = true, HelpText = "File path of the solution on which to enforce style rules.")]
            public string SolutionFilePath { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Used by CommandLineParser through reflection.")]
            [Option('v', "verbose", HelpText = "Enable verbose logging mode to the console.")]
            public bool VerboseConsoleLogging { get; set; }

            [HelpOption]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Used by CommandLineParser through reflection.")]
            public string GetUsage()
            {
                var helpText = new HelpText("Reformats source code per configured style rules.")
                {
                    AddDashesToOption = true,
                    MaximumDisplayWidth = 80
                };

                helpText.AddPreOptionsLine("Usage information:");
                helpText.AddOptions(this);

                return helpText;
            }
        }
    }
}
