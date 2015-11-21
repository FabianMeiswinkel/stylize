// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json.Linq;

namespace Stylize.Engine.Configuration
{
    class JsonOptionApplier : IOptionApplier
    {
        readonly IDictionary<OptionKey, object> optionValues;

        public JsonOptionApplier(
            string featureName, IDictionary<string, JToken> jsonOptions, IReadOnlyList<IOption> supportedOptions)
        {
            this.optionValues = new Dictionary<OptionKey, object>();
            foreach (KeyValuePair<string, JToken> jsonOption in jsonOptions)
            {
                IOption option = supportedOptions.FirstOrDefault(
                    o => o.Feature.Equals(featureName) && o.Name.Equals(jsonOption.Key));
                if (option == null)
                {
                    throw new StylizeConfigurationException(
                        $"Option {featureName}:{jsonOption.Key} in the configuration file does not match any exported options.");
                }

                this.optionValues.Add(new OptionKey(option), jsonOption.Value.ToObject(option.Type));
            }
        }

        public OptionSet ApplyOptions(OptionSet options)
        {
            return this.optionValues.Aggregate(
                options,
                (newOptions, optionValue) => newOptions.WithChangedOption(optionValue.Key, optionValue.Value));
        }
    }
}
