// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine.Matchers
{
    public abstract class RegexDocumentMatcher : IDocumentMatcher
    {
        readonly Option<string> patternOption;
        readonly Dictionary<string, Regex> regexCache = new Dictionary<string, Regex>();

        protected RegexDocumentMatcher(Option<string> patternOption)
        {
            this.patternOption = patternOption;
        }

        protected abstract string SelectValue(Document document);

        public Task<bool> IsMatchAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            string pattern = document.GetOption(this.patternOption);
            if (String.IsNullOrEmpty(pattern))
            {
                return Task.FromResult(false);
            }

            Regex regex = this.regexCache.GetOrAdd(pattern, p => new Regex(p, RegexOptions.Compiled));

            string value = this.SelectValue(document);
            return Task.FromResult(regex.IsMatch(value));
        }
    }
}
