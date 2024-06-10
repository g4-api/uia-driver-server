/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;
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

        /// <summary>
        /// Creates a new WebDriver response model for a located element.
        /// </summary>
        /// <param name="session">The session identifier.</param>
        /// <param name="id">The unique identifier of the located element.</param>
        /// <returns>A <see cref="WebDriverResponseModel"/> representing the response with the element reference.</returns>
        public static WebDriverResponseModel NewElementResponse(string session, string id)
        {
            // Create the response model containing the session ID and the element reference
            return new WebDriverResponseModel
            {
                Session = session,
                Value = new Dictionary<string, string>
                {
                    [ElementProperties.ElementReference] = id
                }
            };
        }

        /// <summary>
        /// Creates a new WebDriver response model indicating that an element was not found.
        /// </summary>
        /// <param name="locationStrategy">The location strategy used to find the element.</param>
        /// <param name="session">The session identifier.</param>
        /// <returns>A <see cref="WebDriverResponseModel"/> representing the error response.</returns>
        public static WebDriverResponseModel NewNotFoundResponse(LocationStrategyModel locationStrategy, string session)
        {
            // Create the error model indicating that the element was not found
            var error = new WebDriverErrorModel
            {
                Error = "no such element",
                Message = $"Unable to locate element: {{\"method\": \"{locationStrategy.Using}\", \"selector\":\"{locationStrategy.Value}\"}}"
            };

            // Create the response model containing the error and the session ID
            return new WebDriverResponseModel
            {
                Session = session,
                Value = error
            };
        }

        /// <summary>
        /// Creates a new WebDriver response model indicating a stale element reference error.
        /// </summary>
        /// <param name="session">The session identifier.</param>
        /// <returns>A <see cref="WebDriverResponseModel"/> representing the stale reference error response.</returns>
        public static WebDriverResponseModel NewStaleReferenceResponse(string session)
        {
            // Create the error model indicating that the element is not attached to the application document
            var error = new WebDriverErrorModel
            {
                Error = "stale element reference",
                Message = $"stale element reference: element is not attached to the application document (Session info: UiaDriver={session})"
            };

            // Create the response model containing the error and the session ID
            return new WebDriverResponseModel
            {
                Session = session,
                Value = error
            };
        }

    }
}
