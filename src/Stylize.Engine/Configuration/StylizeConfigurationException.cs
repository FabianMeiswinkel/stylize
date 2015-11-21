// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Stylize.Engine.Configuration
{
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "Default constructor excluded to require an exception message")]
    public class StylizeConfigurationException : Exception
    {
        public StylizeConfigurationException(string message)
            : base(message)
        {
        }

        public StylizeConfigurationException(string message, Exception exception)
            : base(message, exception)
        {
        }

        protected StylizeConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
