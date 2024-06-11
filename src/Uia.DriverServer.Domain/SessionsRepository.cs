/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Models;

using UIAutomationClient;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Manages the repository of UI Automation sessions.
    /// </summary>
    /// <param name="sessions">The dictionary of UI Automation sessions.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    public class SessionsRepository(IDictionary<string, UiaSessionResponseModel> sessions, ILogger<SessionsRepository> logger) : ISessionsRepository
    {
        // Initialize the logger instance for logging information and errors in the repository class methods and properties
        private readonly ILogger<SessionsRepository> _logger = logger;

        // Initialize the JSON serializer options for deserializing capabilities into UiaOptions instances
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <inheritdoc />
        public IDictionary<string, UiaSessionResponseModel> Sessions { get; } = sessions;

        /// <inheritdoc />
        public int DeleteSession(string id)
        {
            // Attempt to retrieve the session from the sessions dictionary
            if (!Sessions.TryGetValue(id, out UiaSessionResponseModel session))
            {
                // Log a warning if the session is not found
                _logger?.LogError("Session with ID {SessionId} not found.", id);

                // Return a 404 Not Found status code indicating the session was not found
                return StatusCodes.Status404NotFound;
            }

            // Get the application name from the session
            var name = session?.Application?.StartInfo.FileName;

            try
            {
                // Attempt to kill the application process associated with the session
                session?.Application?.Kill(entireProcessTree: true);
                _logger?.LogInformation("Killed application for session with ID {SessionId}. Application: {ApplicationName}", id, name);
            }
            catch (Exception e)
            {
                // Log a warning if an exception occurs while killing the process
                _logger.LogWarning(e, "Failed to kill application for session with ID {SessionId}.", id);
            }

            // Remove the session from the sessions dictionary
            Sessions.Remove(id);

            // Log the successful deletion of the session
            _logger?.LogInformation("Session with ID {SessionId} and application {ApplicationName} deleted successfully.", id, name);

            // Return a 204 No Content status code indicating successful deletion
            return StatusCodes.Status204NoContent;
        }

        /// <inheritdoc />
        public (int StatusCode, UiaSessionResponseModel Session) GetSession(string id)
        {
            // Attempt to retrieve the session from the sessions dictionary
            if (Sessions.TryGetValue(id, out UiaSessionResponseModel session))
            {
                return (StatusCodes.Status200OK, session);
            }
            else
            {
                _logger?.LogError("Session with ID {SessionId} not found.", id);
                return (StatusCodes.Status404NotFound, default);
            }
        }

        /// <inheritdoc />
        public (int StatusCode, IEnumerable<UiaSessionResponseModel> Sessions) GetSessions()
        {
            // Retrieve all session models from the sessions dictionary
            var sessions = Sessions.Values;

            _logger?.LogInformation("Retrieved {SessionCount} active sessions.", sessions.Count());

            // Return a 200 OK status code and the collection of session models
            return (StatusCodes.Status200OK, sessions);
        }

        /// <inheritdoc />
        public (int StatusCode, XDocument ElementsXml) NewDocumentObjectModel(string id)
        {
            // Attempt to retrieve the session from the sessions dictionary
            if (!Sessions.TryGetValue(id, out UiaSessionResponseModel session))
            {
                _logger?.LogInformation("Session with ID {SessionId} not found.", id);
                return (StatusCodes.Status404NotFound, default);
            }

            // Create a new Document Object Model (DOM) for the session's application root
            var elementsXml = DocumentObjectModelFactory.New(session.ApplicationRoot);

            _logger?.LogInformation("New Document Object Model created for session with ID {SessionId}.", id);

            // Return a 200 OK status code and the XML document representing the DOM
            return (StatusCodes.Status200OK, elementsXml);
        }

        /// <inheritdoc />
        public (int StatusCode, string Entity) NewScreenshot()
        {
            // Capture a new bitmap image
            var bitmap = new OcrRepository().NewBitmap();

            // Use a memory stream to save the bitmap in PNG format
            using MemoryStream ms = new();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            // Convert the bitmap to a base64 encoded string
            var base64 = Convert.ToBase64String(ms.ToArray());

            _logger?.LogInformation("Screenshot captured and converted to base64 string.");

            // Return a 200 OK status code and the base64 encoded string
            return (StatusCodes.Status200OK, base64);
        }

        // TODO: Imeplement capabilites merging https://www.w3.org/TR/webdriver/#capabilities
        /// <inheritdoc />
        public (int StatusCode, object Entity) NewSession(NewSessionModel newSessionModel)
        {
            // Deserialize the AlwaysMatch capability to a dictionary
            var matchCapabilities = newSessionModel.Capabilities.AlwaysMatch.Count == 0
                ? newSessionModel.Capabilities.FirstMatch[0]
                : newSessionModel.Capabilities.AlwaysMatch;

            // Deserialize the options from the capabilities or create a new UiaOptions instance if not present
            var options = matchCapabilities.TryGetValue(key: UiaCapabilities.Options, out object optionsOut) && optionsOut != null
                ? JsonSerializer.Deserialize<UiaOptionsModel>($"{optionsOut}", s_jsonOptions)
                : new UiaOptionsModel();

            // Check if the capabilities contain an application key and if it has a non-empty value
            var isApp = !string.IsNullOrEmpty(options.App);
            var isDesktop = !isApp;

            // Create a new session based on the application type
            var (statusCode, response, session) = !isApp || isDesktop
                ? NewDesktopSession(matchCapabilities)
                : NewApplicationSession(options, matchCapabilities);

            // Check if session creation was successful
            if (statusCode == StatusCodes.Status200OK)
            {
                // Log the successful session creation
                _logger.LogInformation("Session created successfully with Session ID: {SessionId}", session.SessionId);

                // Add the new session to the sessions dictionary
                Sessions[session.SessionId] = session;
            }
            else
            {
                // Log the failed session creation
                _logger.LogError("Failed to create session. StatusCode: {StatusCode}", statusCode);
            }

            // Return the status code and response entity
            return (statusCode, response);
        }

        /// <inheritdoc />
        public (int StatusCode, RectangleModel Entity) SetWindowVisualState(string id, WindowVisualState visualState)
        {
            _logger?.LogInformation("Setting window visual state for session with ID {SessionId} to {VisualState}.", id, visualState);

            // Attempt to retrieve the session from the sessions dictionary
            if (!Sessions.TryGetValue(id, out UiaSessionResponseModel session))
            {
                _logger?.LogWarning("Session with ID {SessionId} not found.", id);
                return (StatusCodes.Status404NotFound, new RectangleModel());
            }

            // Attempt to set the window visual state
            if (session.ApplicationRoot.GetCurrentPattern(UIA_PatternIds.UIA_WindowPatternId) is IUIAutomationWindowPattern pattern)
            {
                try
                {
                    pattern.SetWindowVisualState(visualState);
                    _logger?.LogInformation("Window visual state set to {VisualState} for session with ID {SessionId}.", visualState, id);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "Failed to set window visual state for session with ID {SessionId}.", id);
                    throw;
                }
            }
            else
            {
                _logger?.LogWarning("Window pattern not available for session with ID {SessionId}.", id);
                return (StatusCodes.Status500InternalServerError, new RectangleModel());
            }

            // Return the current bounding rectangle of the application root
            return (StatusCode: StatusCodes.Status200OK, Entity: session.ApplicationRoot.GetRectangle());
        }

        // Creates a new application session with the specified capabilities.
        private static (int StatusCode, WebDriverResponseModel Response, UiaSessionResponseModel Session) NewApplicationSession(UiaOptionsModel options, IDictionary<string, object> capabilities)
        {
            // Get the application file name from the capabilities
            var fileName = options.App;
            var arguments = options.Arguments ?? [];

            // Start or mount the process based on the options
            var process = options.Mount
                ? Array.Find(Process.GetProcesses(), i => fileName.Contains(i.ProcessName, StringComparison.OrdinalIgnoreCase))
                : UiaUtilities.StartProcess(options.Impersonation, fileName, string.Join(" ", arguments), options.WorkingDirectory);

            // Check if the process has exited or is invalid
            if (process?.HasExited != false)
            {
                return (StatusCodes.Status500InternalServerError, default, default);
            }

            // Check if the process has a main window handle, a handle, or is invalid or closed
            // before continuing with the session creation process
            if (process.MainWindowHandle == default && process.Handle == default && (process.SafeHandle.IsInvalid || process.SafeHandle.IsClosed))
            {
                return (StatusCodes.Status500InternalServerError, default, default);
            }

            // Create a new UIA session model
            var session = new UiaSessionResponseModel(new CUIAutomation8(), process)
            {
                Capabilities = capabilities,
                TreeScope = TreeScope.TreeScope_Children,
                ScaleRatio = options.ScaleRatio
            };

            // Create the WebDriver response model
            var response = new WebDriverResponseModel
            {
                Value = new SessionResponseModel
                {
                    SessionId = session.SessionId,
                    Capabilities = capabilities
                }
            };

            // Return the status code, response model, and session model
            return (StatusCodes.Status200OK, response, session);
        }

        // Creates a new desktop session with the specified capabilities.
        private static (int StatusCode, object Response, UiaSessionResponseModel Session) NewDesktopSession(IDictionary<string, object> capabilities)
        {
            // Generate a new unique identifier for the session
            var id = Guid.NewGuid();

            // Create a new UIA session model
            var session = new UiaSessionResponseModel
            {
                SessionId = $"{id}",
                Capabilities = capabilities,
                Runtime = [],
                TreeScope = TreeScope.TreeScope_Children
            };

            // Create the response object
            var response = new
            {
                Value = new
                {
                    SessionId = id,
                    Capabilities = capabilities
                }
            };

            // Return the status code, response object, and session model
            return (StatusCodes.Status200OK, response, session);
        }
    }
}
