/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver/#elements
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Net.Mime;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Extensions;
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
            Tags = ["Elements", "Retrieval"])]
        [SwaggerResponse(200, "Element found successfully.", typeof(object))]
        [SwaggerResponse(400, "Invalid request. The locator strategy model is not valid.")]
        [SwaggerResponse(404, "Element not found. The session ID or locator strategy provided does not match any element.")]
        [SwaggerResponse(500, "Internal server error. An unexpected error occurred while attempting to find the element.")]
        public IActionResult FindElement(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element will be found.")] string session,
            [SwaggerRequestBody(Description = "The locator strategy model containing the strategy to find the element.")] LocationStrategyModel locator)
        {
            try
            {
                // Find the element using the domain's elements repository
                var (statusCode, elementModel) = _domain.ElementsRepository.FindElement(session, locator);

                // Check if the element was not found
                if (statusCode == StatusCodes.Status404NotFound || elementModel == null)
                {
                    // Return a not found response if the element was not found
                    return NotFound();
                }

                // Prepare the response value containing the element reference
                var value = new WebDriverResponseModel
                {
                    Session = session,
                    Value = new Dictionary<string, string>
                    {
                        [ElementProperties.ElementReference] = elementModel.Id
                    }
                };

                // Return the result as JSON with the appropriate status code
                return new JsonResult(value)
                {
                    StatusCode = statusCode
                };
            }
            catch (Exception e)
            {
                // Get the base exception from the error stack trace for logging purposes and error handling
                var baseException = e.GetBaseException();

                // Log the exception (not shown here) and return a 500 error response
                var value = new WebDriverResponseModel
                {
                    Session = session,
                    Value = $"{baseException}"
                };

                // Return the result as JSON with the appropriate status code
                return new JsonResult(value)
                {
                    StatusCode = baseException is FormatException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError
                };
            }
        }

        // POST /wd/hub/session/{session}/element/{element}/element
        // POST /session/{session}/element/{element}/element
        [HttpPost]
        [Route("session/{session}/element/{element}/element")]
        [SwaggerOperation(
            Summary = "Finds an element within the specified element in the given session using the provided locator strategy.",
            Description = "Uses the locator strategy to find an element within the parent element identified by the given session and element IDs.",
            Tags = ["Elements"])]
        [SwaggerResponse(200, "Element found successfully.", typeof(WebDriverResponseModel))]
        [SwaggerResponse(400, "Invalid request. The locator strategy model is not valid.")]
        [SwaggerResponse(404, "Element not found. The session ID, parent element ID, or locator strategy provided does not match any element.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to find the element.")]
        public IActionResult FindElement(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the parent element within which to find the target element.")] string element,
            [SwaggerRequestBody(Description = "The locator strategy model containing the strategy to find the element.")] LocationStrategyModel locator)
        {
            try
            {
                // Find the element using the domain's elements repository
                var (statusCode, elementModel) = _domain.ElementsRepository.FindElement(session, element, locator);

                // Check if the element was not found
                if (statusCode == StatusCodes.Status404NotFound || elementModel == null)
                {
                    // Return a not found response if the element was not found
                    return NotFound();
                }

                // Prepare the response value containing the element reference
                var value = new WebDriverResponseModel
                {
                    Session = session,
                    Value = new Dictionary<string, string>
                    {
                        [ElementProperties.ElementReference] = elementModel.Id
                    }
                };

                // Return the result as JSON with the appropriate status code
                return new JsonResult(value)
                {
                    StatusCode = statusCode
                };
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while attempting to find the element: {e}");
            }
        }

        // POST /wd/hub/session/{session}/element/{element}/click
        // POST /session/{session}/element/{element}/click
        [HttpGet]
        [Route("session/{session}/element/{element}/attribute/{name}")]
        [SwaggerOperation(
            Summary = "Gets the specified attribute of the specified element in the given session.",
            Description = "Retrieves the value of the attribute identified by the given session, element, and attribute name.",
            Tags = ["Elements", "State"])]
        [SwaggerResponse(200, "Element attribute retrieved successfully.", typeof(WebDriverResponseModel))]
        [SwaggerResponse(404, "Element, session, or attribute not found. The session ID, element ID, or attribute name provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the element attribute.")]
        public IActionResult GetElementAttribute(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element's attribute will be retrieved.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element whose attribute will be retrieved.")] string element,
            [SwaggerParameter(Description = "The name of the attribute to retrieve.")] string name)
        {
            // Retrieve the element attribute using the domain's elements repository
            var (statusCode, text) = _domain.ElementsRepository.GetElementAttribute(session, element, name);

            // Prepare the response value containing the element's attribute
            var value = new WebDriverResponseModel
            {
                Value = text
            };

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
            Tags = ["Elements", "State"])]
        [SwaggerResponse(200, "Element text retrieved successfully.", typeof(WebDriverResponseModel))]
        [SwaggerResponse(404, "Element or session not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while retrieving the element text.")]
        public IActionResult GetElementText(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element's text will be retrieved.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element whose text will be retrieved.")] string element)
        {
            // Retrieve the element text using the domain's elements repository
            var (statusCode, text) = _domain
                .ElementsRepository
                .GetElementText(session, element);

            // Prepare the response value containing the element's text
            var value = new WebDriverResponseModel
            {
                Value = text
            };

            // Return the result as JSON with the appropriate status code
            return new JsonResult(value)
            {
                StatusCode = statusCode
            };
        }

        // POST /wd/hub/session/{session}/element/{element}/click
        // POST /session/{session}/element/{element}/click
        [HttpPost]
        [Route("session/{session}/element/{element}/click")]
        [SwaggerOperation(
            Summary = "Invokes a click action on the specified element in the given session.",
            Description = "Performs a click action on the element identified by the given session and element IDs.",
            Tags = ["Elements", "Interaction"])]
        [SwaggerResponse(200, "Click action invoked successfully.")]
        [SwaggerResponse(404, "Element or session not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to click the element.")]
        public IActionResult InvokeClick(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element will be clicked.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be clicked.")] string element)
        {
            // Get the session status code
            var statusCode = _domain.SessionsRepository.GetSession(id: session).StatusCode;

            // Check if the session status code is not OK
            if (statusCode != StatusCodes.Status200OK)
            {
                // Return the status code result if the session is not found
                return new StatusCodeResult(statusCode);
            }

            // Retrieve the element using the domain's elements repository
            var elementModel = _domain.ElementsRepository.GetElement(session, element);

            // Check if the element was not found
            if (elementModel == null)
            {
                // Return a not found response if the element was not found
                return NotFound();
            }

            // Invoke the click action on the element
            elementModel.UIAutomationElement.Invoke();

            // Return an OK response indicating the click action was successful
            return Ok();
        }

        // POST /wd/hub/session/{session}/element/{element}/value
        // POST /session/{session}/element/{element}/value
        [HttpPost]
        [Route("session/{session}/element/{element}/value")]
        [SwaggerOperation(
            Summary = "Sets the value of the specified element in the given session.",
            Description = "Updates the value of the element identified by the given session and element IDs with the provided data.",
            Tags = ["Elements", "Interaction"])]
        [SwaggerResponse(200, "Element value set successfully.")]
        [SwaggerResponse(400, "Invalid request. The data does not contain a valid 'text' property.")]
        [SwaggerResponse(404, "Element or session not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to set the element value.")]
        public IActionResult SetValue(
            [SwaggerParameter(Description = "The unique identifier for the session in which the element's value will be set.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element whose value will be set.")] string element,
            [SwaggerRequestBody(Description = "The data containing the value to set, including the 'text' key with the value to be set.")] TextInputModel textData)
        {
            try
            {
                // Get the session status code
                var statusCode = _domain.SessionsRepository.GetSession(id: session).StatusCode;

                // Check if the session status code is not OK
                if (statusCode != StatusCodes.Status200OK)
                {
                    // Return the status code result if the session is not found
                    return new StatusCodeResult(statusCode);
                }

                // Retrieve the element using the domain's elements repository
                var elementModel = _domain.ElementsRepository.GetElement(session, element);

                // Check if the element was not found
                if (elementModel == null || elementModel.UIAutomationElement == null)
                {
                    // Return a not found response if the element was not found
                    return NotFound("Element or session not found.");
                }

                // Set the value of the element
                elementModel.UIAutomationElement.SendKeys($"{textData.Text}");

                // Return an OK response indicating the value was set successfully
                return Ok();
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while attempting to set the element value: {e.Message}");
            }
        }
    }
}
