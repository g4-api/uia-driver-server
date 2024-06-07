/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Runtime.Serialization;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the response model for WebDriver operations.
    /// </summary>
    [DataContract]
    public class WebDriverResponseModel
    {
        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>The unique identifier for the session.</value>
        [DataMember]
        public string Session { get; set; }

        /// <summary>
        /// Gets or sets the value of the response.
        /// </summary>
        /// <value>The value returned by the WebDriver operation.</value>
        [DataMember]
        public object Value { get; set; }
    }
}
