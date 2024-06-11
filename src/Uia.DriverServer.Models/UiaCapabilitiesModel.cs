/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the UI Automation (UIA) capabilities.
    /// </summary>
    public class NewSessionModel
    {
        [Required]
        public CapabilitiesModel Capabilities { get; set; }

        /// <summary>
        /// The capabilities which will bare to communicate the features supported
        /// by a given implementation.
        /// </summary>
        public class CapabilitiesModel
        {
            /// <summary>
            /// Initialize a new instance of CapabilitiesModel object.
            /// </summary>
            public CapabilitiesModel()
            {
                AlwaysMatch = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                FirstMatch = [];
            }

            /// <summary>
            /// Gets or sets a collection of capabilities that must all be matched.
            /// </summary>
            public Dictionary<string, object> AlwaysMatch { get; set; }

            /// <summary>
            /// Gets or sets a collection of capabilities that at least one must be matched.
            /// </summary>
            public Dictionary<string, object>[] FirstMatch { get; set; }
        }
    }
}
