/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://learn.microsoft.com/en-us/windows/win32/apiindex/windows-api-list
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    /// <summary>
    /// Controller for handling User32-related actions in UI Automation.
    /// </summary>
    /// <param name="domain">The UIA domain interface.</param>
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation User32-related actions",
        externalDocsUrl: "https://learn.microsoft.com/en-us/windows/win32/apiindex/windows-api-list")]
    public class User32Controller(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        // POST wd/hub/user32/session/{session}/element/{element}/click
        // POST user32/session/{session}/element/{element}/click
        [HttpPost]
        [Route("session/user32/{session}/element/{element}/click")]
        [SwaggerOperation(
            Summary = "Invokes a click action on the specified element in the given session.",
            Description = "Performs a click action on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Click action invoked successfully.")]
        [SwaggerResponse(404, "Session or element not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to click the element.")]
        public IActionResult InvokeClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be clicked.")] string element)
        {
            // Get the session status code
            var (statusCode, sessionModel) = _domain.SessionsRepository.GetSession(id: session);

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

            // Check if the UIAutomationElement property of the element model is null
            if (elementModel.UIAutomationElement == null)
            {
                // Get the clickable point of the element based on the session's scale ratio
                var point = elementModel.GetClickablePoint(sessionModel.ScaleRatio);

                // Send a native click to the computed point using the session's automation object
                sessionModel.Automation.SendNativeClick(point);
            }
            else
            {
                // Send a native click to the element using its UIAutomationElement property and the session's scale ratio
                elementModel.UIAutomationElement.SendNativeClick(sessionModel.ScaleRatio);
            }

            // Return an OK response indicating the click action was successful
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/element/{element}/copy
        // POST user32/session/{session}/element/{element}/copy
        [HttpPost]
        [Route("session/user32/{session}/element/{element}/copy")]
        [SwaggerOperation(
            Summary = "Invokes the copy action on the specified element in the given session.",
            Description = "Performs a copy action on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Copy action invoked successfully.")]
        [SwaggerResponse(404, "Session or element not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to copy the element.")]
        public IActionResult InvokeCopy(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be copied.")] string element)
        {
            // Get the session status code
            var (statusCode, sessionModel) = _domain.SessionsRepository.GetSession(id: session);

            // Check if the session status code is not OK
            if (statusCode != StatusCodes.Status200OK)
            {
                // Return the status code result if the session is not found
                return new StatusCodeResult(statusCode);
            }

            // Retrieve the element using the domain's elements repository
            var elementModel = _domain.ElementsRepository.GetElement(session, element);

            // Check if the element was not found
            if (elementModel.UIAutomationElement == null)
            {
                // Return a not found response if the element was not found
                return NotFound();
            }

            try
            {
                // Select the element in the UI Automation framework
                elementModel.UIAutomationElement.Select();
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"{e}");
            }

            // Perform the copy action using the session's automation service
            sessionModel.Automation.SendModifiedKey(modifier: "Ctrl", key: "C");

            // Return an OK response indicating the copy action was successful
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/dclick
        // POST user32/session/{session}/dclick
        [HttpPost]
        [Route("wd/hub/user32/session/{session}/dclick")]
        [Route("user32/session/{session}/dclick")]
        [SwaggerOperation(
            Summary = "Invokes a double-click action at the specified coordinates in the given session.",
            Description = "Performs a double-click action at the coordinates specified by the point parameter in the session identified by the given session ID.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Double-click action invoked successfully.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to perform the double-click action.")]
        public IActionResult InvokeDoubleClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The coordinates where the double-click should be performed.")] PointModel point)
        {
            // Get the session status code
            var (statusCode, sessionModel) = _domain.SessionsRepository.GetSession(id: session);

            // Check if the session status code is not OK
            if (statusCode != StatusCodes.Status200OK)
            {
                // Return the status code result if the session is not found
                return new StatusCodeResult(statusCode);
            }

            // Send a native double-click to the specified coordinates using the session's automation object
            sessionModel.Automation.SendNativeClick(point, repeat: 2);

            // Return an OK response indicating the double-click action was successful
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/element/{element}/dclick
        // POST user32/session/{session}/element/{element}/dclick
        [HttpPost]
        [Route("session/user32/{session}/element/{element}/dclick")]
        [SwaggerOperation(
            Summary = "Invokes a double-click action on the specified element in the given session.",
            Description = "Performs a double-click action on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Double-click action invoked successfully.")]
        [SwaggerResponse(404, "Session or element not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to perform the double-click action.")]
        public IActionResult InvokeDoubleClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be double-clicked.")] string element)
        {
            // Get the session status code and model
            var (statusCode, sessionModel) = _domain.SessionsRepository.GetSession(id: session);

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

            // Check if the UIAutomationElement property of the element model is null
            if (elementModel.UIAutomationElement == null)
            {
                // Get the clickable point of the element based on the session's scale ratio
                var point = elementModel.GetClickablePoint(sessionModel.ScaleRatio);

                // Send a native double-click to the computed point using the session's automation object
                sessionModel.Automation.SendNativeClick(point, repeat: 2);
            }
            else
            {
                // Send a native double-click to the element using its UIAutomationElement
                // property and the session's scale ratio
                elementModel.UIAutomationElement.SendNativeClick(
                    align: default,
                    repeat: 2,
                    scaleRatio: sessionModel.ScaleRatio);
            }

            // Return an OK response indicating the double-click action was successful
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/paste
        // POST user32/session/{session}/paste
        [HttpPost]
        [Route("session/user32/{session}/paste")]
        [SwaggerOperation(
            Summary = "Invokes a paste action in the specified session.",
            Description = "Performs a paste action in the session identified by the given session ID.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Paste action invoked successfully.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to perform the paste action.")]
        public IActionResult InvokePaste(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session)
        {
            // Get the session status code and model
            var (statusCode, sessionModel) = _domain.SessionsRepository.GetSession(id: session);

            // Check if the session status code is not OK
            if (statusCode != StatusCodes.Status200OK)
            {
                // Return the status code result if the session is not found
                return new StatusCodeResult(statusCode);
            }

            try
            {
                // Perform the paste action using the session's automation service
                sessionModel.Automation.SendModifiedKey(modifier: "Ctrl", key: "V");
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while attempting to paste: {e.Message}");
            }

            // Return an OK response indicating the paste action was successful
            return Ok();
        }
    }
}
