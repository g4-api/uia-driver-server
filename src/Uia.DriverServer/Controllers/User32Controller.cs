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
using System.Collections.Generic;
using System.Linq;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals.Models;
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

        // POST wd/hub/user32/session/{session}/element/{element}/select
        // POST user32/session/{session}/element/{element}/select
        [HttpPost]
        [Route("user32/session/{session}/element/{element}/select")]
        [SwaggerOperation(
            Summary = "Selects the specified element in the given session.",
            Description = "Performs a select action on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Element selected successfully.")]
        [SwaggerResponse(404, "Element or session not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to select the element.")]
        public IActionResult SelectElement(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be selected.")] string element)
        {
            try
            {
                // Retrieve the element based on the provided session ID and element ID
                var elementModel = _domain.ElementsRepository.GetElement(session, element);

                // Check if the element was not found
                if (elementModel == null || elementModel.UIAutomationElement == null)
                {
                    // Return a not found response if the element was not found
                    return NotFound("Element or session not found.");
                }

                // Select the UI automation element
                elementModel.UIAutomationElement.Select();

                // Return an OK response indicating the select action was successful
                return Ok();
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while attempting to select the element: {e}");
            }
        }

        // POST wd/hub/user32/session/{session}/value
        // POST user32/session/{session}/value
        [HttpPost]
        [Route("user32/session/{session}/value")]
        [SwaggerOperation(
            Summary = "Sends keystrokes to the specified session.",
            Description = "Performs a send keys action in the session identified by the given session ID with the provided keystrokes.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Keystrokes sent successfully.")]
        [SwaggerResponse(400, "Invalid request. The data does not contain a valid 'text' key.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to send the keystrokes.")]
        public IActionResult SendKeys(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The data containing the keystrokes to send, including the 'text' key with the keystrokes to send.")] IDictionary<string, object> data)
        {
            // Get the session model using the domain's session repository
            var sessionModel = _domain.GetSession(session);

            // Check if the data contains the "text" key
            var isText = data.TryGetValue(key: "text", out object textValue);

            // Return a bad request response if the "text" key is not present
            if (!isText)
            {
                return BadRequest("The data does not contain a valid 'text' key.");
            }

            // Convert the text value to input commands
            var inputs = $"{textValue}".ConvertToInputs().ToArray();

            try
            {
                // Send the input commands using the session's automation service
                sessionModel.Automation.SendInputs(inputs);
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while attempting to send the keystrokes: {e}");
            }

            // Return an OK response indicating the keystrokes were sent successfully
            return Ok();
        }

        // POST wd/hub/user32/session/{s}/inputs
        // POST user32/session/{s}/inputs
        [HttpPost]
        [Route("user32/session/{session}/inputs")]
        [SwaggerOperation(
            Summary = "Sends key scan codes to the specified session.",
            Description = "Performs a send key scan codes action in the session identified by the given session ID with the provided key scan codes.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Key scan codes sent successfully.")]
        [SwaggerResponse(400, "Invalid request. The data does not contain a valid 'wScans' key or the key scans are not in the correct format.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to send the key scan codes.")]
        public IActionResult SendKeyScans(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The data containing the key scan codes to send, including the 'wScans' key with the key scan codes.")] IDictionary<string, object> data)
        {
            // Check if the data contains the "wScans" key
            var isKeyScans = data.TryGetValue("wScans", out var keyScans);

            // Return a bad request response if the "wScans" key is not present or not in the correct format
            if (!isKeyScans)
            {
                return BadRequest(error: "The data does not contain a valid 'wScans' key.");
            }

            // Check if keyScans is of type IEnumerable<string>
            if (keyScans is not IEnumerable<string>)
            {
                return BadRequest(error: "The 'wScans' key must be an array of strings.");
            }

            // Convert the scan codes from strings to the appropriate format
            var wScans = ((IEnumerable<string>)keyScans).Select(i => i.GetScanCode());

            // Get the session model using the domain's session repository
            var sessionModel = _domain.GetSession(session);

            try
            {
                // Send the key scan codes to the session
                foreach (var wScan in wScans)
                {
                    var down = wScan.ConvertToInput(KeyEvent.KeyDown | KeyEvent.Scancode);
                    var up = wScan.ConvertToInput(KeyEvent.KeyUp | KeyEvent.Scancode);
                    sessionModel.Automation.SendInputs(down, up);
                }
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while attempting to send the key scan codes: {e.Message}");
            }

            // Return an OK response indicating the key scan codes were sent successfully
            return Ok();
        }

        // POST wd/hub/user32/session/{session}/modified
        // POST user32/session/{session}/modified
        [HttpPost]
        [Route("user32/session/{session}/modified")]
        [SwaggerOperation(
            Summary = "Sends a modified key (e.g., Ctrl+C) to the specified session.",
            Description = "Performs a send modified key action in the session identified by the given session ID with the provided modifier and main key.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Modified key sent successfully.")]
        [SwaggerResponse(400, "Invalid request. The data does not contain a valid 'modifier' or 'key' key.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to send the modified key.")]
        public IActionResult SendModifiedKey(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The data containing the modifier key and the main key to send.")] IDictionary<string, object> data)
        {
            // Get the session model using the domain's session repository
            var sessionModel = _domain.GetSession(session);

            // Check if the data contains the "modifier" key
            var isModifier = data.TryGetValue(key: "modifier", out var modifier);

            // Check if the data contains the "key" key
            var isKey = data.TryGetValue(key: "key", out var key);

            // Return a bad request response if the "modifier" or "key" key is not present
            if (!isModifier || !isKey)
            {
                return BadRequest(error: "The data must contain both 'modifier' and 'key' keys.");
            }

            try
            {
                // Send the modified key using the session's automation service
                sessionModel.Automation.SendModifiedKey($"{modifier}", $"{key}");
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(
                    statusCode: StatusCodes.Status500InternalServerError,
                    value: $"An error occurred while attempting to send the modified key: {e.Message}");
            }

            // Return an OK response indicating the modified key was sent successfully
            return Ok();
        }
    }
}
