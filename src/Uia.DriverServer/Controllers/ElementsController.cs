/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver/#elements
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System.Collections.Generic;
using System.Net.Mime;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    /// <summary>
    /// Controller for handling element-related actions in UI Automation.
    /// </summary>
    /// <param name="domain">The UIA domain interface.</param>
    [Produces(MediaTypeNames.Application.Json)]
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation element-related actions",
        externalDocsUrl: "https://www.w3.org/TR/webdriver/#elements")]
    public class ElementsController(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // POST /wd/hub/session/{session}/element
        // POST /session/{session}/element
        [HttpPost]
        [Route("session/{session}/element")]
        [SwaggerOperation(
            Summary = "Finds an element in the specified session using the provided locator strategy.",
            Description = "Uses the locator strategy to find an element in the session identified by the given session ID.",
            Tags = ["Elements"])]
        [SwaggerResponse(200, "Element found successfully.", typeof(Dictionary<string, string>))]
        [SwaggerResponse(400, "Invalid request. The locator strategy model is not valid.")]
        [SwaggerResponse(404, "Element not found. The session ID or locator strategy provided does not match any element.")]
        [SwaggerResponse(500, "Internal server error. An unexpected error occurred while attempting to find the element.")]
        public IActionResult FindElement(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element will be found.")] string session,
            [SwaggerRequestBody(Description = "The locator strategy model containing the strategy to find the element.")] LocationStrategyModel locator)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Return a bad request response if the model state is not valid
                return BadRequest(ModelState);
            }

            // Find the element using the domain's elements repository
            var (statusCode, element) = _domain.ElementsRepository.FindElement(session, locator);

            // Check if the element was not found
            if (statusCode == StatusCodes.Status404NotFound || element == null)
            {
                // Return a not found response if the element was not found
                return NotFound();
            }

            // Prepare the response value containing the element reference
            var value = new Dictionary<string, string> { [ElementProperties.ElementReference] = element.Id };

            // Return the result as JSON with the appropriate status code
            return new JsonResult(value)
            {
                StatusCode = statusCode
            };
        }

        // GET /wd/hub/session/{session}/element/{element}/text
        // GET /session/{session}/element/{element}/text
        [HttpGet]
        [Route("session/{session}/element/{element}/text")]
        [SwaggerOperation(
            Summary = "Gets the text of the specified element in the given session.",
            Description = "Retrieves the text content of the element identified by the given session and element IDs.",
            Tags = ["Elements"])]
        [SwaggerResponse(200, "Element text retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "Element or session not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the element text.")]
        public IActionResult GetElementText(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element's text will be retrieved.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element whose text will be retrieved.")] string element)
        {
            // Retrieve the element text using the domain's elements repository
            var (statusCode, text) = _domain
                .ElementsRepository
                .GetElementText(session: session, element: element);

            // Prepare the response value containing the element's text
            var value = new
            {
                Value = text
            };

            // Return the result as JSON with the appropriate status code
            return new JsonResult(value)
            {
                StatusCode = statusCode
            };
        }
    }
}
