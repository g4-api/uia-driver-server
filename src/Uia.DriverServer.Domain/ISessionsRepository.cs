/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;
using System.Xml.Linq;

using Uia.DriverServer.Models;

using UIAutomationClient;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Defines methods for managing sessions in a Windows desktop automation context.
    /// </summary>
    public interface ISessionsRepository
    {
        /// <summary>
        /// Gets the collection of active sessions.
        /// </summary>
        IDictionary<string, UiaSessionModel> Sessions { get; }

        /// <summary>
        /// Creates a new session XML document with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - ElementsXml: The XML document representing the session elements.
        /// </returns>
        (int StatusCode, XDocument ElementsXml) NewDocumentObjectModel(string id);

        /// <summary>
        /// Creates a new session with the specified capabilities.
        /// </summary>
        /// <param name="capabilities">The capabilities for the new session.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Entity: The created session object.
        /// </returns>
        (int StatusCode, object Entity) NewSession(UiaCapabilitiesModel capabilities);

        /// <summary>
        /// Deletes the session with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <returns>An HTTP status code indicating the result of the operation.</returns>
        int DeleteSession(string id);

        /// <summary>
        /// Retrieves the screenshot of the current window.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Entity: The screenshot as a base64 string.
        /// </returns>
        (int StatusCode, string Entity) GetScreenshot();

        /// <summary>
        /// Gets the session with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Session: The session model.
        /// </returns>
        (int StatusCode, UiaSessionModel Session) GetSession(string id);

        /// <summary>
        /// Sets the visual state of the window for the session with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <param name="visualState">The desired window visual state.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Entity: The model representing the window rectangle.
        /// </returns>
        (int StatusCode, RectangleModel Entity) SetWindowVisualState(string id, WindowVisualState visualState);
    }
}
