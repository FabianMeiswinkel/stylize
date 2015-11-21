// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stylize.Engine.Configuration
{
    public class JsonConfigurationParser : IConfigurationParser
    {
        readonly Configuration configuration;

        JsonConfigurationParser(string json)
        {
            if (json == null) { throw new ArgumentNullException(nameof(json)); }

            this.configuration = JsonConvert.DeserializeObject<Configuration>(json);
            this.ExportAssemblyNames = new List<string>(this.configuration.ExportAssemblyNames);
        }

        public IReadOnlyList<string> ExportAssemblyNames { get; }

        static IDictionary<string, IOptionApplier> BuildOptionDictionary(
            IDictionary<string, IDictionary<string, JToken>> jsonOptions, IReadOnlyList<IOption> supportedOptions)
        {
            return jsonOptions.ToDictionary(
                o => o.Key,
                o => (IOptionApplier)new JsonOptionApplier(o.Key, o.Value, supportedOptions));
        }

        public static JsonConfigurationParser FromFile(string configFilePath)
        {
            if (configFilePath == null) { throw new ArgumentNullException(nameof(configFilePath)); }

            return new JsonConfigurationParser(File.ReadAllText(configFilePath));
        }

        public static JsonConfigurationParser FromString(string json)
        {
            return new JsonConfigurationParser(json);
        }

        public StylizeConfiguration ParseConfiguration(IReadOnlyList<IOption> supportedOptions)
        {
            IOptionApplier globalOptions = new JsonOptionApplier(
                GlobalOptions.Name, this.configuration.GlobalOptions, supportedOptions);

            return new StylizeConfiguration(
                exclusionMatchers: BuildOptionDictionary(this.configuration.ExclusionMatchers, supportedOptions),
                globalOptions: globalOptions,
                rules: BuildOptionDictionary(this.configuration.Rules, supportedOptions),
                repositoryName: this.configuration.RepositoryName);
        }

        [JsonObject]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
            Justification = "Instantiated by JSON.NET")]
        class Configuration
        {
            [JsonProperty("exclusionMatchers")]
            public IDictionary<string, IDictionary<string, JToken>> ExclusionMatchers { get; } =
                new Dictionary<string, IDictionary<string, JToken>>();

            [JsonProperty("exportAssemblies")]
            public IList<string> ExportAssemblyNames { get; } = new List<string>();

            [JsonProperty("globalOptions")]
            public IDictionary<string, JToken> GlobalOptions { get; } = new Dictionary<string, JToken>();

            [JsonProperty("repository")]
            public string RepositoryName { get; set; }

            [JsonProperty("rules")]
            public IDictionary<string, IDictionary<string, JToken>> Rules { get; } =
                new Dictionary<string, IDictionary<string, JToken>>();
        }
    }
}
