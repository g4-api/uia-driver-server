/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver/#sessions
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    /// <summary>
    /// Controller for handling session-related actions in UI Automation.
    /// </summary>
    /// <param name="domain">The UIA domain interface.</param>
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation session-related actions",
        externalDocsUrl: "https://www.w3.org/TR/webdriver/#sessions")]
    public class SessionsController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // DELETE wd/hub/session/{session}
        // DELETE session/{session}
        [HttpDelete]
        [Route("session/{session}")]
        [SwaggerOperation(
            Summary = "Deletes the specified session.",
            Description = "Removes the session identified by the given session ID from the system.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Session deleted successfully.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to delete the session.")]
        public IActionResult DeleteSession(
            [FromRoute][SwaggerParameter(Description = "The unique identifier for the session to be deleted.")] string session)
        {
            // Delete the session using the domain's sessions repository
            var statusCode = _domain.SessionsRepository.DeleteSession(session);

            // Return the status code result indicating the outcome of the delete operation
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = statusCode
            };
        }

        // DELETE wd/hub/session
        // DELETE session
        [HttpDelete]
        [Route("session")]
        [SwaggerOperation(
            Summary = "Deletes all active sessions.",
            Description = "Removes all active sessions from the system.",
            Tags = ["Sessions"])]
        [SwaggerResponse(204, "All sessions deleted successfully.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to delete the sessions.")]
        public IActionResult DeleteSessions()
        {
            // Iterate over all session IDs and attempt to delete each one
            foreach (var id in _domain.SessionsRepository.Sessions.Keys)
            {
                try
                {
                    _domain.SessionsRepository.DeleteSession(id);
                }
                catch
                {
                    // Ignore any exceptions during session deletion
                }
            }

            // Return a 200 OK status code indicating all sessions were deleted
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        // GET wd/hub/session/{session}/dom
        // GET session/{session}/dom
        [HttpGet]
        [Route("session/{session}/dom")]
        [SwaggerOperation(
            Summary = "Gets the Document Object Model (DOM) for the specified session.",
            Description = "Retrieves the DOM for the session identified by the given session ID.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "DOM retrieved successfully.", typeof(string))]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the DOM.")]
        public IActionResult GetDocumentObjectModel(
            [SwaggerParameter(Description = "The unique identifier for the session.")][FromRoute] string session)
        {
            // Retrieve the DOM for the specified session
            var (statusCode, objectModel) = _domain.DocumentRepository.GetPageSource(session);

            // Prepare the content based on the status code
            var content = statusCode == StatusCodes.Status200OK
                ? objectModel
                : $"<Desktop><Error>Session with ID {session} not found.</Error></Desktop>";

            // Return the result as XML content with the appropriate status code
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Xml,
                StatusCode = statusCode
            };
        }

        // POST /wd/hub/session/{session}/element/dom
        // POST /session/{session}/element/dom
        [HttpPost]
        [Route("session/{session}/element/dom")]
        [SwaggerOperation(
            Summary = "Creates the Document Object Model (DOM) for a specific element within a session.",
            Description = "Finds the specified element using the provided locator strategy, then generates the DOM for that element.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "DOM created successfully.", typeof(string))]
        [SwaggerResponse(400, "Invalid request. The locator strategy model is not valid.")]
        [SwaggerResponse(404, "Element not found. The session ID or locator strategy provided does not match any element.")]
        [SwaggerResponse(500, "Internal server error. An unexpected error occurred while attempting to find the element.")]
        public IActionResult GetDocumentObjectModel(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element will be found.")][FromRoute] string session,
            [SwaggerRequestBody(Description = "The locator strategy model containing the strategy to find the element.")][FromBody] LocationStrategyModel locator)
        {
            // Attempt to find the element in the specified session using the provided locator strategy.
            var (findStatusCode, elementModel) = _domain.ElementsRepository.FindElement(session, locator);

            // If the element is not found, return a 404 Not Found response with an appropriate error message.
            if (findStatusCode == StatusCodes.Status404NotFound || elementModel == null)
            {
                return NotFound(WebDriverResponseModel.NewElementNotFoundResponse(locator, session));
            }

            // Create a new Document Object Model (DOM) for the found element.
            var (domStatusCode, objectModel) = _domain.DocumentRepository.GetElementSource(elementModel);

            // Prepare the XML content to return based on the status of the DOM creation.
            var content = domStatusCode == StatusCodes.Status200OK
                ? objectModel
                : $"<Desktop><Error>Session with ID {session} not found.</Error></Desktop>";

            // Return the generated XML content along with the appropriate HTTP status code.
            return new ContentResult
            {
                Content = content,
                ContentType = MediaTypeNames.Application.Xml,
                StatusCode = domStatusCode
            };
        }

        // GET wd/hub/status
        // GET /status
        [HttpGet]
        [Route("status")]
        [SwaggerOperation(
            Summary = "Gets the status of the current sessions.",
            Description = "Retrieves the current status of sessions, indicating whether new sessions can be created.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Status retrieved successfully.", typeof(SessionStatusModel))]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the status.")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetStatus()
        {
            // Retrieve the current session keys
            var sessions = _domain.SessionsRepository.Sessions.Keys;

            // Determine if the sessions stack is full
            var isFull = sessions.Count > 0;

            // Prepare the status message based on the current session count
            var message = isFull
                ? "Current sessions stack is full, the maximum allowed sessions number is 1"
                : "Sessions slots available, can create new session";

            // Return the status as a JSON result
            return new JsonResult(new
            {
                Value = new SessionStatusModel
                {
                    Message = message,
                    OperatingSystem = new Dictionary<string, object>
                    {
                        ["arch"] = $"{RuntimeInformation.OSArchitecture}",
                        ["name"] = RuntimeInformation.OSDescription,
                        ["version"] = $"{Environment.OSVersion.Version}"
                    },
                    Ready = !isFull,
                    Sessions = sessions
                }
            });
        }

        // GET wd/hub/session/{session}/window
        // GET session/{session}/window
        [HttpGet]
        [Route("session/{session}/window")]
        [SwaggerOperation(
            Summary = "Gets the handle of the currently focused window for the specified session.",
            Description = "Retrieves the handle of the currently focused window for the session identified by the given session ID.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Window handle retrieved successfully.", typeof(string))]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the window handle.")]
        public IActionResult GetWindowHandle(
            [SwaggerParameter(Description = "The unique identifier for the session.")][FromRoute] string session)
        {
            // Get the handle of the currently focused window
            var (statusCode, handle) = _domain.SessionsRepository.GetHandle(session);

            // Prepare the response based on the status code
            var response = statusCode == StatusCodes.Status404NotFound
                ? new WebDriverResponseModel(session, new Dictionary<string, string> { ["error"] = "no such window" })
                : new WebDriverResponseModel(session, value: handle);

            // Return the result as a JSON response with the appropriate status code
            return new JsonResult(response)
            {
                StatusCode = statusCode
            };
        }

        // GET wd/hub/session/{session}/window/handles
        // GET session/{session}/window/handles
        [HttpGet]
        [Route("session/{session}/window/handles")]
        [SwaggerOperation(
            Summary = "Gets all window handles for the specified session.",
            Description = "Retrieves all window handles for the session identified by the given session ID.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Window handles retrieved successfully.", typeof(IEnumerable<string>))]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the window handles.")]
        public IActionResult GetWindowHandles(
            [SwaggerParameter(Description = "The unique identifier for the session.")][FromRoute] string session)
        {
            // Get the window handles for the specified session
            var (statusCode, value) = _domain.SessionsRepository.GetHandles(id: session);

            // Return the window handles as a JSON result with the appropriate status code
            return new JsonResult(new WebDriverResponseModel(session, value))
            {
                StatusCode = statusCode
            };
        }

        // POST wd/hub/session
        // POST session
        [HttpPost]
        [Route("session")]
        [SwaggerOperation(
            Summary = "Creates a new session with the specified capabilities.",
            Description = "Initializes a new session using the provided capabilities and returns the session details.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Session created successfully.", typeof(UiaSessionResponseModel))]
        [SwaggerResponse(400, "Invalid request. The capabilities model is not valid.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while creating the session.")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult NewSession(
            [SwaggerRequestBody(Description = "The capabilities required to create the session.")] NewSessionModel capabilities)
        {
            // Create a new session using the domain's sessions repository
            var (statusCode, session) = _domain.SessionsRepository.NewSession(capabilities);

            // Return the result as JSON with the appropriate status code
            return new JsonResult(session)
            {
                StatusCode = statusCode
            };
        }

        // POST wd/hub/session/{session}/window
        // POST session/{session}/window
        [HttpPost]
        [Route("session/{session}/window")]
        [SwaggerOperation(
            Summary = "Switches to the specified window in the session.",
            Description = "Sets the focus to the specified window handle or name within the given session.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Switched to window successfully.")]
        [SwaggerResponse(400, "Invalid argument. The window handle or name is not valid.")]
        [SwaggerResponse(404, "No such window. The window handle or name does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while switching the window.")]
        public IActionResult SwitchWindow(
            [SwaggerParameter(Description = "The unique identifier for the session.")][FromRoute] string session,
            [SwaggerRequestBody(Description = "The request model containing the window handle or name to switch to.")] WindowHandleRequestModel requestModel)
        {
            // Switch to the specified window within the session
            var statusCode = _domain.SessionsRepository.SwitchWindow(id: session, requestModel.Handle);

            // Prepare the response value based on the status code result of the operation
            var errorCode = string.Empty;

            // Determine the error code based on the status code
            if (statusCode == StatusCodes.Status404NotFound)
            {
                errorCode = "no such window";
            }
            else if (statusCode == StatusCodes.Status400BadRequest)
            {
                errorCode = "invalid argument";
            }

            // Prepare the response value
            var value = statusCode != 200
                ? new Dictionary<string, string> { ["error"] = errorCode }
                : null;

            // Return the result as a JSON response with the appropriate status code
            return new JsonResult(new WebDriverResponseModel(session, value))
            {
                StatusCode = statusCode
            };
        }
    }
}
