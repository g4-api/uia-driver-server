/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver/#sessions
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Net.Mime;

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

        // DELETE wd/hub/session/{id}
        // DELETE session/{id}
        [HttpDelete]
        [Route("session/{id}")]
        [SwaggerOperation(
            Summary = "Deletes the specified session.",
            Description = "Removes the session identified by the given session ID from the system.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Session deleted successfully.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to delete the session.")]
        public IActionResult DeleteSession(
            [FromRoute][SwaggerParameter(Description = "The unique identifier for the session to be deleted.")] string id)
        {
            // Delete the session using the domain's sessions repository
            var statusCode = _domain.SessionsRepository.DeleteSession(id);

            // Return the status code result indicating the outcome of the delete operation
            return new StatusCodeResult(statusCode);
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

            // Return a 204 No Content status code indicating all sessions were deleted
            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // GET wd/hub/session/{id}
        // GET session/{id}
        [HttpGet]
        [Route("session/{id}")]
        [SwaggerOperation(
            Summary = "Gets the Document Object Model (DOM) for the specified session.",
            Description = "Retrieves the DOM for the session identified by the given session ID.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "DOM retrieved successfully.", typeof(string))]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the DOM.")]
        public IActionResult GetDocumentObjectModel(
            [SwaggerParameter(Description = "The unique identifier for the session.")][FromRoute] string id)
        {
            try
            {
                // Retrieve the DOM for the specified session
                var (statusCode, objectModel) = _domain.SessionsRepository.NewDocumentObjectModel(id);

                // Prepare the content based on the status code
                var content = statusCode == StatusCodes.Status200OK
                    ? objectModel.ToString()
                    : $"<Desktop><Error>Session with ID {id} not found.</Error></Desktop>";

                // Return the result as XML content with the appropriate status code
                return new ContentResult
                {
                    Content = content,
                    ContentType = MediaTypeNames.Application.Xml,
                    StatusCode = statusCode
                };
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"{e}");
            }
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
            try
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
                return new JsonResult(new SessionStatusModel
                {
                    Message = message,
                    Ready = !isFull,
                    Sessions = sessions
                });
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"{e}");
            }
        }

        // POST wd/hub/session
        // POST session
        [HttpPost]
        [Route("session")]
        [SwaggerOperation(
            Summary = "Creates a new session with the specified capabilities.",
            Description = "Initializes a new session using the provided capabilities and returns the session details.",
            Tags = ["Sessions"])]
        [SwaggerResponse(200, "Session created successfully.", typeof(UiaSessionModel))]
        [SwaggerResponse(400, "Invalid request. The capabilities model is not valid.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while creating the session.")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult NewSession(
            [SwaggerRequestBody(Description = "The capabilities required to create the session.")] UiaCapabilitiesModel capabilities)
        {
            // Create a new session using the domain's sessions repository
            var (statusCode, session) = _domain.SessionsRepository.NewSession(capabilities);

            // Return the result as JSON with the appropriate status code
            return new JsonResult(session)
            {
                StatusCode = statusCode
            };
        }
    }
}
