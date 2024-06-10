/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver/#screen-capture
 */
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    /// <summary>
    /// Controller for handling screen capture actions in UI Automation.
    /// </summary>
    /// <param name="domain">The UIA domain interface.</param>
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation screen capture actions",
        externalDocsUrl: "https://www.w3.org/TR/webdriver/#screen-capture")]
    public class ScreenCaptureController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // GET /wd/hub/session/{id}/screenshot
        // GET /session/{id}/screenshot
        [HttpGet]
        [Route("session/{id}/screenshot")]
        [SwaggerOperation(
            Summary = "Captures a screenshot for the specified session.",
            Description = "Takes a screenshot of the screen for the session identified by the given session ID.",
            Tags = ["ScreenCapture"])]
        [SwaggerResponse(200, "Screenshot captured successfully.", typeof(WebDriverResponseModel))]
        [SwaggerResponse(500, "Internal server error. An error occurred while capturing the screenshot.")]
        public IActionResult GetScreenshot(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string id)
        {
            // Capture the screenshot using the domain's sessions repository
            var (statusCode, screenshot) = _domain.SessionsRepository.NewScreenshot();

            // Prepare the response value containing the screenshot data
            var value = new WebDriverResponseModel
            {
                Session = id,
                Value = screenshot
            };

            // Return the result as JSON with the appropriate status code
            return new JsonResult(value)
            {
                StatusCode = statusCode
            };
        }
    }
}
