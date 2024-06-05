/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the UI Automation (UIA) capabilities.
    /// </summary>
    public class UiaCapabilitiesModel
    {
        /// <summary>
        /// Gets or sets a dictionary of capabilities where the key is a string
        /// representing the capability name, and the value is an object representing
        /// the capability value.
        /// </summary>
        public IDictionary<string, object> Capabilities { get; set; }
    }
}
