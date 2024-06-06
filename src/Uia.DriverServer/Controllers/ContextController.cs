/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
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
    [ApiController]
    [Route("wd/hub"), Route("/")]
    public class ContextController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        [HttpPost]
        [Route("session/{id}/window/maximize")]
        [SwaggerResponse(200, "Window maximized successfully.", typeof(RectangleModel))]
        [SwaggerResponse(404, "Invalid session id.")]
        [SwaggerResponse(500, "Internal server error.")]
        [SwaggerOperation(
            Summary = "Maximizes the window for the specified session.",
            Description = "Sets the window state to maximized for the given session id.")]
        public IActionResult WindowMaximize(
            [FromRoute][SwaggerParameter(Description = "The unique identifier for the session in which the window will be maximized.")] string id)
        {
            // Set the window state to maximized for the given session id.
            var session = _domain
                .SessionsRepository
                .SetWindowVisualState(id, WindowVisualState.WindowVisualState_Maximized);

            // Return the result as JSON with the appropriate status code and content type.
            return new JsonResult(value: session.Entity)
            {
                StatusCode = session.StatusCode,
                ContentType = MediaTypeNames.Application.Json
            };
        }
    }
}
