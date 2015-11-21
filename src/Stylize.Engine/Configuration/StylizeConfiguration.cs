// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Collections.Generic;

namespace Stylize.Engine.Configuration
{
    public class StylizeConfiguration
    {
        public StylizeConfiguration(
            IDictionary<string, IOptionApplier> exclusionMatchers,
            IOptionApplier globalOptions,
            IDictionary<string, IOptionApplier> rules,
            string repositoryName)
        {
            this.ExclusionMatchers = new Dictionary<string, IOptionApplier>(exclusionMatchers);
            this.GlobalOptions = globalOptions;
            this.Rules = new Dictionary<string, IOptionApplier>(rules);
            this.RepositoryName = repositoryName;
        }

        public IReadOnlyDictionary<string, IOptionApplier> ExclusionMatchers { get; }

        public IOptionApplier GlobalOptions { get; }

        public string RepositoryName { get; }

        public IReadOnlyDictionary<string, IOptionApplier> Rules { get; }
    }
}
