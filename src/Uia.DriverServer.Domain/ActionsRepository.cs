/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Marshals.Models;
using Uia.DriverServer.Models;

#pragma warning disable S3011 // Suppresses warnings about using reflection to access members, which is necessary for dynamic method invocation in this context.
#pragma warning disable S1144, IDE0051, RCS1213 // Suppresses warnings about unused private methods, which are dynamically invoked using reflection.
namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Implements the IActionsRepository interface to handle sending actions to the system.
    /// </summary>
    public class ActionsRepository : IActionsRepository
    {
        // Dictionary mapping special characters to their corresponding scan codes.
        private static Dictionary<string, ushort> CharToScanCode => new()
        {
            { "\uE000", 0x01 }, // Null (example)
            { "\uE001", 0x02 }, // Cancel (example)
            { "\uE002", 0x03 }, // Help (example)
            { "\uE003", 0x0E }, // Backspace
            { "\uE004", 0x0F }, // Tab
            { "\uE005", 0x04 }, // Clear (example)
            { "\uE006", 0x1C }, // Return
            { "\uE007", 0x1C }, // Enter
            { "\uE008", 0x2A }, // Shift
            { "\uE009", 0x1D }, // Control
            { "\uE00A", 0x38 }, // Alt
            { "\uE00B", 0x39 }, // Pause (example)
            { "\uE00C", 0x01 }, // Escape
            { "\uE00D", 0x39 }, // Space
            { "\uE00E", 0x49 }, // PageUp
            { "\uE00F", 0x51 }, // PageDown
            { "\uE010", 0x4F }, // End
            { "\uE011", 0x47 }, // Home
            { "\uE012", 0x4B }, // Left / ArrowLeft
            { "\uE013", 0x48 }, // Up / ArrowUp
            { "\uE014", 0x4D }, // Right / ArrowRight
            { "\uE015", 0x50 }, // Down / ArrowDown
            { "\uE016", 0x52 }, // Insert
            { "\uE017", 0x53 }, // Delete
            { "\uE018", 0x27 }, // Semicolon
            { "\uE019", 0x0D }, // Equal
            { "\uE01A", 0x52 }, // NumberPad0 (example)
            { "\uE01B", 0x4F }, // NumberPad1 (example)
            { "\uE01C", 0x50 }, // NumberPad2 (example)
            { "\uE01D", 0x51 }, // NumberPad3 (example)
            { "\uE01E", 0x4B }, // NumberPad4 (example)
            { "\uE01F", 0x4C }, // NumberPad5 (example)
            { "\uE020", 0x4D }, // NumberPad6 (example)
            { "\uE021", 0x47 }, // NumberPad7 (example)
            { "\uE022", 0x48 }, // NumberPad8 (example)
            { "\uE023", 0x49 }, // NumberPad9 (example)
            { "\uE024", 0x37 }, // Multiply
            { "\uE025", 0x4E }, // Add
            { "\uE026", 0x04 }, // Separator (example)
            { "\uE027", 0x4A }, // Subtract
            { "\uE028", 0x53 }, // Decimal (example)
            { "\uE029", 0x35 }, // Divide
            { "\uE031", 0x3B }, // F1
            { "\uE032", 0x3C }, // F2
            { "\uE033", 0x3D }, // F3
            { "\uE034", 0x3E }, // F4
            { "\uE035", 0x3F }, // F5
            { "\uE036", 0x40 }, // F6
            { "\uE037", 0x41 }, // F7
            { "\uE038", 0x42 }, // F8
            { "\uE039", 0x43 }, // F9
            { "\uE03A", 0x44 }, // F10
            { "\uE03B", 0x57 }, // F11
            { "\uE03C", 0x58 }, // F12
            { "\uE03D", 0x5B }, // Meta / Command (example)
            { "\uE040", 0x29 }  // ZenkakuHankaku (example)
        };

        /// <inheritdoc />
        public void SendActions(UiaSessionResponseModel session, ActionsModel actionsModel)
        {
            foreach (var action in actionsModel.Actions)
            {
                // Iterate through each input in the sequence.
                foreach (var input in FilterActions(action))
                {
                    // Send the input to the system.
                    SendInput(instance: this, session: session, inputData: input);
                }
            }
        }

        // Sends input to the specified UI automation session based on the provided input data.
        private static void SendInput(ActionsRepository instance, UiaSessionResponseModel session, Dictionary<string, object> inputData)
        {
            // Check if the input data contains a "type" key, and retrieve its value
            if (!inputData.TryGetValue("type", out object type))
            {
                // If "type" is not found, return early
                return;
            }

            // Retrieve all non-public, static methods from the ActionsRepository type
            var inputMethods = instance
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

            // Find the method that matches the input type name (case-insensitive)
            var inputMethod = Array.Find(inputMethods, m => m.Name.Equals($"{type}", StringComparison.OrdinalIgnoreCase));

            // Create a new InputDataModel with the provided input data and session
            var inputDataModel = new InputDataModel
            {
                Data = inputData,
                Session = session
            };

            // Invoke the input method with the input data model
            inputMethod.Invoke(instance, [inputDataModel]);
        }

        // Handles the key down event by sending the appropriate input to the system.
        private static void KeyDown(InputDataModel inputData)
        {
            // Check if the input data contains a "value" key
            if (!inputData.Data.TryGetValue("value", out object value))
            {
                // If "value" is not found, return early
                return;
            }

            // Convert the value to a string key
            var key = $"{value}";

            // Check if the key is a special character that requires
            // a scan code lookup table entry (e.g., "\uE000")
            if (CharToScanCode.TryGetValue(key, out ushort keyCode))
            {
                // Convert the scan code to a keyboard input event
                var input = keyCode.ConvertToInput(KeyEvent.KeyDown);

                // Send the input event to the system
                inputData.Session.Automation.SendInputs(input);

                // return early
                return;
            }

            // Convert the value to a sequence of keyboard input events
            var inputs = key.ConvertToInputs(KeyEvent.KeyDown).ToArray();

            // Send the input events to the system
            inputData.Session.Automation.SendInputs(inputs);
        }

        // Handles the key up event by sending the appropriate input to the system.
        private static void KeyUp(InputDataModel inputData)
        {
            // Check if the input data contains a "value" key
            if (!inputData.Data.TryGetValue("value", out object value))
            {
                // If "value" is not found, return early
                return;
            }

            // Convert the value to a string key
            var key = $"{value}";

            // Check if the key is a special character that requires
            // a scan code lookup table entry (e.g., "\uE000")
            if (CharToScanCode.TryGetValue(key, out ushort keyCode))
            {
                // Convert the scan code to a keyboard input event
                var input = keyCode.ConvertToInput(KeyEvent.KeyUp);

                // Send the input event to the system
                inputData.Session.Automation.SendInputs(input);

                // return early
                return;
            }

            // Convert the value to a sequence of keyboard input events
            var inputs = $"{value}".ConvertToInputs(KeyEvent.KeyUp).ToArray();

            // Send the input events to the system
            inputData.Session.Automation.SendInputs(inputs);
        }

        // Handles the pause event by pausing the execution for the specified duration.
        private static void Pause(InputDataModel inputData)
        {
            // Check if the input data contains a "duration" key
            if (!inputData.Data.TryGetValue("duration", out object value))
            {
                // If "duration" is not found, return early
                return;
            }

            // Try to parse the duration value from the input data
            var isDuration = int.TryParse($"{value}", out int durationOut);

            // If parsing is successful, use the parsed value; otherwise, default to 0
            var duration = isDuration ? durationOut : 0;

            // Pause the execution for the specified duration
            Thread.Sleep(TimeSpan.FromMilliseconds(duration));
        }

        // Handles the pointer down event by sending the appropriate mouse input to the system.
        private static void PointerDown(InputDataModel inputData)
        {
            // Check if the input data contains a "button" key
            if (!inputData.Data.TryGetValue("button", out object value))
            {
                // If "button" is not found, return early
                return;
            }

            // Try to parse the button value from the input data
            _ = int.TryParse($"{value}", out int buttonOut);

            // Determine the mouse event based on the button value
            var mouseEvent = buttonOut == 2 ? MouseEvent.RightDown : MouseEvent.LeftDown;

            // Get the physical cursor position
            var point = User32.GetPhysicalCursorPosition();

            // Create a new mouse input for the session based on the mouse event
            var input = inputData.Session.NewMouseInput(mouseEvent, point.x, point.y);

            // Send the input event to the system
            inputData.Session.Automation.SendInputs(input);
        }

        // Handles the pointer move event by moving the cursor to the specified coordinates.
        private static void PointerMove(InputDataModel inputData)
        {
            // Determines the new point coordinates based on the origin and input data.
            static (int X, int Y) NewPoint(string origin, InputDataModel inputData)
            {
                // Check if the origin is an element identifier
                var isElement = !origin.Equals("pointer") && !origin.Equals("viewport");

                // Try to get the X coordinate from the input data, default to 0 if not found
                var x = inputData.Data.TryGetValue("x", out object xValue)
                    ? int.Parse($"{xValue}")
                    : int.Parse("0");

                // Try to get the Y coordinate from the input data, default to 0 if not found
                var y = inputData.Data.TryGetValue("y", out object yValue)
                    ? int.Parse($"{yValue}")
                    : int.Parse("0");

                if (!isElement)
                {
                    // Return the coordinates as a tuple
                    return (x, y);
                }

                // Deserialize the origin string to get the element identifier
                var elementData = JsonSerializer.Deserialize<Dictionary<string, string>>(origin);
                var element = elementData.First().Value;

                // Get the element model from the session using the element identifier
                var elementModel = inputData.Session.GetElement(element);

                // Check if the element can have focus
                var canHaveFocus = elementModel.UIAutomationElement?.CurrentIsKeyboardFocusable == 1;

                // Set focus to the element if it can have focus
                if (canHaveFocus)
                {
                    try
                    {
                        elementModel.UIAutomationElement.SetFocus();
                    }
                    catch
                    {
                        // Silently ignore any exceptions
                    }
                }

                // Get the clickable point of the element
                var clickablePoint = elementModel.GetClickablePoint();

                // Set the coordinates to the clickable point of the element
                x = clickablePoint.X + x;
                y = clickablePoint.Y + y;

                // Return the coordinates as a tuple
                return (x, y);
            }

            // Get the origin value from the input data, default to an empty string if not found
            var origin = inputData.Data.TryGetValue("origin", out object originValue)
                ? $"{originValue}"
                : string.Empty;

            // Get the new coordinates based on the origin and input data
            var (x, y) = NewPoint(origin, inputData);

            // If the origin is "pointer", adjust the coordinates relative to the current cursor position
            if (origin.Equals("pointer"))
            {
                // Get the current physical cursor position
                var location = User32.GetPhysicalCursorPosition();

                // Adjust the new coordinates based on the current cursor position
                x += location.x;
                y += location.y;
            }

            // Set the physical cursor position to the new coordinates
            User32.SetPhysicalCursorPosition(x, y);
        }

        // Handles the pointer up event by sending the appropriate mouse input to the system.
        private static void PointerUp(InputDataModel inputData)
        {
            // Check if the input data contains a "button" key
            if (!inputData.Data.TryGetValue("button", out object value))
            {
                // If "button" is not found, return early
                return;
            }

            // Try to parse the button value from the input data
            _ = int.TryParse($"{value}", out int buttonOut);

            // Determine the mouse event based on the button value
            var mouseEvent = buttonOut == 2 ? MouseEvent.RightUp : MouseEvent.LeftUp;

            // Get the physical cursor position
            var point = User32.GetPhysicalCursorPosition();

            // Create a new mouse input for the session based on the mouse event
            var input = inputData.Session.NewMouseInput(mouseEvent, point.x, point.y);

            // Send the input event to the system
            inputData.Session.Automation.SendInputs(input);
        }

        // Filters the actions from the given action model to exclude unnecessary pause actions.
        private static List<Dictionary<string, object>> FilterActions(ActionsModel.ActionModel actionModel)
        {
            // List to hold the filtered actions
            var filteredActions = new List<Dictionary<string, object>>();

            // Iterate over each action in the action model
            foreach (var item in actionModel.Actions)
            {
                // Extract the type of the action as a string
                var type = $"{item["type"]}";

                // Try to get the duration value from the action dictionary
                var durationValue = item.TryGetValue("duration", out object d) ? d : 0;

                // Check if the duration value can be parsed to an integer
                var isDuration = int.TryParse($"{durationValue}", out int durationOut);

                // Add the action to the filtered list if it's not a pause or if it's a pause with a non-zero duration
                if (!type.Equals("pause") || (isDuration && durationOut != 0))
                {
                    filteredActions.Add(item);
                }
            }

            // Return the list of filtered actions
            return filteredActions;
        }

        /// <summary>
        /// Represents the input data model containing event details and session information.
        /// </summary>
        private sealed class InputDataModel
        {
            /// <summary>
            /// Gets or sets the dictionary containing input event data.
            /// </summary>
            public Dictionary<string, object> Data { get; set; }

            /// <summary>
            /// Gets or sets the UI automation session response model.
            /// </summary>
            public UiaSessionResponseModel Session { get; set; }
        }
    }
}
