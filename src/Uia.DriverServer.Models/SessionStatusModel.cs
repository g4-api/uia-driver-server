/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the status model for a WebDriver session.
    /// </summary>
    [DataContract]
    public class SessionStatusModel
    {
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>The status message indicating the current state of the session.</value>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session is ready.
        /// </summary>
        /// <value><c>true</c> if the session is ready; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool Ready { get; set; }

        /// <summary>
        /// Gets or sets the collection of session identifiers.
        /// </summary>
        /// <value>A collection of session identifiers.</value>
        [DataMember]
        public IEnumerable<string> Sessions { get; set; }
    }
}
