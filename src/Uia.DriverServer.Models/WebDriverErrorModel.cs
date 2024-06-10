/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the error model for WebDriver responses.
    /// </summary>
    public class WebDriverErrorModel
    {
        /// <summary>
        /// Gets or sets the error data.
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets the error type.
        /// </summary>
        /// <value>The type of error.</value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message describing the issue.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace of the error.
        /// </summary>
        /// <value>The stack trace providing details about where the error occurred.</value>
        public string StackTrace { get; set; }
    }
}
