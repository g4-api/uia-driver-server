/*
 * RESOURCES
 * https://learn.microsoft.com/en-us/windows/win32/apiindex/windows-api-list
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Uia.DriverServer.Domain;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Marshals.Models;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Controllers
{
    [Route("wd/hub"), Route("/")]
    [ApiController]
    [SwaggerTag(
        description: "Controller for UI Automation User32-related actions",
        externalDocsUrl: "https://learn.microsoft.com/en-us/windows/win32/apiindex/windows-api-list")]
    public class User32Controller(IUiaDomain domain) : ControllerBase
    {
        // Initialize the UIA domain interface
        private readonly IUiaDomain _domain = domain;

        [HttpGet]
        [Route("user32/layouts")]
        [SwaggerOperation(
            Summary = "Retrieves available keyboard layouts.",
            Description = "Gets a list of available keyboard layout identifiers from the CodeMaps class.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Keyboard layouts retrieved successfully.", typeof(IEnumerable<string>))]
        public IActionResult GetKeyboardLayouts()
        {
            // Retrieve and return the list of keyboard layouts.
            return Ok(CodeMaps.GetLayouts());
        }

        [HttpPost]
        [Route("user32/layouts")]
        [SwaggerOperation(
            Summary = "Sets the keyboard layout.",
            Description = "Sets the current keyboard layout to the one specified in the request body.",
            Tags = ["User32"])]
        [SwaggerResponse(204, "Keyboard layout set successfully.")]
        public IActionResult SetKeyboardLayout(
            [FromBody][SwaggerParameter(Description = "The keyboard layout model containing the layout identifier to be set.")] KeyboardLayoutModel layoutModel)
        {
            // Switch the keyboard layout using the provided layout identifier from the model.
            User32.SwitchKeyboardLayout(layoutModel.Layout);

            // Return a 204 No Content response indicating the keyboard layout was successfully set.
            return NoContent();
        }

        [HttpPost]
        [Route("session/{session}/user32/element/{element}/copy")]
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
            var sessionModel = _domain.SessionsRepository.GetSession(id: session).Session;

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
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/paste")]
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
            var sessionModel = _domain.SessionsRepository.GetSession(id: session).Session;

            try
            {
                // Perform the paste action using the session's automation service
                sessionModel.Automation.SendModifiedKey(modifier: "Ctrl", key: "V");
            }
            catch (Exception e)
            {
                // Log the exception (not shown here) and return a 500 error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while attempting to paste: {e}");
            }

            // Return an OK response indicating the paste action was successful
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/element/{element}/select")]
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
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/value")]
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
            [SwaggerRequestBody(Description = "The data containing the keystrokes to send, including the 'text' key with the keystrokes to send.")] TextInputModel textData)
        {
            // Get the session model using the domain's session repository
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Convert the text value to input commands
            var inputs = $"{textData.Text}".ConvertToInputs().ToArray();

            // Send the input commands using the session's automation service
            sessionModel.Automation.SendInputs(inputs);

            // Return an OK response indicating the keystrokes were sent successfully
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/inputs")]
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
            [SwaggerRequestBody(Description = "The data containing the key scan codes to send, including the 'wScans' key with the key scan codes.")] ScanCodesInputModel keyScansData)
        {
            // Get the session model using the domain's session repository
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Get the session model using the domain's session repository
            var statusCode = _domain.SessionsRepository.SendUser32Keys(automation: sessionModel.Automation, keyScansData);

            // Return an OK response indicating the key scan codes were sent successfully
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = statusCode
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/modified")]
        [SwaggerOperation(
            Summary = "Sends a modified key (e.g., Ctrl+C) to the specified session.",
            Description = "Performs a send modified key action in the session identified by the given session ID with the provided modifier and main key.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Modified key sent successfully.")]
        [SwaggerResponse(400, "Invalid request. The data does not contain valid 'modifier' or 'key' properties.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to send the modified key.")]
        public IActionResult SendModifiedKey(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The data containing the modifier key and the main key to send.")] ModifiedKeyInputModel inputData)
        {
            // Get the session model using the domain's session repository
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Send the modified key using the session's automation service
            sessionModel.Automation.SendModifiedKey($"{inputData.Modifier}", $"{inputData.Key}");

            // Return an OK response indicating the modified key was sent successfully
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/element/{element}/click")]
        [SwaggerOperation(
            Summary = "Invokes a click action on the specified element in the given session.",
            Description = "Performs a click action on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Click action invoked successfully.")]
        [SwaggerResponse(404, "Session or element not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to click the element.")]
        public IActionResult SendNativeClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be clicked.")] string element)
        {
            // Get the session status code
            var sessionModel = _domain.SessionsRepository.GetSession(id: session).Session;

            // Retrieve the element using the domain's elements repository
            var elementModel = _domain.ElementsRepository.GetElement(session, element);

            // Check if the element is stale
            if (elementModel == default)
            {
                // Return a not found response if the element is stale
                return NotFound(WebDriverResponseModel.NewStaleReferenceResponse(session));
            }

            // Check if the UIAutomationElement property of the element model is null
            if (elementModel.UIAutomationElement == null)
            {
                // Get the clickable point of the element based on the session's scale ratio
                var point = elementModel.GetClickablePoint();

                // Send a native click to the computed point using the session's automation object
                sessionModel.Automation.SendNativeClick(point);
            }
            else
            {
                // Send a native click to the element using its UIAutomationElement property and the session's scale ratio
                elementModel.UIAutomationElement.SendNativeClick();
            }

            // Return an OK response indicating the click action was successful
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/click")]
        [SwaggerOperation(
            Summary = "Performs a native click at the specified coordinates in the given session.",
            Description = "Sends a native click action to the coordinates specified by the point parameter in the session identified by the given session ID.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Native click performed successfully.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to perform the native click.")]
        public IActionResult SendNativeClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The coordinates where the click should be performed.")] PointModel point)
        {
            // Retrieve the session based on the provided ID
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Perform a native click at the specified coordinates using the session's automation
            sessionModel.Automation.SendNativeClick(point.X, point.Y);

            // Return an OK response indicating the native click was performed successfully
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/dclick")]
        [SwaggerOperation(
            Summary = "Invokes a double-click action at the specified coordinates in the given session.",
            Description = "Performs a double-click action at the coordinates specified by the point parameter in the session identified by the given session ID.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Double-click action invoked successfully.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to perform the double-click action.")]
        public IActionResult SendNativeDoubleClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The coordinates where the double-click should be performed.")] PointModel point)
        {
            // Get the session status code
            var sessionModel = _domain.SessionsRepository.GetSession(id: session).Session;

            // Send a native double-click to the specified coordinates using the session's automation object
            sessionModel.Automation.SendNativeClick(point, repeat: 2);

            // Return an OK response indicating the double-click action was successful
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/element/{element}/dclick")]
        [SwaggerOperation(
            Summary = "Invokes a double-click action on the specified element in the given session.",
            Description = "Performs a double-click action on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Double-click action invoked successfully.")]
        [SwaggerResponse(404, "Session or element not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to perform the double-click action.")]
        public IActionResult SendNativeDoubleClick(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to be double-clicked.")] string element)
        {
            // Get the session status code and model
            var sessionModel = _domain.SessionsRepository.GetSession(id: session).Session;

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
                var point = elementModel.GetClickablePoint();

                // Send a native double-click to the computed point using the session's automation object
                sessionModel.Automation.SendNativeClick(point, repeat: 2);
            }
            else
            {
                // Send a native double-click to the element using its UIAutomationElement
                // property and the session's scale ratio
                elementModel.UIAutomationElement.SendNativeClick(align: default, repeat: 2);
            }

            // Return an OK response indicating the double-click action was successful
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpGet]
        [Route("session/{session}/user32/element/{element}/focus")]
        [SwaggerOperation(
            Summary = "Sets focus on the specified element in the given session.",
            Description = "Sets focus on the element identified by the given session and element IDs.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Focus set successfully.")]
        [SwaggerResponse(404, "Element or session not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to set focus on the element.")]
        public IActionResult SetFocus(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element to set focus on.")] string element)
        {
            // Retrieve the element based on the provided session ID and element ID
            var elementModel = _domain.ElementsRepository.GetElement(session, element);

            // Check if the element was not found
            if (elementModel == null || elementModel.UIAutomationElement == null)
            {
                // Return a not found response if the element was not found
                return NotFound("Element or session not found.");
            }

            // Set focus on the UI automation element
            elementModel.UIAutomationElement.SetFocus();

            // Return an OK response indicating the focus action was successful
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/mouse/move")]
        [SwaggerOperation(
            Summary = "Sets the mouse position to the specified coordinates in the given session.",
            Description = "Moves the mouse cursor to the coordinates specified by the point parameter in the session identified by the given session ID.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Mouse position set successfully.")]
        [SwaggerResponse(400, "Invalid request. The provided point data is not valid.")]
        [SwaggerResponse(404, "Session not found. The session ID provided does not exist.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to set the mouse position.")]
        public IActionResult SetMousePosition(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerRequestBody(Description = "The coordinates to set the mouse position to.")] PointModel point)
        {
            // Retrieve the session based on the provided ID
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Set the cursor position using the session's automation
            sessionModel.Automation.SetCursorPosition(point.X, point.Y);

            // Return an OK response indicating the mouse position was set successfully
            return new JsonResult(new WebDriverResponseModel())
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpPost]
        [Route("session/{session}/user32/element/{element}/mouse/move")]
        [SwaggerOperation(
            Summary = "Sets the mouse position to the specified element in the given session.",
            Description = "Moves the mouse cursor to the specified element with alignment and offset options in the session identified by the given session ID.",
            Tags = ["User32"])]
        [SwaggerResponse(200, "Mouse position set successfully.", typeof(PointModel))]
        [SwaggerResponse(404, "Session or element not found. The session ID or element ID provided does not exist.")]
        [SwaggerResponse(400, "Invalid input data. The alignment value is not supported.")]
        [SwaggerResponse(500, "Internal server error. An error occurred while attempting to set the mouse position.")]
        public IActionResult SetMousePosition(
            [SwaggerParameter(Description = "The unique identifier for the session.")] string session,
            [SwaggerParameter(Description = "The unique identifier for the element.")] string element,
            [SwaggerRequestBody(Description = "The data containing alignment and offset information.")] MousePositionInputModel poistionData)
        {
            // Retrieve the session based on the provided ID
            var sessionModel = _domain.SessionsRepository.GetSession(session).Session;

            // Retrieve the element based on the provided session ID and element ID
            var elementModel = _domain.ElementsRepository.GetElement(session, element);

            // Determine alignment of the mouse pointer (default: TopLeft)
            var alignment = string.IsNullOrEmpty(poistionData.Alignment)
                ? "TopLeft"
                : poistionData.Alignment;

            // Determine top offset of the mouse pointer (default: 1)
            var topOffset = poistionData.OffsetY;

            // Determine left offset of the mouse pointer (default: 1)
            var leftOffset = poistionData.OffsetX;

            // Get the clickable point on the element with the specified alignment and offsets
            var point = elementModel
                .UIAutomationElement
                .GetClickablePoint(alignment, topOffset, leftOffset);

            // Set the cursor position to the calculated point
            sessionModel.Automation.SetCursorPosition(point.X, point.Y);

            // Return a JSON result with the clickable point and a 200 OK status code
            return new JsonResult(value: point)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
