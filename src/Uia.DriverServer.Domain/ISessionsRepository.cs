﻿using System.Collections.Generic;

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
        /// Deletes the session with the specified ID.
        /// </summary>
        /// <param name="id">The session identifier.</param>
        /// <returns>An HTTP status code indicating the result of the operation.</returns>
        int DeleteSession(string id);

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
        /// Retrieves all active sessions.
        /// </summary>
        /// <returns>A tuple containing the status code and a collection of all session models.</returns>
        (int StatusCode, IEnumerable<UiaSessionResponseModel> Sessions) GetSessions();

        /// <summary>
        /// Retrieves the handle of the currently focused window for the specified session.
        /// </summary>
        /// <param name="id">The ID of the session (currently unused).</param>
        /// <returns>A tuple containing the status code and the handle of the focused window as a hexadecimal string.</returns>
        public (int StatusCode, string Handle) GetHandle(string id);

        /// <summary>
        /// Retrieves all window handles.
        /// </summary>
        /// <param name="id">The ID of the session (currently unused).</param>
        /// <returns>A tuple containing the status code and a list of window handles.</returns>
        (int StatusCode, IEnumerable<string> Handles) GetHandles(string id);

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
        /// Sends User32 key scan codes by converting the provided text into scan codes and delegating to the main SendUser32Keys method.
        /// </summary>
        /// <param name="automation">The <see cref="CUIAutomation8"/> instance used for automation operations.</param>
        /// <param name="textData">The <see cref="TextInputModel"/> containing the text to convert to scan codes and the associated options.</param>
        /// <returns>An integer status code indicating the result of sending the keys.</returns>
        int SendUser32Keys(CUIAutomation8 automation, TextInputModel textData);

        /// <summary>
        /// Sends User32 key scan codes based on the provided input model.
        /// </summary>
        /// <param name="keyScansData">The <see cref="ScanCodesInputModel"/> containing the scan codes and options such as keyboard layout, delay, and sticky keys flag.</param>
        /// <returns>Returns an integer status code (200 indicates success).</returns>
        int SendUser32Keys(CUIAutomation8 automation, ScanCodesInputModel keyScansData);

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

        /// <summary>
        /// Switches focus to a window specified by its handle or name within a given session.
        /// </summary>
        /// <param name="id">The ID of the session.</param>
        /// <param name="windowHandleOrName">The handle or name of the window to switch to.</param>
        /// <returns>Status code indicating the result of the operation.</returns>
        int SwitchWindow(string id, string windowHandleOrName);
    }
}
