// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Stylize.Engine.Configuration;

namespace Stylize.Engine
{
    static class CompositionContainerExtensions
    {
        public static IReadOnlyList<Export<TPart, TMetadata>> GetExports<TPart, TMetadata>(
            this CompositionContainer container, IReadOnlyDictionary<string, IOptionApplier> nameOptionMap)
            where TMetadata : INamedMetadata
        {
            var exports = new List<Export<TPart, TMetadata>>();

            IReadOnlyList<Lazy<TPart, TMetadata>> allExports = container.GetExports<TPart, TMetadata>().ToList();
            foreach (KeyValuePair<string, IOptionApplier> nameOptionPair in nameOptionMap)
            {
                Lazy<TPart, TMetadata> export = allExports.FirstOrDefault(
                    e => nameOptionPair.Key.Equals(e.Metadata.Name));
                if (export == null)
                {
                    throw new StylizeConfigurationException(
                        $"Unable to find export of type {typeof(TPart)} with name '{nameOptionPair.Key}'");
                }

                exports.Add(new Export<TPart, TMetadata>(export.Value, export.Metadata, nameOptionPair.Value));
            }

            return exports;
        }
    }
}
