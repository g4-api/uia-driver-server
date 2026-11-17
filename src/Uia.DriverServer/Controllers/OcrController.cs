/*
 * CHANGE LOG - keep only last 5 threads
 *
 * RESOURCES
 * https://github.com/g4-api/ocr-inspector
 */
#if RELEASE_EMGU || DEBUG_EMGU
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(description: Catalog.Tag)]
    public class OcrController(IUiaDomain domain) : ControllerBase
    {
        #region *** Fields       ***

        // Holds the domain facade that owns screen capture and recognition for every request of this controller.
        private readonly IUiaDomain _domain = domain;

        #endregion

        #region *** Methods      ***

        // GET /wd/hub/session/{session}/ocr
        // GET /session/{session}/ocr
        [HttpGet]
        [Route("session/{session}/ocr")]
        [SwaggerOperation(
            Summary = Catalog.GetSnapshot.Summary,
            Description = Catalog.GetSnapshot.Description,
            Tags = [Catalog.TagName])]
        [SwaggerResponse(StatusCodes.Status200OK, Catalog.GetSnapshot.Responses.Ok, typeof(WebDriverResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Catalog.GetSnapshot.Responses.InternalServerError)]
        public IActionResult GetSnapshot(
            [SwaggerParameter(Description = Catalog.GetSnapshot.Parameters.Session)] string session)
        {
            // Capture the screen and recognize its words in one step so the image and the words always agree.
            var snapshot = _domain.OcrRepository.NewSnapshot();

            // Wrap the snapshot in the standard WebDriver envelope used by every session-scoped endpoint.
            var value = new WebDriverResponseModel
            {
                Session = session,
                Value = snapshot
            };

            return new JsonResult(value)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        #endregion

        #region *** Nested Types ***

        // Holds the published wording of every action of this controller. The text is wire contract: clients
        // receive it through the OpenAPI document.
        private static class Catalog
        {
            public const string TagName = "Ocr";
            public const string Tag = "Controller for OCR (Optical Character Recognition) inspection";

            // Wording of the GetSnapshot action.
            public static class GetSnapshot
            {
                public const string Summary = "Captures the screen and returns it with the OCR words recognized on it.";
                public const string Description = "Takes one screenshot of the primary display, runs the driver's OCR " +
                    "engine on it, and returns the image as a Base64-encoded PNG together with every recognized word, " +
                    "its rectangle, confidence, occurrence index, and ready-to-use OCR locator.";

                // Parameter wording, one constant per action parameter named after the parameter in PascalCase.
                public static class Parameters
                {
                    public const string Session = "The unique identifier for the session.";
                }

                // Response wording, one constant per status named after the status.
                public static class Responses
                {
                    public const string Ok = "Snapshot captured and recognized successfully.";
                    public const string InternalServerError = "Internal server error. An error occurred while " +
                        "capturing the screen or recognizing its text.";
                }
            }
        }

        #endregion
    }
}
#endif
