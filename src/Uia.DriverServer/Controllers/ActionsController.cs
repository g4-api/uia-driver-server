/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation User32-related actions",
        externalDocsUrl: "https://www.w3.org/TR/webdriver/#actions")]
    public class ActionsController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // POST session/{session}/actions
        [HttpPost]
        [Route("session/{session}/actions")]
        [SwaggerOperation(
            Summary = "Sends User32 actions to the specified session.",
            Description = "Performs User32-based actions in the session identified by the given session ID. These actions interact directly with native Windows User32 APIs.",
            Tags = ["User32, Actions"])]
        [SwaggerResponse(200, "Actions sent successfully.")]
        [SwaggerResponse(400, "Invalid request. The provided action data is not valid.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to send the actions.")]
        public IActionResult SendActions(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The data containing the User32 actions to be performed.")] ActionsModel data)
        {
            // Get the session model using the domain's session repository
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Send the User32 actions to the session
            _domain.ActionsRepository.SendActions(sessionModel, data);

            // Return an OK response indicating the actions were sent successfully
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/element/{element}/actions")]
        public IActionResult InvokeCopy(string session, string element, [FromBody] IDictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
