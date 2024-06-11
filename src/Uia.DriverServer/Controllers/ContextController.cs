/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 * https://www.w3.org/TR/webdriver/#contexts
 */
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

using UIAutomationClient;

namespace Uia.DriverServer.Controllers
{
    /// <summary>
    /// Controller for handling UI Automation context-related actions.
    /// </summary>
    /// <param name="domain">The UIA domain interface.</param>
    [Produces(MediaTypeNames.Application.Json)]
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation context-related actions",
        externalDocsUrl: "https://www.w3.org/TR/webdriver/#contexts")]
    public class ContextController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // POST /wd/hub/session/{session}/window/maximize
        // POST /session/{session}/window/maximize
        [HttpPost]
        [Route("session/{session}/window/maximize")]
        [SwaggerOperation(
            Summary = "Maximizes the window for the specified session.",
            Description = "Sets the window state to maximized for the given session id.",
            Tags = ["Context"])]
        [SwaggerResponse(200, "Window maximized successfully.", typeof(RectangleModel))]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An unexpected error occurred while attempting to maximize the window.")]
        public IActionResult InvokeWindowMaximize(
            [SwaggerParameter(Description = "The unique identifier for the session in which the window will be maximized.")] string session)
        {
            // Set the window state to maximized for the given session id.
            var (statusCode, rectangle) = _domain
                .SessionsRepository
                .SetWindowVisualState(session, WindowVisualState.WindowVisualState_Maximized);

            // Return the result as JSON with the appropriate status code and content type.
            return new JsonResult(new WebDriverResponseModel(session, rectangle))
            {
                StatusCode = statusCode
            };
        }
    }
}
