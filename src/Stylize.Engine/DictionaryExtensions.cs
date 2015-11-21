// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;

namespace Stylize.Engine
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> buildValue)
        {
            if (dictionary == null) { throw new ArgumentNullException(nameof(dictionary)); }
            if (buildValue == null) { throw new ArgumentNullException(nameof(buildValue)); }

            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = buildValue(key);
                dictionary[key] = value;
            }

            return value;
        }

        public static TValue GetValue<TKey, TValue>(
            this IDictionary<TKey, object> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null) { throw new ArgumentNullException(nameof(dictionary)); }

            object value;
            if (dictionary.TryGetValue(key, out value))
            {
                return (TValue)value;
            }

            return defaultValue;
        }
    }
}
