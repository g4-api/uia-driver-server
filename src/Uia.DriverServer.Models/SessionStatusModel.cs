/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the status model for a WebDriver session.
    /// </summary>
    public class SessionStatusModel
    {
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>The status message indicating the current state of the session.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the operating system information represented as a dictionary of key-value pairs.
        /// </summary>
        [JsonPropertyName("os")]
        public IDictionary<string, object> OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session is ready.
        /// </summary>
        /// <value><c>true</c> if the session is ready; otherwise, <c>false</c>.</value>
        public bool Ready { get; set; }

        /// <summary>
        /// Gets or sets the collection of session identifiers.
        /// </summary>
        /// <value>A collection of session identifiers.</value>
        public IEnumerable<string> Sessions { get; set; }
    }
}
