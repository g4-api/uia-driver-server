﻿/*
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
        IDictionary<string, UiaSessionResponseModel> Sessions { get; }

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
        /// <param name="newSessionModel">The capabilities for the new session.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Entity: The created session object.
        /// </returns>
        (int StatusCode, object Entity) NewSession(NewSessionModel newSessionModel);

        /// <summary>
        /// Deletes the session with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <returns>An HTTP status code indicating the result of the operation.</returns>
        int DeleteSession(string id);

        /// <summary>
        /// Retrieves all active sessions.
        /// </summary>
        /// <returns>A tuple containing the status code and a collection of all session models.</returns>
        (int StatusCode, IEnumerable<UiaSessionResponseModel> Sessions) GetSessions();

        /// <summary>
        /// Retrieves the screenshot of the current window.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Entity: The screenshot as a base64 string.
        /// </returns>
        (int StatusCode, string Entity) NewScreenshot();

        /// <summary>
        /// Gets the session with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Session: The session model.
        /// </returns>
        (int StatusCode, UiaSessionResponseModel Session) GetSession(string id);

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
