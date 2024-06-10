/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the response model for a session.
    /// </summary>
    public class SessionResponseModel
    {
        /// <summary>
        /// Gets or sets the capabilities of the session.
        /// </summary>
        /// <value>A dictionary containing the session capabilities.</value>
        public IDictionary<string, object> Capabilities { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the session.
        /// </summary>
        /// <value>The unique identifier for the session.</value>
        public string SessionId { get; set; }
    }
}
