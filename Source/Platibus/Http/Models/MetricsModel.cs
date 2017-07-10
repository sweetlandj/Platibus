using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Platibus.Http.Models
{
    /// <summary>
    /// A model representing metrics
    /// </summary>
    [Serializable]
    public class MetricsModel : Dictionary<string, long>
    {
        /// <summary>
        /// Initializes a new <see cref="MetricsModel"/>
        /// </summary>
        public MetricsModel()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MetricsModel"/> from serialized data
        /// </summary>
        /// <param name="info">The serialized information</param>
        /// <param name="context">The streaming context</param>
        protected MetricsModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
