using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Marshals.Models;
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
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
                _logger?.LogWarning(e, "Failed to kill application for session with ID {SessionId}.", id);
            }

            // Remove the session from the sessions dictionary
            Sessions.Remove(id);

            // Log the successful deletion of the session
            _logger?.LogInformation("Session with ID {SessionId} and application {ApplicationName} deleted successfully.", id, name);

            // Return a 200 OK status code indicating successful deletion
            return StatusCodes.Status200OK;
        }

        /// <inheritdoc />
        public (int StatusCode, string Handle) GetHandle(string id)
        {
            // Get the handle and name of the current focused window.
            var (handle, name) = User32.GetFocusedWindowHandle();

            // If the handle is not valid, return a 404 Not Found status code.
            if (handle == IntPtr.Zero)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // Create an object containing the handle and name of the window.
            var handleObject = new
            {
                Handle = handle.ToString("X"),
                Name = name
            };

            // Serialize the handle object to a JSON string.
            var handleJson = JsonSerializer.Serialize(handleObject, s_jsonOptions);

            // Return a 200 OK status code with the JSON string.
            return (StatusCodes.Status200OK, handleJson);
        }

        /// <inheritdoc />
        public (int StatusCode, IEnumerable<string> Handles) GetHandles(string id)
        {
            // Return status code 200 (OK) and the list of window handles.
            return (StatusCodes.Status200OK, User32.GetHandles());
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
            var sessionModels = Sessions.Values;

            _logger?.LogInformation("Retrieved {SessionCount} active sessions.", sessionModels.Count);

            // Return a 200 OK status code and the collection of session models
            return (StatusCodes.Status200OK, sessionModels);
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
            // Initialize a dictionary to hold the merged capabilities from "alwaysMatch" and "firstMatch".
            var matchCapabilities = new Dictionary<string, object>();

            // Check if "alwaysMatch" capabilities are empty and if "firstMatch"
            // capabilities are not empty to determine which capabilities to use for session creation.
            var isAlwaysMatchEmpty = newSessionModel.Capabilities.AlwaysMatch == null || newSessionModel.Capabilities.AlwaysMatch.Count == 0;
            var isFirstMatchEmpty = newSessionModel.Capabilities.FirstMatch == null || newSessionModel.Capabilities.FirstMatch.Length == 0;

            // If "alwaysMatch" capabilities are empty and "firstMatch" capabilities are not empty,
            // use the first set of "firstMatch" capabilities for session creation.
            if (isAlwaysMatchEmpty && !isFirstMatchEmpty)
            {
                matchCapabilities = newSessionModel.Capabilities.FirstMatch[0];
            }
            if(!isAlwaysMatchEmpty)
            {
                matchCapabilities = newSessionModel.Capabilities.AlwaysMatch;
            }

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
        public int SendUser32Keys(CUIAutomation8 automation, TextInputModel textData)
        {
            // Create a new ScanCodesInputModel using the text from textData.
            // Each character in the text is converted to a string to form the scan codes array.
            var keyScansData = new ScanCodesInputModel
            {
                // Convert each character from textData.Text into a string.
                ScanCodes = textData.Text.Select(i => $"{i}"),

                // Copy the input options from textData.
                Options = textData.Options
            };

            // Delegate the key sending operation to the overloaded SendUser32Keys method.
            return SendUser32Keys(automation, keyScansData);
        }

        /// <inheritdoc />
        public int SendUser32Keys(CUIAutomation8 automation, ScanCodesInputModel keyScansData)
        {
            // Local method to send key inputs for non-sticky keys.
            static void Send(CUIAutomation8 automation, string keyboardLayout, IEnumerable<string> input, int delay)
            {
                // Iterate over each scan code.
                foreach (var character in input)
                {
                    // Convert the character to down key input.
                    var down = character.ConvertToInputs(keyboardLayout, KeyEvent.KeyDown).ToArray();

                    // Convert the character to up key input.
                    var up = character.ConvertToInputs(keyboardLayout, KeyEvent.KeyUp).ToArray();

                    // Send both "down" and "up" inputs together.
                    automation.SendInputs([.. down, .. up]);

                    // Pause between each key input. If delay is 0, default to 100 milliseconds.
                    Thread.Sleep(delay == 0 ? 100 : delay);
                }
            }

            // Local method to send key inputs for sticky keys (press and hold, then release all at once).
            static void SendSticky(CUIAutomation8 automation, IEnumerable<ushort> wScans, int delay)
            {
                // Iterate over each scan code to send the "key down" actions.
                foreach (var wScan in wScans)
                {
                    // Convert the scan code to a "key down" input.
                    var down = wScan.ConvertToInput(KeyEvent.KeyDown | KeyEvent.Scancode);

                    // Send only the "down" input.
                    automation.SendInputs(down);

                    // Pause between key down actions. Default to 100 milliseconds if delay is 0.
                    Thread.Sleep(delay == 0 ? 100 : delay);
                }

                // Iterate over each scan code in reverse order to send the "key up" actions.
                foreach (var wScan in wScans.Reverse())
                {
                    // Convert the scan code to a "key up" input.
                    var up = wScan.ConvertToInput(KeyEvent.KeyUp | KeyEvent.Scancode);

                    // Send the "up" input to release the key.
                    automation.SendInputs(up);
                }
            }

            // Set the keyboard layout to the specified layout.
            User32.SwitchKeyboardLayout(keyScansData.Options.KeyboardLayout);

            // Convert each key scan string from the input model to its corresponding scan code using the provided keyboard layout.
            var wScans = keyScansData.ScanCodes.Select(i => i.GetScanCode(layout: keyScansData.Options.KeyboardLayout));

            // Depending on whether sticky keys are enabled, call the appropriate sending method.
            if (keyScansData.Options.StickyKeys)
            {
                SendSticky(automation, wScans, delay: keyScansData.Options.Delay);
            }
            else
            {
                Send(
                    automation,
                    keyboardLayout: keyScansData.Options.KeyboardLayout,
                    input: keyScansData.ScanCodes,
                    delay: keyScansData.Options.Delay);
            }

            // Return a success status code.
            return 204;
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

        /// <inheritdoc />
        public int SwitchWindow(string id, string windowHandleOrName)
        {
            // Attempt to retrieve the session from the sessions dictionary
            var session = Sessions[id];

            // Check if the window handle or name is numeric to determine the type of search to perform (by handle or name).
            var isNumeric = int.TryParse(windowHandleOrName, out int windowHandle);

            // Initialize the window handle as an IntPtr.
            IntPtr hwnd;

            // If the window handle is numeric, initialize the window handle as an IntPtr from the numeric value.
            // Otherwise, find the window by its name.
            if (isNumeric)
            {
                // Initialize the window handle as an IntPtr from the numeric value.
                hwnd = new IntPtr(windowHandle);

                // Check if the window handle is valid.
                if (!User32.AssertWindow(hwnd))
                {
                    return 400;
                }
            }
            else
            {
                hwnd = User32.FindWindowByName(windowHandleOrName);
            }

            // Get the UI Automation element from the window handle. 
            // If the element is not found, return a 404 Not Found status code.
            IUIAutomationElement element;
            try
            {
                element = session.Automation.ElementFromHandle(hwnd);
            }
            catch
            {
                return StatusCodes.Status404NotFound;
            }

            // Set focus to the element if it exists.
            element?.SetFocus();

            // Return the appropriate status code based on whether the element was found.
            return element == null
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status200OK;
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
