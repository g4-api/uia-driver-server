/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

using Uia.DriverServer.Marshals;
using Uia.DriverServer.Marshals.Models;
using Uia.DriverServer.Models;

using UIAutomationClient;

namespace Uia.DriverServer.Extensions
{
    public static class AutomationExtensions
    {
        // A static dictionary mapping key names to their corresponding scan codes.
        private static readonly Dictionary<string, ushort> s_CodeMap = new (StringComparer.OrdinalIgnoreCase)
        {
            ["Esc"] = 0x01,        // Escape key
            ["1"] = 0x02,          // 1 key
            ["2"] = 0x03,          // 2 key
            ["3"] = 0x04,          // 3 key
            ["4"] = 0x05,          // 4 key
            ["5"] = 0x06,          // 5 key
            ["6"] = 0x07,          // 6 key
            ["7"] = 0x08,          // 7 key
            ["8"] = 0x09,          // 8 key
            ["9"] = 0x0A,          // 9 key
            ["0"] = 0x0B,          // 0 key
            ["-"] = 0x0C,          // Hyphen key
            ["="] = 0x0D,          // Equals key
            ["Backspace"] = 0x0E,  // Backspace key
            ["Tab"] = 0x0F,        // Tab key
            ["Q"] = 0x10,          // Q key
            ["W"] = 0x11,          // W key
            ["E"] = 0x12,          // E key
            ["R"] = 0x13,          // R key
            ["T"] = 0x14,          // T key
            ["Y"] = 0x15,          // Y key
            ["U"] = 0x16,          // U key
            ["I"] = 0x17,          // I key
            ["O"] = 0x18,          // O key
            ["P"] = 0x19,          // P key
            ["["] = 0x1A,          // Open bracket key
            ["]"] = 0x1B,          // Close bracket key
            ["Enter"] = 0x1C,      // Enter key
            ["Ctrl"] = 0x1D,       // Control key
            ["A"] = 0x1E,          // A key
            ["S"] = 0x1F,          // S key
            ["D"] = 0x20,          // D key
            ["F"] = 0x21,          // F key
            ["G"] = 0x22,          // G key
            ["H"] = 0x23,          // H key
            ["J"] = 0x24,          // J key
            ["K"] = 0x25,          // K key
            ["L"] = 0x26,          // L key
            [";"] = 0x27,          // Semicolon key
            ["'"] = 0x28,          // Apostrophe key
            ["`"] = 0x29,          // Grave accent key
            ["LShift"] = 0x2A,     // Left Shift key
            [@"\"] = 0x2B,         // Backslash key
            ["Z"] = 0x2C,          // Z key
            ["X"] = 0x2D,          // X key
            ["C"] = 0x2E,          // C key
            ["V"] = 0x2F,          // V key
            ["B"] = 0x30,          // B key
            ["N"] = 0x31,          // N key
            ["M"] = 0x32,          // M key
            [","] = 0x33,          // Comma key
            ["."] = 0x34,          // Period key
            ["/"] = 0x35,          // Slash key
            ["RShift"] = 0x36,     // Right Shift key
            ["PrtSc"] = 0x37,      // Print Screen key
            ["Alt"] = 0x38,        // Alt key
            [" "] = 0x39,          // Spacebar key
            ["CapsLock"] = 0x3A,   // Caps Lock key
            ["F1"] = 0x3B,         // F1 key
            ["F2"] = 0x3C,         // F2 key
            ["F3"] = 0x3D,         // F3 key
            ["F4"] = 0x3E,         // F4 key
            ["F5"] = 0x3F,         // F5 key
            ["F6"] = 0x40,         // F6 key
            ["F7"] = 0x41,         // F7 key
            ["F8"] = 0x42,         // F8 key
            ["F9"] = 0x43,         // F9 key
            ["F10"] = 0x44,        // F10 key
            ["Num"] = 0x45,        // Num Lock key
            ["Scroll"] = 0x46,     // Scroll Lock key
            ["Home"] = 0x47,       // Home key
            ["Up"] = 0x48,         // Up arrow key
            ["PgUp"] = 0x49,       // Page Up key
            ["Left"] = 0x4B,       // Left arrow key
            ["Center"] = 0x4C,     // Center key
            ["Right"] = 0x4D,      // Right arrow key
            ["End"] = 0x4F,        // End key
            ["Down"] = 0x50,       // Down arrow key
            ["PgDn"] = 0x51,       // Page Down key
            ["Ins"] = 0x52,        // Insert key
            ["Del"] = 0x53         // Delete key
        };

        /// <summary>
        /// Confirms the presence of required capabilities in the provided <see cref="NewSessionModel"/>.
        /// </summary>
        /// <param name="capabilities">The capabilities to confirm.</param>
        /// <returns>
        /// A tuple containing an <see cref="IActionResult"/> response and a boolean indicating the result.
        /// </returns>
        public static (IActionResult Response, bool Result) ConfirmCapabilities(this NewSessionModel capabilities)
        {
            // Create a ContentResult to represent the response.
            var response = new ContentResult
            {
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = StatusCodes.Status500InternalServerError
            };

            // Get the capabilities dictionary.
            var capability = capabilities.Capabilities;

            // Check if the required Application capability is present.
            if (!capability.AlwaysMatch.ContainsKey(UiaCapabilities.Application))
            {
                // Set the response content to indicate the missing capability.
                response.Content = string.Format("Missing required capability: {0}", UiaCapabilities.Application);

                // Return the response with a failure result.
                return (response, false);
            }

            // Set the response status code and content type to indicate success.
            response.StatusCode = StatusCodes.Status200OK;
            response.Content = string.Empty;
            response.ContentType = MediaTypeNames.Application.Json;

            // Return the response with a success result.
            return (response, true);
        }

        /// <summary>
        /// Confirms whether the specified UI Automation element is enabled.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to check.</param>
        /// <returns><c>true</c> if the element is enabled; otherwise, <c>false</c>.</returns>
        public static bool ConfirmEnabledState(this IUIAutomationElement element)
        {
            // Check if the element's current enabled state is 1 (true).
            return element.CurrentIsEnabled == 1;
        }

        /// <summary>
        /// Converts a <see cref="IUIAutomationElement"/> to an <see cref="UiaElementModel"/>.
        /// </summary>
        /// <param name="automationElement">The <see cref="IUIAutomationElement"/> to convert.</param>
        /// <returns>An <see cref="UiaElementModel"/> representing the UI Automation element.</returns>
        public static UiaElementModel ConvertToElement(this IUIAutomationElement automationElement)
        {
            // Check if the automation element is null.
            if(automationElement == default)
            {
                return default;
            }

            // Get the automation ID of the element.
            var automationId = automationElement.CurrentAutomationId;

            // If the automation ID is null or empty, generate a new GUID as the ID; otherwise, use the automation ID.
            var id = string.IsNullOrEmpty(automationId)
                ? $"{Guid.NewGuid()}"
                : automationElement.CurrentAutomationId;

            // Create a new LocationModel and set its properties based on the element's bounding rectangle.
            var location = new RectangleModel
            {
                Bottom = automationElement.CurrentBoundingRectangle.bottom,
                Left = automationElement.CurrentBoundingRectangle.left,
                Right = automationElement.CurrentBoundingRectangle.right,
                Top = automationElement.CurrentBoundingRectangle.top
            };

            // Create and return a new ElementModel with the ID, UI Automation element, and location.
            return new UiaElementModel
            {
                Id = id,
                UIAutomationElement = automationElement,
                Rectangle = location
            };
        }

        /// <summary>
        /// Converts the specified key code and keyboard event into an <see cref="Input"/> object.
        /// </summary>
        /// <param name="keyCode">The key code to be converted.</param>
        /// <param name="keyboardEvents">The keyboard event associated with the key code.</param>
        /// <returns>An <see cref="Input"/> object representing the specified key code and keyboard event.</returns>
        public static Input ConvertToInput(this ushort keyCode, KeyEvent keyboardEvents)
        {
            // Create a new Input object using the specified key code and keyboard event
            return NewKeyboardInput(keyCode, keyboardEvents);
        }

        /// <summary>
        /// Converts a string input into a sequence of keyboard input events.
        /// </summary>
        /// <param name="input">The string input to convert.</param>
        /// <returns>A sequence of <see cref="Input"/> structures representing the keyboard input events.</returns>
        public static IEnumerable<Input> ConvertToInputs(this string input)
        {
            // Check if the input is a single key in the scan code map.
            if (s_CodeMap.TryGetValue(input, out ushort value))
            {
                // Return the key down and key up events for the key.
                return
                [
                    NewKeyboardInput(keyCode: value, KeyEvent.KeyDown | KeyEvent.Scancode),
                    NewKeyboardInput(keyCode: value, KeyEvent.KeyUp | KeyEvent.Scancode)
                ];
            }

            // Initialize a list to store the input events.
            var inputs = new List<Input>();

            // Iterate over each character in the input string.
            foreach (var character in input)
            {
                // Check if the character is a modified key (e.g., Shift + key).
                var (modified, modifier, keyCode) = ConfirmModifiedKey(input: $"{character}");

                if (modified)
                {
                    // Add the modified key input events to the list.
                    inputs.AddRange(NewKeyboardInput(modifier, keyCode));
                    continue;
                }

                // Get the scan code for the character.
                var wScan = s_CodeMap.ContainsKey($"{character}")
                    ? s_CodeMap[$"{character}"]
                    : (ushort)0x00;

                // Add the key down and key up events for the character to the list.
                inputs.AddRange(
                [
                    NewKeyboardInput(wScan, KeyEvent.KeyDown | KeyEvent.Scancode),
                    NewKeyboardInput(wScan, KeyEvent.KeyUp | KeyEvent.Scancode)
                ]);
            }

            // Return the list of input events.
            return inputs;
        }

        /// <summary>
        /// Converts a string representation of a tree scope to a corresponding <see cref="TreeScope"/> value.
        /// </summary>
        /// <param name="treeScope">The string representation of the tree scope.</param>
        /// <returns>The corresponding <see cref="TreeScope"/> value.</returns>
        public static TreeScope ConvertToTreeScope(this string treeScope) => treeScope.ToUpper() switch
        {
            "NONE" => TreeScope.TreeScope_None,
            "ANCESTORS" => TreeScope.TreeScope_Ancestors,
            "CHILDREN" => TreeScope.TreeScope_Children,
            "DESCENDANTS" => TreeScope.TreeScope_Descendants,
            "ELEMENT" => TreeScope.TreeScope_Element,
            "PARENT" => TreeScope.TreeScope_Parent,
            "SUBTREE" => TreeScope.TreeScope_Subtree,
            _ => TreeScope.TreeScope_Descendants
        };

        /// <summary>
        /// Finds an element based on a location strategy within a UI Automation session.
        /// </summary>
        /// <param name="session">The UI Automation session model.</param>
        /// <param name="locationStrategy">The location strategy model.</param>
        /// <returns>A tuple containing the found element, its corresponding XML object model, and its runtime ID.</returns>
        public static (IUIAutomationElement Element, XNode ObjectModel, string Runtime) FindElement(
            this UiaSessionResponseModel session,
            LocationStrategyModel locationStrategy)
        {
            // Get the location strategy value and trim any leading or trailing whitespace
            var input = new string(locationStrategy.Value).Trim();

            // Define a regex pattern to match the root or desktop path
            var pattern = new Regex("^(\\/)+(root|desktop)");

            // Check if the input matches the desktop pattern
            var isDesktop = pattern.IsMatch(input);

            // If the input does not match the desktop pattern, return null values
            if (!isDesktop)
            {
                return (Element: null, ObjectModel: null, Runtime: null);
            }

            // Remove the root/desktop part from the input
            locationStrategy.Value = pattern.Replace(input, replacement: string.Empty);

            // Create a new document object model from the application root
            var documentObjectModel = DocumentObjectModelFactory.New(session.ApplicationRoot);

            // Select the XML element based on the modified input expression
            var xmlElement = documentObjectModel.XPathSelectElement(expression: input);

            // Get the runtime ID value from the XML element's "id" attribute
            var runtimeValue = xmlElement?.Attribute("id").Value;

            // Deserialize the runtime ID value into an array of integers
            var runtime = JsonSerializer.Deserialize<IEnumerable<int>>(runtimeValue).ToArray();

            // Create a property condition based on the runtime ID
            var condition = session.Automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, runtime);

            // Get the root element of the desktop
            var desktop = session.Automation.GetRootElement();

            // Find the first element that matches the condition within the descendants of the desktop
            var element = desktop.FindFirst(TreeScope.TreeScope_Descendants, condition);

            // Return a tuple containing the found element, the XML object model, and the runtime ID value
            return (Element: element, ObjectModel: xmlElement, Runtime: runtimeValue);
        }

        /// <summary>
        /// Retrieves an <see cref="IUIAutomationElement"/> by its runtime ID.
        /// </summary>
        /// <param name="session">The <see cref="UiaSessionResponseModel"/> containing the UI Automation session.</param>
        /// <param name="runtime">The runtime ID of the element to retrieve, serialized as a JSON string.</param>
        /// <returns>
        /// The <see cref="IUIAutomationElement"/> matching the runtime ID if found; otherwise, <c>null</c>.
        /// </returns>
        public static IUIAutomationElement FindElementByRuntime(this UiaSessionResponseModel session, string runtime)
        {
            // Get the root element of the session.
            var containerElement = GetApplicationRootElement(session);

            // Deserialize the runtime ID from the JSON string to an IEnumerable<int>.
            var id = JsonSerializer.Deserialize<int[]>(json: runtime);

            // Create a property condition for the runtime ID.
            var condition = session
                .Automation
                .CreatePropertyCondition(propertyId: UIA_PropertyIds.UIA_RuntimeIdPropertyId, value: id);

            // Find and return the first element matching the runtime ID condition within the descendants of the root element.
            return containerElement.FindFirst(scope: TreeScope.TreeScope_Descendants, condition);
        }

        /// <summary>
        /// Finds elements that match the specified condition among the immediate children of the given element.
        /// </summary>
        /// <param name="element">The parent element to search within.</param>
        /// <param name="condition">The condition that the elements must meet.</param>
        /// <returns>An IEnumerable of matching IUIAutomationElement objects.</returns>
        public static IEnumerable<IUIAutomationElement> FindElements(this IUIAutomationElement element, IUIAutomationCondition condition)
        {
            // Call the overloaded FindElements method with TreeScope_Children as the default scope
            return FindElements(element, condition, TreeScope.TreeScope_Children);
        }

        /// <summary>
        /// Finds elements that match the specified condition within the given scope of the element.
        /// </summary>
        /// <param name="element">The parent element to search within.</param>
        /// <param name="condition">The condition that the elements must meet.</param>
        /// <param name="scope">The scope within which to search for the elements.</param>
        /// <returns>An IEnumerable of matching IUIAutomationElement objects.</returns>
        public static IEnumerable<IUIAutomationElement> FindElements(
            this IUIAutomationElement element, IUIAutomationCondition condition, TreeScope scope)
        {
            // Call the FindAll method on the root element with the specified scope and condition
            var elementsArray = element.FindAll(scope, condition);

            // Check if no elements were found (elementsArray is null or has zero length)
            if (elementsArray == null || elementsArray.Length == 0)
            {
                // Return an empty array of IUIAutomationElement
                return [];
            }

            // Initialize a new list to hold the found elements
            var elements = new List<IUIAutomationElement>();

            // Iterate over each element in the elementsArray
            for (int i = 0; i < elementsArray.Length; i++)
            {
                // Add the current element to the elements list
                elements.Add(elementsArray.GetElement(i));
            }

            // Return the list of found elements
            return elements;
        }

        /// <summary>
        /// Gets the root UI Automation element for the application associated with the given session.
        /// </summary>
        /// <param name="session">The <see cref="UiaSessionResponseModel"/> instance representing the UI Automation session.</param>
        /// <returns>An <see cref="IUIAutomationElement"/> representing the root element of the application.</returns>
        public static IUIAutomationElement GetApplicationRoot(this UiaSessionResponseModel session)
        {
            // Calls the GetRoot method to retrieve the root UI Automation element for the application.
            return GetApplicationRootElement(session);
        }

        /// <summary>
        /// Retrieves the value of a specified attribute from the UI Automation element or the XML node.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> from which to retrieve the attribute.</param>
        /// <param name="name">The name of the attribute to retrieve.</param>
        /// <returns>The value of the attribute if found; otherwise, an empty string.</returns>
        public static string GetAttribute(this UiaElementModel element, string name)
        {
            // Check if the UI Automation element is not null
            if (element.UIAutomationElement != null)
            {
                // Retrieve the attribute value from the UI Automation element
                return GetAttribute(element.UIAutomationElement, name);
            }

            // Check if the Node property is null
            if (element.Node == null)
            {
                // Return an empty string if the Node is null
                return string.Empty;
            }

            // Retrieve the attribute value from the Node's Document Root, or return an empty string if not found
            return element.Node.Document.Root.Attribute(name)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the value of a specified attribute from the UI Automation element.
        /// </summary>
        /// <param name="element">The UI Automation element from which to retrieve the attribute.</param>
        /// <param name="name">The name of the attribute to retrieve.</param>
        /// <returns>The value of the attribute if found; otherwise, an empty string.</returns>
        public static string GetAttribute(this IUIAutomationElement element, string name)
        {
            // Retrieve the attributes of the element with a timeout of 5 seconds
            var attributes = GetAttributes(element, timeout: TimeSpan.FromSeconds(5));

            // Try to get the specified attribute from the attributes dictionary
            var isAttribute = attributes.TryGetValue(key: name, out var attribute);

            // Return the attribute value if found; otherwise, return an empty string
            return isAttribute ? attribute : string.Empty;
        }

        /// <summary>
        /// Gets a dictionary of attributes for the specified UI Automation element within the default timeout period of 5 seconds.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the attributes of.</param>
        /// <returns>
        /// A dictionary of attribute names and values for the specified element, or an empty dictionary if the operation fails.
        /// </returns>
        public static IDictionary<string, string> GetAttributes(this IUIAutomationElement element)
        {
            // Call the overloaded GetAttributes method with a default timeout of 5 seconds.
            return GetAttributes(element, timeout: TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gets a dictionary of attributes for the specified UI Automation element within the given timeout period.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the attributes of.</param>
        /// <param name="timeout">The maximum amount of time to attempt to get the attributes.</param>
        /// <returns>
        /// A dictionary of attribute names and values for the specified element, or an empty dictionary if the operation fails.
        /// </returns>
        public static IDictionary<string, string> GetAttributes(this IUIAutomationElement element, TimeSpan timeout)
        {
            // Formats the attributes of the UI Automation element into a dictionary.
            static Dictionary<string, string> FormatAttributes(IUIAutomationElement info) => new(StringComparer.OrdinalIgnoreCase)
            {
                ["AcceleratorKey"] = info.CurrentAcceleratorKey.FormatXml(),
                ["AccessKey"] = info.CurrentAccessKey.FormatXml(),
                ["AriaProperties"] = info.CurrentAriaProperties.FormatXml(),
                ["AriaRole"] = info.CurrentAriaRole.FormatXml(),
                ["AutomationId"] = info.CurrentAutomationId.FormatXml(),
                ["Bottom"] = $"{info.CurrentBoundingRectangle.bottom}",
                ["ClassName"] = info.CurrentClassName.FormatXml(),
                ["FrameworkId"] = info.CurrentFrameworkId.FormatXml(),
                ["HelpText"] = info.CurrentHelpText.FormatXml(),
                ["IsContentElement"] = info.CurrentIsContentElement == 1 ? "true" : "false",
                ["IsControlElement"] = info.CurrentIsControlElement == 1 ? "true" : "false",
                ["IsEnabled"] = info.CurrentIsEnabled == 1 ? "true" : "false",
                ["IsKeyboardFocusable"] = info.CurrentIsKeyboardFocusable == 1 ? "true" : "false",
                ["IsPassword"] = info.CurrentIsPassword == 1 ? "true" : "false",
                ["IsRequiredForForm"] = info.CurrentIsRequiredForForm == 1 ? "true" : "false",
                ["ItemStatus"] = info.CurrentItemStatus.FormatXml(),
                ["ItemType"] = info.CurrentItemType.FormatXml(),
                ["Left"] = $"{info.CurrentBoundingRectangle.left}",
                ["Name"] = info.CurrentName.FormatXml(),
                ["NativeWindowHandle"] = $"{info.CurrentNativeWindowHandle}",
                ["Orientation"] = $"{info.CurrentOrientation}",
                ["ProcessId"] = $"{info.CurrentProcessId}",
                ["Right"] = $"{info.CurrentBoundingRectangle.right}",
                ["Top"] = $"{info.CurrentBoundingRectangle.top}"
            };

            // Calculate the expiration time for the timeout.
            var expiration = DateTime.Now.Add(timeout);

            // Attempt to get the attributes until the timeout expires.
            while (DateTime.Now < expiration)
            {
                try
                {
                    // Format and return the attributes of the element.
                    return FormatAttributes(element);
                }
                catch (COMException)
                {
                    // Ignore COM exceptions and continue attempting until the timeout expires.
                }
            }

            // Return an empty dictionary if the attributes could not be retrieved within the timeout.
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the default clickable point on the UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> to get the clickable point of.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this UiaElementModel element)
        {
            // Call the main GetClickablePoint method with default parameters.
            return ExportClickablePoint(
                boundingRectangle: element.Rectangle,
                align: "TopLeft",
                topOffset: 1,
                leftOffset: 1,
                scaleRatio: 1.0D);
        }

        /// <summary>
        /// Gets the clickable point on the UI Automation element with the specified scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> to get the clickable point of.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this UiaElementModel element, double scaleRatio)
        {
            // Call the main GetClickablePoint method with the specified scale ratio and default other parameters.
            return ExportClickablePoint(
                boundingRectangle: element.Rectangle,
                align: "TopLeft",
                topOffset: 1,
                leftOffset: 1,
                scaleRatio);
        }

        /// <summary>
        /// Gets the clickable point on the UI Automation element with the specified alignment.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this UiaElementModel element, string align)
        {
            // Call the main GetClickablePoint method with the specified alignment and default other parameters.
            return ExportClickablePoint(
                boundingRectangle: element.Rectangle,
                align,
                topOffset: 1,
                leftOffset: 1,
                scaleRatio: 1.0D);
        }

        /// <summary>
        /// Gets the clickable point on the UI Automation element with the specified alignment and scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this UiaElementModel element, string align, double scaleRatio)
        {
            // Call the main ExportClickablePoint method with the specified alignment, scale ratio, and default offsets.
            return ExportClickablePoint(
                boundingRectangle: element.Rectangle,
                align,
                topOffset: 1,
                leftOffset: 1,
                scaleRatio);
        }

        /// <summary>
        /// Gets a clickable point on the UI Automation element based on the specified alignment, offsets, and scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="topOffset">The vertical offset to apply to the clickable point.</param>
        /// <param name="leftOffset">The horizontal offset to apply to the clickable point.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the clickable point calculation.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(
            this UiaElementModel element, string align, int topOffset, int leftOffset, double scaleRatio)
        {
            // Call the main ExportClickablePoint method with the specified alignment, scale ratio, and default offsets.
            return ExportClickablePoint(boundingRectangle: element.Rectangle, align, topOffset, leftOffset, scaleRatio);
        }

        /// <summary>
        /// Gets the default clickable point on the UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the clickable point of.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this IUIAutomationElement element)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Call the main GetClickablePoint method with default parameters.
            return ExportClickablePoint(
                boundingRectangle,
                align: "TopLeft",
                topOffset: 1,
                leftOffset: 1,
                scaleRatio: 1.0D);
        }

        /// <summary>
        /// Gets the clickable point on the UI Automation element with the specified scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the clickable point of.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this IUIAutomationElement element, double scaleRatio)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Call the main GetClickablePoint method with the specified scale ratio and default other parameters.
            return ExportClickablePoint(
                boundingRectangle,
                align: "TopLeft",
                topOffset: 1,
                leftOffset: 1,
                scaleRatio);
        }

        /// <summary>
        /// Gets the clickable point on the UI Automation element with the specified alignment.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this IUIAutomationElement element, string align)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Call the main GetClickablePoint method with the specified alignment and default other parameters.
            return ExportClickablePoint(boundingRectangle, align, topOffset: 1, leftOffset: 1, scaleRatio: 1.0D);
        }

        /// <summary>
        /// Gets the clickable point on the UI Automation element with the specified alignment and scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(this IUIAutomationElement element, string align, double scaleRatio)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Call the main ExportClickablePoint method with the specified alignment, scale ratio, and default offsets.
            return ExportClickablePoint(boundingRectangle, align, topOffset: 1, leftOffset: 1, scaleRatio);
        }

        /// <summary>
        /// Gets a clickable point on the UI Automation element based on the specified alignment, offsets, and scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="topOffset">The vertical offset to apply to the clickable point.</param>
        /// <param name="leftOffset">The horizontal offset to apply to the clickable point.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the clickable point calculation.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(
            this IUIAutomationElement element, string align, int topOffset, int leftOffset, double scaleRatio)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Call the main ExportClickablePoint method with the specified alignment, scale ratio, and default offsets.
            return ExportClickablePoint(boundingRectangle, align, topOffset, leftOffset, scaleRatio);
        }

        /// <summary>
        /// Gets a clickable point on the UI Automation element based on the specified alignment, and offsets.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the clickable point of.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="topOffset">The vertical offset to apply to the clickable point.</param>
        /// <param name="leftOffset">The horizontal offset to apply to the clickable point.</param>
        /// <returns>A <see cref="PointModel"/> representing the clickable point on the element.</returns>
        public static PointModel GetClickablePoint(
            this IUIAutomationElement element, string align, int topOffset, int leftOffset)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Call the main ExportClickablePoint method with the specified alignment, offsets, and default scale ratio.
            return ExportClickablePoint(boundingRectangle, align, topOffset, leftOffset, scaleRatio: 1.0D);
        }

        /// <summary>
        /// Retrieves an <see cref="UiaElementModel"/> from the session by its ID.
        /// </summary>
        /// <param name="session">The <see cref="UiaSessionResponseModel"/> containing the elements.</param>
        /// <param name="id">The ID of the element to retrieve.</param>
        /// <returns>
        /// The <see cref="UiaElementModel"/> if found in the session; otherwise, <c>null</c>.
        /// </returns>
        public static UiaElementModel GetElement(this UiaSessionResponseModel session, string id)
        {
            // Try to get the element with the specified ID from the session's elements.
            if (!session.Elements.TryGetValue(id, out UiaElementModel cachedElement))
            {
                // Return null if the element is not found.
                return null;
            }

            // Return the cached element.
            return cachedElement;
        }

        /// <summary>
        /// Gets the scan code for the specified key.
        /// </summary>
        /// <param name="key">The key to get the scan code for.</param>
        /// <returns>The scan code for the key if it exists in the map; otherwise, <see cref="ushort.MaxValue"/>.</returns>
        public static ushort GetKeyCode(this string key)
        {
            // Try to get the scan code from the map. If the key is found, return the scan code; otherwise, return ushort.MaxValue.
            return s_CodeMap.TryGetValue(key, out ushort keyCode) ? keyCode : ushort.MaxValue;
        }

        /// <summary>
        /// Retrieves the bounding rectangle of the specified UI Automation element.
        /// </summary>
        /// <param name="element">The UI Automation element from which to retrieve the bounding rectangle.</param>
        /// <returns>A <see cref="RectangleModel"/> representing the bounding rectangle of the element.</returns>
        public static RectangleModel GetRectangle(this IUIAutomationElement element) => new()
        {
            // Retrieve the bottom coordinate of the bounding rectangle
            Bottom = element.CurrentBoundingRectangle.bottom,

            // Retrieve the left coordinate of the bounding rectangle
            Left = element.CurrentBoundingRectangle.left,

            // Retrieve the right coordinate of the bounding rectangle
            Right = element.CurrentBoundingRectangle.right,

            // Retrieve the top coordinate of the bounding rectangle
            Top = element.CurrentBoundingRectangle.top
        };

        /// <summary>
        /// Retrieves the runtime ID of an element based on the specified XPath.
        /// </summary>
        /// <param name="session">The <see cref="UiaSessionResponseModel"/> containing the document object model.</param>
        /// <param name="xpath">The XPath expression to select the element.</param>
        /// <returns>The runtime ID of the selected element if found; otherwise, <c>null</c>.</returns>
        public static string GetRuntime(this UiaSessionResponseModel session, string xpath)
        {
            // Update the document object model of the session.
            session.DocumentObjectModel = DocumentObjectModelFactory.New(session.ApplicationRoot);

            // Select the XML element based on the XPath expression.
            var xmlElement = session.DocumentObjectModel.XPathSelectElement(expression: xpath);

            // Return the value of the "id" attribute of the selected element, or null if the element is not found.
            return xmlElement?.Attribute("id").Value;
        }

        /// <summary>
        /// Retrieves the scan code associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which to retrieve the scan code.</param>
        /// <returns>The scan code associated with the specified key, or <see cref="ushort.MaxValue"/> if the key is not found.</returns>
        public static ushort GetScanCode(this string key)
        {
            // Try to get the scan code from the code map using the provided key
            var isCodes = s_CodeMap.TryGetValue(key, out ushort code);

            // If the key is found in the map, return the associated code; otherwise, return ushort.MaxValue
            return isCodes ? code : ushort.MaxValue;
        }

        /// <summary>
        /// Gets the screen resolution using device capabilities.
        /// </summary>
        /// <param name="_">An instance of <see cref="CUIAutomation8"/> (unused).</param>
        /// <returns>A <see cref="Size"/> structure that contains the width and height of the screen in pixels.</returns>
        public static Size GetScreenResolution(this CUIAutomation8 _)
        {
            // Create a Graphics object from the desktop window handle.
            var graphics = Graphics.FromHwnd(IntPtr.Zero);

            // Get the device context handle from the Graphics object.
            var desktop = graphics.GetHdc();

            // Retrieve the physical screen height using the device context handle.
            int physicalScreenHeight = User32.GetDeviceCapabilities(desktop, (int)DeviceCapabilities.Desktopvertres);

            // Retrieve the physical screen width using the device context handle.
            int physicalScreenWidth = User32.GetDeviceCapabilities(desktop, (int)DeviceCapabilities.Desktophorzres);

            // Release the device context handle.
            graphics.ReleaseHdc(desktop);

            // Dispose the Graphics object.
            graphics.Dispose();

            // Return the screen resolution as a Size object.
            return new Size(physicalScreenWidth, physicalScreenHeight);
        }

        /// <summary>
        /// Gets the tag name of the UI Automation element with a default timeout of 5 seconds.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the tag name of.</param>
        /// <returns>The tag name of the element, or an empty string if the operation fails.</returns>
        public static string GetTagName(this IUIAutomationElement element)
        {
            // Call the overloaded GetTagName method with a default timeout of 5 seconds.
            return GetTagName(element, timeout: TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Gets the tag name of the UI Automation element within the specified timeout.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the tag name of.</param>
        /// <param name="timeout">The maximum amount of time to attempt to get the tag name.</param>
        /// <returns>The tag name of the element, or an empty string if the operation fails.</returns>
        public static string GetTagName(this IUIAutomationElement element, TimeSpan timeout)
        {
            // Calculate the expiration time for the timeout.
            var expires = DateTime.Now.Add(timeout);

            // Attempt to get the tag name until the timeout expires.
            while (DateTime.Now < expires)
            {
                try
                {
                    // Get the control type field name corresponding to the element's current control type.
                    var controlType = typeof(UIA_ControlTypeIds).GetFields()
                        .Where(f => f.FieldType == typeof(int))
                        .FirstOrDefault(f => (int)f.GetValue(null) == element.CurrentControlType)?.Name;

                    // Extract and return the tag name from the control type field name.
                    return Regex.Match(input: controlType, pattern: "(?<=UIA_).*(?=ControlTypeId)").Value;
                }
                catch (COMException)
                {
                    // Ignore COM exceptions and continue attempting until the timeout expires.
                }
            }

            // Return an empty string if the tag name could not be retrieved within the timeout.
            return string.Empty;
        }

        /// <summary>
        /// Retrieves the text content of the specified UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="UiaElementModel"/> from which to retrieve the text content.</param>
        /// <returns>The text content of the element, or an empty string if no text is found.</returns>
        public static string GetText(this UiaElementModel element)
        {
            // Check if the UI Automation element is not null
            if (element.UIAutomationElement != null)
            {
                // Retrieve the text content from the UI Automation element
                return GetText(element.UIAutomationElement);
            }

            // Check if the Node property is null
            if (element.Node == null)
            {
                // Return an empty string if the Node is null
                return string.Empty;
            }

            // Return the text content from the Node's Document Root, or an empty string if it's null or empty
            return string.IsNullOrEmpty(element.Node.Document.Root.Value)
                ? string.Empty
                : element.Node.Document.Root.Value;
        }

        /// <summary>
        /// Gets the text content of the specified UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to get the text content of.</param>
        /// <returns>The text content of the element, or an empty string if no text patterns are found.</returns>
        public static string GetText(this IUIAutomationElement element)
        {
            // Retrieves a list of supported pattern IDs for the specified UI Automation element.
            static List<int> GetPatterns(IUIAutomationElement element)
            {
                // Initialize a list to store the supported pattern IDs.
                var patternsResult = new List<int>();

                // Iterate through the fields of the UIA_PatternIds type.
                foreach (var pattern in typeof(UIA_PatternIds).GetFields().Where(f => f.FieldType == typeof(int)))
                {
                    // Get the pattern ID from the field.
                    var id = (int)pattern.GetValue(null);

                    // Check if the element supports the pattern ID.
                    if (element.GetCurrentPattern(id) == null)
                    {
                        // Skip to the next pattern ID if the element does not support the current pattern ID.
                        continue;
                    }

                    // Add the supported pattern ID to the result list.
                    patternsResult.Add(id);
                }

                // Return the list of supported pattern IDs.
                return patternsResult;
            }

            // Define an array of text-related pattern IDs.
            var textPatterns = new[]
            {
                UIA_PatternIds.UIA_TextChildPatternId,
                UIA_PatternIds.UIA_TextEditPatternId,
                UIA_PatternIds.UIA_TextPattern2Id,
                UIA_PatternIds.UIA_TextPatternId,
                UIA_PatternIds.UIA_ValuePatternId
            };

            // Get the patterns supported by the element that match the text-related pattern IDs.
            var patterns = GetPatterns(element).Where(i => textPatterns.Contains(i));

            // If no text-related patterns are found, return an empty string.
            if (!patterns.Any())
            {
                return string.Empty;
            }

            // Get the first matching pattern ID.
            var id = patterns.First();

            // Get the current pattern for the element using the pattern ID.
            var pattern = element.GetCurrentPattern(id);

            // Get the text content using the TextPatternFactory.
            return TextPatternFactory.GetText(id, pattern);
        }

        /// <summary>
        /// Clicks the specified UI Automation element using the default alignment (TopLeft) and scale ratio (1.0).
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after clicking.</returns>
        public static IUIAutomationElement Invoke(this IUIAutomationElement element)
        {
            return Invoke(element, align: "MiddleCenter", scaleRatio: 1.0D);
        }

        /// <summary>
        /// Clicks the specified UI Automation element using the specified alignment and default scale ratio (1.0).
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after clicking.</returns>
        public static IUIAutomationElement Invoke(this IUIAutomationElement element, string align)
        {
            return Invoke(element, align, scaleRatio: 1.0D);
        }

        /// <summary>
        /// Clicks the specified UI Automation element using the default alignment (TopLeft) and the specified scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after clicking.</returns>
        public static IUIAutomationElement Invoke(this IUIAutomationElement element, double scaleRatio)
        {
            return Invoke(element, align: "TopLeft", scaleRatio);
        }

        /// <summary>
        /// Clicks the specified UI Automation element using the specified alignment and scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after clicking.</returns>
        public static IUIAutomationElement Invoke(this IUIAutomationElement element, string align, double scaleRatio)
        {
            // If the element is null, return the element without doing anything.
            if (element == null)
            {
                return element;
            }

            // Determine the type of pattern the element supports.
            var isInvoke = element.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) != null;
            var isExpandCollapse = !isInvoke && element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) != null;
            var isSelectable = !isInvoke && !isExpandCollapse && element.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) != null;
            var isCords = !isInvoke && !isExpandCollapse && !isSelectable;

            // Perform the appropriate action based on the pattern type.
            if (isInvoke)
            {
                // Invoke the element if it supports the Invoke pattern.
                InvokeElement(element);

                // Return the element after performing the click action.
                return element;
            }
            if (isExpandCollapse)
            {
                // Expand or collapse the element if it supports the ExpandCollapse pattern.
                SwitchExpandCollapse(element);

                // Return the element after performing the expand or collapse action.
                return element;
            }
            if (isSelectable)
            {
                // Select the element if it supports the SelectionItem pattern.
                SelectElement(element);

                // Return the element after performing the selection action.
                return element;
            }

            if (isCords)
            {
                // Create a new RectangleModel from the element's current bounding rectangle.
                var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

                // Click the element based on its coordinates if it does not support any specific pattern.
                var point = ExportClickablePoint(boundingRectangle, align, topOffset: 1, leftOffset: 1, scaleRatio);
                User32.SetPhysicalCursorPosition(point.X, point.Y);

                // Send a mouse click event to the system using the coordinates.
                var mouseDown = NewMouseInput(MouseEvent.LeftDown, point.X, point.Y);
                var mouseUp = NewMouseInput(MouseEvent.LeftUp, point.X, point.Y);
                User32.SendInput([mouseDown, mouseUp]);
            }

            // Return the element after performing the click action.
            return element;
        }

        /// <summary>
        /// Creates a new <see cref="UiaElementModel"/> with a clickable point from the coordinates specified in the location strategy value.
        /// </summary>
        /// <param name="locationStrategy">The <see cref="LocationStrategyModel"/> containing the value with coordinates.</param>
        /// <returns>
        /// An <see cref="UiaElementModel"/> with a clickable point if the value contains valid coordinates; otherwise, <c>null</c>.
        /// </returns>
        public static UiaElementModel NewPointElement(this LocationStrategyModel locationStrategy)
        {
            // Check if the location strategy value contains coordinates using a regular expression.
            var isCords = Regex.IsMatch(input: locationStrategy.Value, pattern: "(?i)//cords\\[\\d+,\\d+]");

            // Return null if the value does not contain valid coordinates.
            if (!isCords)
            {
                return null;
            }

            // Extract the coordinates from the value using a regular expression.
            var json = Regex.Match(input: locationStrategy.Value, pattern: "\\[\\d+,\\d+]").Value;

            // Deserialize the coordinates from the JSON string.
            var cords = JsonSerializer.Deserialize<int[]>(json);

            // Return an ElementModel with the deserialized coordinates as a clickable point.
            return new UiaElementModel
            {
                ClickablePoint = new PointModel(xpos: cords[0], ypos: cords[1])
            };
        }

        /// <summary>
        /// Selects the specified UI Automation element using the SelectionItem pattern.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to select.</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after selecting it.</returns>
        public static IUIAutomationElement Select(this IUIAutomationElement element)
        {
            return SelectElement(element);
        }

        /// <summary>
        /// Sends an array of input events to the system and retrieves the number of events sent and the error code.
        /// </summary>
        /// <param name="_">An instance of <see cref="CUIAutomation8"/> (unused).</param>
        /// <param name="inputs">An array of <see cref="Input"/> structures representing the input events to be sent.</param>
        /// <returns>
        /// A tuple containing the number of events successfully inserted into the input stream and the last Win32 error code.
        /// </returns>
        public static (uint NumberOfEvents, int ErrorCode) SendInputs(this CUIAutomation8 _, params Input[] inputs)
        {
            // Send the array of input events to the system.
            var numberOfEvents = User32.SendInput(inputs);

            // Retrieve the last Win32 error code.
            var errorCode = Marshal.GetLastWin32Error();

            // Return the number of events and the error code as a tuple.
            return (numberOfEvents, errorCode);
        }

        /// <summary>
        /// Sends the specified text to the UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to send the text to.</param>
        /// <param name="text">The text to send to the element.</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after the text has been sent.</returns>
        /// <exception cref="Exception">Throws the base exception if there is an error setting the value.</exception>
        public static IUIAutomationElement SendKeys(this IUIAutomationElement element, string text)
        {
            // Attempt to get the Value pattern from the element.
            var pattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);
            var isValue = pattern != null;

            // If the element is keyboard focusable, set focus to the element.
            if (element.CurrentIsKeyboardFocusable == 1)
            {
                element.SetFocus();
            }

            // If the element does not support the Value pattern, return the element.
            if (!isValue)
            {
                return element;
            }

            try
            {
                // Get the current value of the element.
                var valuePattern = (IUIAutomationValuePattern)pattern;

                // Create a new value by concatenating the existing value with the specified text.
                var value = (string.IsNullOrEmpty(valuePattern.CurrentValue) ? string.Empty : valuePattern.CurrentValue) + text;

                // Set the value of the element using the Value pattern.
                ((IUIAutomationValuePattern)pattern).SetValue(value);

                // Return the element after setting the value.
                return element;
            }
            catch (Exception e)
            {
                // Throw the base exception if there is an error setting the value.
                throw e.GetBaseException();
            }
        }

        /// <summary>
        /// Sends a key press with a modifier (e.g., Ctrl, Alt, Shift) using the specified key.
        /// </summary>
        /// <param name="_">An instance of <see cref="CUIAutomation8"/> (unused).</param>
        /// <param name="modifier">The modifier key to use (e.g., "Ctrl", "Alt", "Shift").</param>
        /// <param name="key">The key to press along with the modifier.</param>
        public static void SendModifiedKey(this CUIAutomation8 _, string modifier, string key)
        {
            // Gets the scan code for the specified key.
            static ushort GetKeyCode(string key)
            {
                // Return the scan code if found, otherwise return 0x00.
                return s_CodeMap.TryGetValue(key, out ushort keyCode) ? keyCode : (ushort)0x00;
            }

            // Get the scan code for the modifier key.
            var modifierCode = GetKeyCode(modifier);

            // Get the scan code for the main key.
            var keyCode = GetKeyCode(key);

            // Create the input structure for the modified key press.
            var inputs = NewKeyboardInput(modifierCode, keyCode);

            // Send the input to the system.
            User32.SendInput(inputs);

            // Retrieve the last Win32 error code.
            Marshal.GetLastWin32Error();
        }

        /// <summary>
        /// Sends a native click at the clickable point of the specified UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        public static void SendNativeClick(this IUIAutomationElement element)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Get the clickable point of the element.
            var point = ExportClickablePoint(
                boundingRectangle,
                align: default,
                topOffset: 1,
                leftOffset: 1,
                scaleRatio: 1.0D);

            // Set the cursor position to the specified coordinates.
            User32.SetPhysicalCursorPosition(x: point.X, y: point.Y);

            // Send the native click.
            SendNativeClick(_: default);
        }

        /// <summary>
        /// Sends a native click at the specified coordinates multiple times.
        /// </summary>
        /// <param name="_">An instance of <see cref="CUIAutomation8"/> (unused).</param>
        /// <param name="x">The x-coordinate to click.</param>
        /// <param name="y">The y-coordinate to click.</param>
        /// <param name="repeat">The number of times to repeat the click.</param>
        public static void SendNativeClick(this CUIAutomation8 _, int x, int y, int repeat)
        {
            // Repeat the click the specified number of times.
            for (int i = 0; i < repeat; i++)
            {
                // Set the cursor position to the specified coordinates.
                User32.SetPhysicalCursorPosition(x, y);

                // Send the native click.
                SendNativeClick(_: default);
            }
        }

        /// <summary>
        /// Sends a native click to the specified point on the screen a given number of times.
        /// </summary>
        /// <param name="_">The CUIAutomation8 instance.</param>
        /// <param name="point">The coordinates where the click should occur.</param>
        /// <param name="repeat">The number of times to repeat the click.</param>
        public static void SendNativeClick(this CUIAutomation8 _, PointModel point, int repeat)
        {
            // Repeat the click the specified number of times.
            for (int i = 0; i < repeat; i++)
            {
                // Set the cursor position to the specified coordinates.
                User32.SetPhysicalCursorPosition(x: point.X, y: point.Y);

                // Send the native click.
                SendNativeClick(_: default);
            }
        }

        /// <summary>
        /// Sends a native click to the specified point on the screen.
        /// </summary>
        /// <param name="_">The CUIAutomation8 instance.</param>
        /// <param name="point">The coordinates where the click should occur.</param>
        public static void SendNativeClick(this CUIAutomation8 _, PointModel point)
        {
            // Set the cursor position to the specified coordinates.
            User32.SetPhysicalCursorPosition(x: point.X, y: point.Y);

            // Send the native click.
            SendNativeClick(_: default);
        }

        /// <summary>
        /// Sends a native click at the clickable point of the specified UI Automation element, adjusted by the scale ratio.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        public static void SendNativeClick(this IUIAutomationElement element, double scaleRatio)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Get the clickable point of the element adjusted by the scale ratio.
            var point = ExportClickablePoint(
                boundingRectangle,
                align: default,
                topOffset: 1,
                leftOffset: 1,
                scaleRatio);

            // Set the cursor position to the specified coordinates.
            User32.SetPhysicalCursorPosition(point.X, point.Y);

            // Send a native click at the current cursor position.
            SendNativeClick(_: default);
        }

        /// <summary>
        /// Sends a native click at the clickable point of the specified UI Automation element with the specified alignment.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        public static void SendNativeClick(this IUIAutomationElement element, string align)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Get the clickable point of the element with the specified alignment.
            var point = ExportClickablePoint(boundingRectangle, align, topOffset: 1, leftOffset: 1, scaleRatio: 1.0D);

            // Set the cursor position to the specified coordinates.
            User32.SetPhysicalCursorPosition(point.X, point.Y);

            // Send a native click at the current cursor position.
            SendNativeClick(_: default);
        }

        /// <summary>
        /// Sends a native click at the specified coordinates.
        /// </summary>
        /// <param name="_">An instance of <see cref="CUIAutomation8"/> (unused).</param>
        /// <param name="x">The x-coordinate to click.</param>
        /// <param name="y">The y-coordinate to click.</param>
        public static void SendNativeClick(this CUIAutomation8 _, int x, int y)
        {
            // Set the cursor position to the specified coordinates.
            User32.SetPhysicalCursorPosition(x, y);

            // Send a native click at the current cursor position.
            SendNativeClick(_);
        }

        /// <summary>
        /// Sends a native click at the clickable point of the specified UI Automation element with the specified alignment, repeated a number of times.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="repeat">The number of times to repeat the click.</param>
        public static void SendNativeClick(this IUIAutomationElement element, string align, int repeat)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Get the clickable point of the element with the specified alignment.
            var point = ExportClickablePoint(boundingRectangle, align, topOffset: 1, leftOffset: 1, scaleRatio: 1.0D);

            // Set the cursor position to the clickable point.
            User32.SetPhysicalCursorPosition(point.X, point.Y);

            // Repeat the click the specified number of times.
            for (int i = 0; i < repeat; i++)
            {
                SendNativeClick(_: null);
            }
        }

        /// <summary>
        /// Sends a native click at the clickable point of the specified UI Automation element with the specified alignment and scale ratio, repeated a number of times.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to click.</param>
        /// <param name="align">The alignment for the clickable point (e.g., TOPLEFT, TOPCENTER, etc.).</param>
        /// <param name="repeat">The number of times to repeat the click.</param>
        /// <param name="scaleRatio">The scale ratio to apply to the coordinates.</param>
        public static void SendNativeClick(this IUIAutomationElement element, string align, int repeat, double scaleRatio)
        {
            // Create a new RectangleModel from the element's current bounding rectangle.
            var boundingRectangle = NewRectangleModel(element.CurrentBoundingRectangle);

            // Get the clickable point of the element with the specified alignment and scale ratio.
            var point = ExportClickablePoint(boundingRectangle, align, topOffset: 1, leftOffset: 1, scaleRatio);

            // Set the cursor position to the clickable point.
            User32.SetPhysicalCursorPosition(point.X, point.Y);

            // Repeat the click the specified number of times.
            for (int i = 0; i < repeat; i++)
            {
                SendNativeClick(_: null);
            }
        }

        /// <summary>
        /// Sends a native mouse click at the current cursor position.
        /// </summary>
        /// <param name="_">An instance of CUIAutomation8 (not used).</param>
        public static void SendNativeClick(this CUIAutomation8 _)
        {
            // Get the current cursor position.
            var point = User32.GetPhysicalCursorPosition();

            // Create a mouse input structure for the mouse down event.
            var mouseDown = NewMouseInput(MouseEvent.LeftDown, point.x, point.y);

            // Create a mouse input structure for the mouse up event.
            var mouseUp = NewMouseInput(MouseEvent.LeftUp, point.x, point.y);

            // Send the input array to simulate the mouse click.
            User32.SendInput([mouseDown, mouseUp]);
        }

        /// <summary>
        /// Sets the cursor position to the specified coordinates.
        /// </summary>
        /// <param name="automation">An instance of <see cref="CUIAutomation8"/>.</param>
        /// <param name="x">The x-coordinate to set the cursor position to.</param>
        /// <param name="y">The y-coordinate to set the cursor position to.</param>
        /// <returns>The same instance of <see cref="CUIAutomation8"/> after setting the cursor position.</returns>
        public static CUIAutomation8 SetCursorPosition(this CUIAutomation8 automation, int x, int y)
        {
            // Set the cursor position to the specified coordinates.
            User32.SetPhysicalCursorPosition(x, y);

            // Return the automation instance.
            return automation;
        }

        /// <summary>
        /// Toggles the expand/collapse state of the specified UI Automation element.
        /// </summary>
        /// <param name="element">The <see cref="IUIAutomationElement"/> to toggle the expand/collapse state of.</param>
        /// <returns>The same <see cref="IUIAutomationElement"/> after toggling the state.</returns>
        public static IUIAutomationElement SwitchExpandCollapseState(this IUIAutomationElement element)
        {
            return SwitchExpandCollapse(element);
        }

        /// <summary>
        /// Updates the Document Object Model (DOM) for the specified UI Automation session.
        /// </summary>
        /// <param name="session">The <see cref="UiaSessionResponseModel"/> to update.</param>
        /// <returns>The same <see cref="UiaSessionResponseModel"/> with the updated DOM.</returns>
        public static UiaSessionResponseModel UpdateDocumentObjectModel(this UiaSessionResponseModel session)
        {
            // Create a new Document Object Model (DOM) for the application's root element.
            var objectModel = DocumentObjectModelFactory.New(session.ApplicationRoot);

            // If the object model creation failed, return the session without any changes.
            if (objectModel == null)
            {
                return session;
            }

            // Update the session's Document Object Model (DOM) with the new object model.
            session.DocumentObjectModel = objectModel;

            // Return the updated session.
            return session;
        }

        // Confirms if a key is a modified key and returns the modifier and key code.
        private static (bool Modified, ushort Modifier, ushort KeyCode) ConfirmModifiedKey(string input)
        {
            // Initialize a list to store information about modified keys.
            var info = new List<(string Input, ushort Modifier, ushort ModifiedKeyCode)>();

            // Add entries for modified keys.
            info.AddRange(
            [
                (Input: "!", Modifier: 0x2A, ModifiedKeyCode: 0x02), // Shift + 1
                (Input: "@", Modifier: 0x2A, ModifiedKeyCode: 0x03), // Shift + 2
                (Input: "#", Modifier: 0x2A, ModifiedKeyCode: 0x04), // Shift + 3
                (Input: "$", Modifier: 0x2A, ModifiedKeyCode: 0x05), // Shift + 4
                (Input: "%", Modifier: 0x2A, ModifiedKeyCode: 0x06), // Shift + 5
                (Input: "^", Modifier: 0x2A, ModifiedKeyCode: 0x07), // Shift + 6
                (Input: "&", Modifier: 0x2A, ModifiedKeyCode: 0x08), // Shift + 7
                (Input: "*", Modifier: 0x2A, ModifiedKeyCode: 0x09), // Shift + 8
                (Input: "(", Modifier: 0x2A, ModifiedKeyCode: 0x0A), // Shift + 9
                (Input: ")", Modifier: 0x2A, ModifiedKeyCode: 0x0B), // Shift + 0
                (Input: "_", Modifier: 0x2A, ModifiedKeyCode: 0x0C), // Shift + -
                (Input: "+", Modifier: 0x2A, ModifiedKeyCode: 0x0D), // Shift + =
                (Input: ":", Modifier: 0x2A, ModifiedKeyCode: 0x27), // Shift + ';' (Colon)
                (Input: "<", Modifier: 0x2A, ModifiedKeyCode: 0x33), // Shift + ,
                (Input: ">", Modifier: 0x2A, ModifiedKeyCode: 0x34), // Shift + .
                (Input: "?", Modifier: 0x2A, ModifiedKeyCode: 0x35), // Shift + /
                (Input: "{", Modifier: 0x2A, ModifiedKeyCode: 0x1A), // Shift + [
                (Input: "}", Modifier: 0x2A, ModifiedKeyCode: 0x1B), // Shift + ]
                (Input: "|", Modifier: 0x2A, ModifiedKeyCode: 0x2B), // Shift + \
                (Input: "~", Modifier: 0x2A, ModifiedKeyCode: 0x29)  // Shift + `
            ]);

            // Check if the input key is in the list of modified keys.
            var isModified = info.Exists(i => i.Input.Equals(input));

            // Get the modifier and modified key code if the key is modified, otherwise set to 0x00.
            var modifier = isModified ? info.First(i => i.Input.Equals(input)).Modifier : (ushort)0x00;
            var modifiedKeyCode = isModified ? info.First(i => i.Input.Equals(input)).ModifiedKeyCode : (ushort)0x00;

            // Return the results as a tuple.
            return (isModified, modifier, modifiedKeyCode);
        }

        // Gets a clickable point on the UI Automation element based on the specified alignment, offsets, and scale ratio.
        private static PointModel ExportClickablePoint(
            RectangleModel boundingRectangle, string align, int topOffset, int leftOffset, double scaleRatio)
        {
            // Local function to get the center point of the element, adjusted by the scale ratio.
            static PointModel GetPoint(double scaleRatio, RectangleModel boundingRectangle)
            {
                // Ensure the scale ratio is valid.
                scaleRatio = scaleRatio <= 0 ? 1 : scaleRatio;

                // Calculate the horizontal delta (half the width of the bounding rectangle).
                var hDelta = (boundingRectangle.Right - boundingRectangle.Left) / 2;

                // Calculate the vertical delta (half the height of the bounding rectangle).
                var vDelta = (boundingRectangle.Bottom - boundingRectangle.Top) / 2;

                // Calculate the x-coordinate of the center point, adjusted by the scale ratio.
                var x = (int)((boundingRectangle.Left + hDelta) / scaleRatio);

                // Calculate the y-coordinate of the center point, adjusted by the scale ratio.
                var y = (int)((boundingRectangle.Top + vDelta) / scaleRatio);

                // Return the center point as a PointModel object.
                return new PointModel(xpos: x, ypos: y);
            }

            // Extract the bottom edge of the bounding rectangle.
            var bottom = boundingRectangle.Bottom;

            // Extract the left edge of the bounding rectangle.
            var left = boundingRectangle.Left;

            // Extract the right edge of the bounding rectangle.
            var right = boundingRectangle.Right;

            // Extract the top edge of the bounding rectangle.
            var top = boundingRectangle.Top;

            // Ensure the scale ratio is valid.
            scaleRatio = scaleRatio <= 0 ? 1 : scaleRatio;

            // Calculate the clickable point based on the specified alignment.
            switch (align.ToUpper())
            {
                case "TOPLEFT":
                    // Calculate the point at the top-left corner.
                    return new PointModel
                    {
                        X = (int)(left / scaleRatio) + leftOffset,
                        Y = (int)(top / scaleRatio) + topOffset
                    };

                case "TOPCENTER":
                    // Calculate the point at the top-center.
                    var horizontalDeltaTopCenter = (right - left) / 2;
                    return new PointModel
                    {
                        X = (int)((left + horizontalDeltaTopCenter) / scaleRatio) + leftOffset,
                        Y = (int)(top / scaleRatio) + topOffset
                    };

                case "TOPRIGHT":
                    // Calculate the point at the top-right corner.
                    return new PointModel
                    {
                        X = (int)(right / scaleRatio) + leftOffset,
                        Y = (int)(top / scaleRatio) + topOffset
                    };

                case "MIDDLELEFT":
                    // Calculate the point at the middle-left.
                    var verticalDeltaMiddleLeft = (bottom - top) / 2;
                    return new PointModel
                    {
                        X = (int)(left / scaleRatio) + leftOffset,
                        Y = (int)((top + verticalDeltaMiddleLeft) / scaleRatio) + topOffset
                    };

                case "MIDDLERIGHT":
                    // Calculate the point at the middle-right.
                    var verticalDeltaMiddleRight = (bottom - top) / 2;
                    return new PointModel
                    {
                        X = (int)(right / scaleRatio) + leftOffset,
                        Y = (int)((top + verticalDeltaMiddleRight) / scaleRatio) + topOffset
                    };

                case "BOTTOMLEFT":
                    // Calculate the point at the bottom-left corner.
                    return new PointModel
                    {
                        X = (int)(left / scaleRatio) + leftOffset,
                        Y = (int)(bottom / scaleRatio) + topOffset
                    };

                case "BOTTOMCENTER":
                    // Calculate the point at the bottom-center.
                    var horizontalDeltaBottomCenter = (right - left) / 2;
                    return new PointModel
                    {
                        X = (int)((left + horizontalDeltaBottomCenter) / scaleRatio) + leftOffset,
                        Y = (int)(bottom / scaleRatio) + topOffset
                    };

                case "BOTTOMRIGHT":
                    // Calculate the point at the bottom-right corner.
                    return new PointModel
                    {
                        X = (int)(right / scaleRatio) + leftOffset,
                        Y = (int)(bottom / scaleRatio) + topOffset
                    };

                default:
                    // Return the center point of the element if the alignment is not recognized.
                    return GetPoint(scaleRatio, boundingRectangle);
            }
        }

        // Retrieves the root UI Automation element for the application based on the session details.
        private static IUIAutomationElement GetApplicationRootElement(UiaSessionResponseModel session)
        {
            // Retrieves the root UI Automation element for the application from File Explorer based on the session details.
            static IUIAutomationElement GetFromFileExplorer(UiaSessionResponseModel session)
            {
                // Retrieves an IUIAutomationElement representing a window from File Explorer based on the session's folder path.
                static IUIAutomationElement Get(UiaSessionResponseModel session)
                {
                    // Define property and control type IDs.
                    const int NameProperty = UIA_PropertyIds.UIA_NamePropertyId;
                    const int ControlTypeProperty = UIA_PropertyIds.UIA_ControlTypePropertyId;
                    const int ControlTypeWindow = UIA_ControlTypeIds.UIA_WindowControlTypeId;
                    const int ControlTypeToolBar = UIA_ControlTypeIds.UIA_ToolBarControlTypeId;

                    // Get the folder path from the session's application start info arguments.
                    var folder = session.Application.StartInfo.Arguments;

                    // Get the last part of the folder path (folder name).
                    var folderLast = new DirectoryInfo(folder).Name;

                    // Create conditions to match the window by control type and name.
                    var windowCondition = session.Automation.CreatePropertyCondition(ControlTypeProperty, ControlTypeWindow);
                    var windowPartialNameCondition = session.Automation.CreatePropertyCondition(NameProperty, folderLast);
                    var windowFullNameCondition = session.Automation.CreatePropertyCondition(NameProperty, folder);
                    var windowNameCondition = session.Automation.CreateOrCondition(windowFullNameCondition, windowPartialNameCondition);
                    var windowRootCondition = session.Automation.CreateAndCondition(windowCondition, windowNameCondition);

                    // Create a condition to match toolbars by control type.
                    var toolBarCondition = session
                        .Automation
                        .CreatePropertyCondition(ControlTypeProperty, ControlTypeToolBar);

                    // Find the first window element that matches the window root condition.
                    var window = session
                        .Automation
                        .GetRootElement()
                        .FindFirst(TreeScope.TreeScope_Descendants, windowRootCondition);

                    // Find all toolbar elements within the window.
                    var toolBars = window.FindAll(TreeScope.TreeScope_Descendants, toolBarCondition);
                    var names = new List<string>();

                    // Iterate through the toolbar elements and get their names.
                    for (int toolBar = 0; toolBar < toolBars.Length; toolBar++)
                    {
                        names.Add($"{toolBars.GetElement(toolBar)?.GetCurrentPropertyValue(NameProperty)}");
                    }

                    // Check if any toolbar name contains the folder path.
                    var isWindow = names.Exists(i => i.Contains(folder, StringComparison.OrdinalIgnoreCase));

                    // Return the window element if found; otherwise, return null.
                    return isWindow ? window : null;
                }

                // Check if the directory specified in the session's application start info arguments exists.
                if (!Directory.Exists(session.Application.StartInfo.Arguments))
                {
                    return null;
                }

                IUIAutomationElement rootElement = null;
                var timeout = DateTime.Now.Add(session.Timeout);

                // Loop until the current time exceeds the timeout time.
                while (DateTime.Now < timeout)
                {
                    // Attempt to get the root element using the session details.
                    rootElement = Get(session);

                    // If the root element is found, update the session runtime and return the root element.
                    if (rootElement != null)
                    {
                        session.Runtime = rootElement.GetRuntimeId().Cast<int>().ToArray();
                        return rootElement;
                    }

                    // Sleep for a short period before trying again.
                    Thread.Sleep(100);
                }

                // Return the root element if found, otherwise return null.
                return rootElement;
            }

            // Retrieves the root UI Automation element from the application based on the session details.
            static IUIAutomationElement GetFromApplication(UiaSessionResponseModel session)
            {
                // Creates a UI Automation condition based on the session's runtime, handle, or process ID.
                static IUIAutomationCondition GetCondition(UiaSessionResponseModel session)
                {
                    // Check if the session has a runtime ID.
                    var isRuntime = session.Runtime?.Length > 0;

                    // Check if the application has a main window handle.
                    var isHandle = session.Application.MainWindowHandle != default;

                    // Determine the property ID to use for the condition.
                    var id = isRuntime
                        ? UIA_PropertyIds.UIA_RuntimeIdPropertyId
                        : UIA_PropertyIds.UIA_NativeWindowHandlePropertyId;

                    // If neither runtime nor handle are available, use the process ID property.
                    if (!isRuntime && !isHandle)
                    {
                        id = UIA_PropertyIds.UIA_ProcessIdPropertyId;
                    }

                    // Create a condition based on the runtime ID if available.
                    if (isRuntime)
                    {
                        return session.Automation.CreatePropertyCondition(id, session.Runtime.ToArray());
                    }

                    // Create a condition based on the main window handle if available.
                    // Otherwise, create a condition based on the process ID.
                    return isHandle
                        ? session.Automation.CreatePropertyCondition(id, session.Application.MainWindowHandle)
                        : session.Automation.CreatePropertyCondition(id, session.Application.Id);
                }

                // Declare the variable to hold the root element.
                IUIAutomationElement rootElement;

                // Calculate the timeout time by adding the session's timeout duration to the current time.
                var timeout = DateTime.Now.Add(session.Timeout);

                // Loop until the timeout is reached.
                while (DateTime.Now < timeout)
                {
                    // Get the condition to find the root element.
                    var condition = GetCondition(session);

                    // Get the root element of the automation tree.
                    var root = session.Automation.GetRootElement();

                    // Find the first element that matches the condition within the descendants of the root element.
                    rootElement = root.FindFirst(TreeScope.TreeScope_Descendants, condition);

                    // If the root element is found, update the session runtime and return the root element.
                    if (rootElement != null)
                    {
                        // Update the session's runtime ID with the runtime ID of the found root element.
                        session.Runtime = rootElement.GetRuntimeId().Cast<int>().ToArray();

                        // Return the found root element.
                        return rootElement;
                    }

                    // Sleep for a short period before trying again.
                    Thread.Sleep(100);
                }

                // Return null if the root element is not found within the timeout period.
                return null;
            }

            // Define constants for comparison and the explorer executable name.
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;
            const string Explorer = "EXPLORER.EXE";

            // Check if the application's name or file contains "EXPLORER.EXE" (case insensitive).
            // If it is an explorer application, get the root element from the file explorer.
            // Otherwise, get the root element from the application.
            return session.Application.GetNameOrFile().Contains(Explorer, Compare)
                ? GetFromFileExplorer(session)
                : GetFromApplication(session);
        }

        // Invokes the specified UI Automation element using the Invoke pattern.
        private static void InvokeElement(IUIAutomationElement element)
        {
            // Get the Invoke pattern from the element.
            var currentPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId);
            var pattern = (IUIAutomationInvokePattern)currentPattern;

            // If the pattern is null, the element does not support the Invoke pattern, so return.
            if (pattern == null)
            {
                return;
            }

            try
            {
                // Try to invoke the element using the Invoke pattern.
                pattern.Invoke();
            }
            catch (Exception e)
            {
                // If an exception occurs, throw the base exception.
                throw e.GetBaseException();
            }
        }

        // Creates a sequence of keyboard input events for pressing and releasing a key with a modifier (e.g., Shift + key).
        private static Input[] NewKeyboardInput(ushort modifierKeyCode, ushort keyCode) =>
        [
            // Press the modifier key down.
            NewKeyboardInput(modifierKeyCode, KeyEvent.KeyDown | KeyEvent.Scancode),

            // Press the main key down.
            NewKeyboardInput(keyCode, KeyEvent.KeyDown | KeyEvent.Scancode),

            // Release the main key.
            NewKeyboardInput(keyCode, KeyEvent.KeyUp | KeyEvent.Scancode),

            // Release the modifier key.
            NewKeyboardInput(modifierKeyCode, KeyEvent.KeyUp | KeyEvent.Scancode),
        ];

        // Creates a new keyboard input event with the specified key code and keyboard events.
        private static Input NewKeyboardInput(ushort keyCode, KeyEvent keyboardEvents) => new()
        {
            // Set the type of the input event to keyboard.
            type = (int)SendInputEventType.Keyboard,

            // Initialize the union structure for input event.
            union = new InputUnion
            {
                ki = new KeyboardInput
                {
                    wVk = 0,                                          // Virtual-key code is set to 0 because the scan code is used.
                    wScan = keyCode,                                  // The hardware scan code for the key.
                    dwFlags = (uint)keyboardEvents,                   // Flags specifying various aspects of the keystroke.
                    dwExtraInfo = User32.GetMessageExtraInformation() // Additional information associated with the event.
                }
            }
        };

        // Creates a new mouse input event with the specified mouse events and coordinates.
        private static Input NewMouseInput(MouseEvent mouseEvents, int x, int y) => new()
        {
            // Set the type of the input event to keyboard.
            type = (int)SendInputEventType.Mouse,

            // Initialize the union structure for input event.
            union = new InputUnion
            {
                mi = new MouseInput
                {
                    dx = x,
                    dy = y,
                    dwFlags = (uint)mouseEvents,
                    dwExtraInfo = User32.GetMessageExtraInformation()
                }
            }
        };

        // Create a bounding rectangle model from the element's current bounding rectangle.
        private static RectangleModel NewRectangleModel(this tagRECT tagRECT) => new()
        {
            Bottom = tagRECT.bottom,
            Left = tagRECT.left,
            Right = tagRECT.right,
            Top = tagRECT.top
        };

        // Selects the specified element using the SelectionItem pattern.
        private static IUIAutomationElement SelectElement(IUIAutomationElement element)
        {
            // Attempt to get the SelectionItem pattern from the element.
            var pattern = element?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId);

            // Check if the pattern is of type IUIAutomationSelectionItemPattern.
            if (pattern is not IUIAutomationSelectionItemPattern selectionItemPattern)
            {
                return element;
            }

            try
            {
                // Set focus to the element.
                element.SetFocus();

                // Select the element using the SelectionItem pattern.
                selectionItemPattern.Select();
            }
            catch (Exception e)
            {
                // Throw the base exception if an error occurs.
                throw e.GetBaseException();
            }

            // Return the element after selecting it.
            return element;
        }

        // Toggles the expand/collapse state of the specified UI Automation element.
        private static IUIAutomationElement SwitchExpandCollapse(IUIAutomationElement element)
        {
            // Attempt to get the ExpandCollapse pattern from the element.
            var currentPattern = element.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId);
            var pattern = (IUIAutomationExpandCollapsePattern)currentPattern;

            // Check the current state of the element.
            if (pattern.CurrentExpandCollapseState != ExpandCollapseState.ExpandCollapseState_Collapsed)
            {
                // If the element is not collapsed, collapse it.
                pattern.Collapse();

                // Return the element after toggling the state.
                return element;
            }

            try
            {
                // If the element is collapsed, expand it.
                pattern.Expand();
            }
            catch (InvalidOperationException)
            {
                // If expanding the element fails, set focus to the element and try again.
                element.SetFocus();
                pattern.Expand();
            }

            // Return the element after toggling the state.
            return element;
        }
    }
}
