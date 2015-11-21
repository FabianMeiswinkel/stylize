// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using Stylize.Engine.Configuration;

namespace Stylize.Engine
{
    class Export<TPart, TMetadata>
    {
        public Export(TPart part, TMetadata metadata, IOptionApplier optionApplier)
        {
            this.Part = part;
            this.Metadata = metadata;
            this.OptionApplier = optionApplier;
        }

        public TMetadata Metadata { get; }

        public IOptionApplier OptionApplier { get; }

        public TPart Part { get; }
    }
}
