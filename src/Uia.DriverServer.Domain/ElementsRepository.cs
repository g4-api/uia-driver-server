/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

using Uia.DriverServer.Attributes;
using Uia.DriverServer.Extensions;
using Uia.DriverServer.Models;

using UIAutomationClient;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Represents the repository for element-related operations.
    /// </summary>
    /// <param name="sessions">The dictionary containing session models.</param>
    public class ElementsRepository(IDictionary<string, UiaSessionModel> sessions) : IElementsRepository
    {
        // Initialize the sessions dictionary containing session models as a private readonly
        // field for the repository class instance to use internally and externally as needed
        // for element-related operations.
        private readonly IDictionary<string, UiaSessionModel> _sessions = sessions;

        /// <inheritdoc />
        public (int Status, UiaElementModel ElementModel) FindElement(string session, LocationStrategyModel locationStrategy)
        {
            // Call the overloaded FindElement method with an empty element string
            return FindElement(session, element: string.Empty, locationStrategy);
        }

        /// <inheritdoc />
        public (int Status, UiaElementModel ElementModel) FindElement(string session, string element, LocationStrategyModel locationStrategy)
        {
            // Try to retrieve the session model from the sessions dictionary
            var isSession = _sessions.TryGetValue(session, out UiaSessionModel uiaSession);

            // If the session is not found, return a 404 status code
            if (!isSession)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // If an element identifier is provided, get the element; otherwise, set it to default
            var uiaElement = !string.IsNullOrEmpty(element)
                ? GetElementBySession(_sessions, session, element)
                : default;

            // Get the locator hierarchy and determine if the root is included
            var (isRoot, hierarchy) = FormatLocatorHierarchy(locationStrategy);

            // If the hierarchy is empty, return a 400 status code indicating a bad request
            if (hierarchy.Length == 0)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // Determine the root element based on whether the root is included in the hierarchy
            var rootElement = isRoot
                ? new CUIAutomation8().GetRootElement()
                : uiaSession.ApplicationRoot;

            // If the element has a valid UI Automation element and the root is not included, use the element as the root
            if (uiaElement?.UIAutomationElement != null && !isRoot)
            {
                rootElement = uiaElement.UIAutomationElement;
            }

            // Setup the output element model with the root element
            var outputElement = new UiaElementModel
            {
                UIAutomationElement = rootElement
            };

            // Iterate through the hierarchy to find the element by each segment
            foreach (var pathSegment in hierarchy)
            {
                // Find the element by the current segment and update the root
                // element accordingly for the next segment search iteration
                outputElement = FindElementBySegment(new CUIAutomation8(), outputElement.UIAutomationElement, pathSegment);

                // Setup flags to check if the element is not found, the rectangle is not found, or the clickable point is not found
                var notFound = outputElement?.UIAutomationElement == default;
                var notFoundRectangle = outputElement?.Rectangle == default;
                var notFoundClickablePoint = outputElement?.ClickablePoint == default;

                // If the root element is not found at any segment, return a 404 status code
                if (notFound && notFoundRectangle && notFoundClickablePoint)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
            }

            // Add or update the element in the session's elements dictionary
            uiaSession.Elements[outputElement.Id] = outputElement;

            // Return the status code and the found element model
            return (StatusCodes.Status200OK, outputElement);
        }

        /// <inheritdoc />
        public UiaElementModel GetElement(string session, string element)
        {
            return GetElementBySession(_sessions, session, element);
        }

        /// <inheritdoc />
        public (int StatusCode, string Value) GetElementAttribute(string session, string element, string name)
        {
            // Retrieve the element object from the session elements dictionary based on the session and element identifiers
            var elementModel = GetElementBySession(_sessions, session, element);

            // If the element is not found, return a 404 status code
            if (elementModel == null)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // Retrieve the attribute value from the element model
            var attribute = elementModel.GetAttribute(name);

            // Return the appropriate status code and attribute value
            return string.IsNullOrEmpty(attribute)
                ? (StatusCodes.Status404NotFound, string.Empty)
                : (StatusCodes.Status200OK, attribute);
        }

        /// <inheritdoc />
        public (int StatusCode, string Text) GetElementText(string session, string element)
        {
            // Retrieve the element object from the session elements dictionary based on the session and element identifiers
            var elementModel = GetElementBySession(_sessions, session, element);

            // If the element is not found, return a 404 status code
            if (elementModel == null)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // Retrieve the text content from the element model
            var value = elementModel.GetText();

            // Return the appropriate status code and text content
            return string.IsNullOrEmpty(value)
                ? (StatusCodes.Status404NotFound, string.Empty)
                : (StatusCodes.Status200OK, value);
        }

        // Parses the locator strategy to determine if it starts from the desktop and extracts the hierarchy of segments.
        private static (bool FromDesktop, string[] Hierarchy) FormatLocatorHierarchy(LocationStrategyModel locationStrategy)
        {
            // Extract values enclosed in single quotes
            var values = Regex.Matches(locationStrategy.Value, "(?<==').+?(?=')").Select(i => i.Value).ToArray();

            // Determine if the locator starts from the desktop
            var isRoot = Regex.IsMatch(locationStrategy.Value, @"(?is)^(\(+)?\/(root|desktop)");

            // Remove the desktop/root prefix from the locator value if present
            var xpath = isRoot
                ? Regex.Replace(locationStrategy.Value, @"(?is)^(\(+)?\/(root|desktop)", string.Empty)
                : locationStrategy.Value;

            // Create a dictionary to store tokens and their corresponding values
            var tokens = new Dictionary<string, string>();
            for (int i = 0; i < values.Length; i++)
            {
                tokens[$"value_token_{i}"] = values[i];
                xpath = xpath.Replace(values[i], $"value_token_{i}");
            }

            // Split the xpath into segments
            var hierarchy = Regex
                .Split(xpath, @"\/(?=\w+|\*)(?![^\[]*\])")
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // Adjust segments if they start with a '/'
            for (int i = 0; i < hierarchy.Length; i++)
            {
                var segment = hierarchy[i];
                if (!segment.Equals("/") && !segment.EndsWith('/'))
                {
                    continue;
                }
                hierarchy[i + 1] = $"/{hierarchy[i + 1]}";
            }

            // Clean up the hierarchy by removing trailing '/' and empty segments
            hierarchy = hierarchy
                .Where(i => !string.IsNullOrEmpty(i) && !i.Equals("/"))
                .Select(i => i.TrimEnd('/'))
                .ToArray();

            // Replace tokens in the hierarchy with their original values
            for (int i = 0; i < hierarchy.Length; i++)
            {
                foreach (var token in tokens)
                {
                    hierarchy[i] = hierarchy[i].Replace(token.Key, token.Value);
                }
            }

            // Return the flag indicating if the locator starts from the desktop and the hierarchy of segments
            return (isRoot, hierarchy);
        }

        // Retrieves an element from the session's elements dictionary.
        private static UiaElementModel GetElementBySession(IDictionary<string, UiaSessionModel> sessions, string session, string element)
        {
            // Try to retrieve the session model from the sessions dictionary
            if (!sessions.TryGetValue(session, out UiaSessionModel sessionModel))
            {
                // Return default if the session is not found
                return default;
            }

            // Check if the session model's elements dictionary contains the specified element
            if (sessionModel.Elements?.ContainsKey(element) != true)
            {
                // Return default if the element is not found
                return default;
            }

            // Return the found element
            return sessionModel.Elements[element];
        }

#pragma warning disable IDE0051, S3011 // These methods are used via reflection to handle specific locator segment types.
        // Finds an element by a specified segment in the UI Automation tree.
        private UiaElementModel FindElementBySegment(CUIAutomation8 session, IUIAutomationElement rootElement, string pathSegment)
        {
            // Get all methods from the current type that have the UiaSegmentTypeAttribute
            var segmentMethods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(i => i.GetCustomAttribute<UiaSegmentTypeAttribute>() != null)
                .ToDictionary(i => i.GetCustomAttribute<UiaSegmentTypeAttribute>()?.Type, i => i, StringComparer.OrdinalIgnoreCase);

            // Extract the segment key from the path segment using a regular expression
            var segmentKey = Regex.Match(input: pathSegment, pattern: @"(?<=^\/?)\w+").Value;

            // Try to get the method for the segment key from the dictionary
            var isSegmentType = segmentMethods.TryGetValue(key: segmentKey, out MethodInfo method);

            // If the segment key is not found, use the default method for 'Uia'
            method = isSegmentType
                ? method
                : segmentMethods["Uia"];

            // Invoke the method with the session, root element, and path segment as parameters
            return (UiaElementModel)method.Invoke(null, [new SegmentDataModel()
            {
                PathSegment = pathSegment,
                RootElement = rootElement,
                Session = session
            }]);
        }

        // Finds an element by the specified segment using the 'Cords' segment type.
        [UiaSegmentType(type: "Coords")]
        private static UiaElementModel ByCoords(SegmentDataModel segmentData)
        {
            // Extract the OCR segment from the path segment using a regular expression
            var segment = Regex.Match(input: segmentData.PathSegment, pattern: @"(?is)(?<=Coords\().*?(?=\))").Value;
            var coords = segment.Split(',').Select(int.Parse).ToArray();

            // Create a clickable point from the coordinates
            var point = new PointModel
            {
                X = coords[0],
                Y = coords[1]
            };

            // Generate a new ID for the UIA element
            var id = $"{Guid.NewGuid()}";

            // Create an XML representation of the OCR element with the word and rectangle information.
            var xml = "<PointElement " +
                $"X=\"{point.X}\" " +
                $"Y=\"{point.Y}\" " +
                $"Id=\"{id}\" />";

            // Return a new UIA element model with the clickable point and a generated ID
            return new UiaElementModel
            {
                ClickablePoint = point,
                Id = id,
                Node = XDocument.Parse(xml).Root
            };
        }

        // Finds an element using the Object Model, converting the UI Automation tree to XML and finding by XPath.
        [UiaSegmentType(type: "ObjectModel")]
        private static UiaElementModel ByObjectModel(SegmentDataModel segmentData)
        {
            // Remove any namespace prefixes from the XPath
            var xpath = Regex.Replace(input: segmentData.PathSegment, pattern: @"(?<=^\/?)\w+:", replacement: string.Empty);
            xpath = "/" + xpath;

            // Create a new Document Object Model (DOM) from the root element
            var objectModel = new DocumentObjectModelFactory(segmentData.RootElement).New();

            // Select the element by XPath and get its 'id' attribute
            var idAttribute = objectModel.XPathSelectElement(xpath)?.Attribute("id")?.Value;

            // If the 'id' attribute is not found, return default
            if (idAttribute == null)
            {
                return default;
            }

            // Deserialize the 'id' attribute to an integer array
            var id = JsonSerializer.Deserialize<int[]>(idAttribute);

            // Create a condition to find the element by its runtime ID
            var condition = segmentData.Session.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, id);

            // Determine the tree scope based on the path segment
            var treeScope = segmentData.PathSegment.StartsWith('/')
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;

            // Find and return the first element that matches the condition within the specified scope
            var element = segmentData.RootElement.FindFirst(treeScope, condition);

            return new UiaElementModel
            {
                UIAutomationElement = element
            };
        }

        // Finds an element using OCR (Optical Character Recognition) based on the specified segment data.
        [UiaSegmentType(type: "Ocr")]
        private static UiaElementModel ByOcr(SegmentDataModel segmentData)
        {
            // Initialize the OCR repository
            var ocr = new OcrRepository();

            // Extract the OCR segment from the path segment using a regular expression
            var segment = Regex.Match(input: segmentData.PathSegment, pattern: @"(?is)(?<=Ocr\().*?(?=\))").Value;

            // Find the element using OCR and return the result
            return ocr.FindElement(segment);
        }

        // Finds an element using the UI Automation (Uia) tree.
        [UiaSegmentType(type: "Uia")]
        private static UiaElementModel ByUia(SegmentDataModel segmentData)
        {
            // Determine if the search scope is descendants or children based on the path segment
            var isDescendants = segmentData.PathSegment.StartsWith('/');

            var treeScope = isDescendants
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;

            // Initialize the condition object
            IUIAutomationCondition condition = XpathParser.ConvertToCondition(segmentData.PathSegment);

            // Extract the index value from the path segment
            var indexValue = Regex.Match(input: segmentData.PathSegment, pattern: @"(?<=\[)\d+(?=])").Value;

            // Try to parse the index value
            var isIndex = int.TryParse(indexValue, out int indexOut);

            // Find the first element that matches the condition if no index is specified
            if (!isIndex)
            {
                return segmentData.RootElement.FindFirst(treeScope, condition).ConvertToElement();
            }

            // Find all elements that match the condition
            var elements = segmentData.RootElement.FindAll(treeScope, condition);

            // Adjust the index to be zero-based
            var index = indexOut < 1 ? 0 : indexOut - 1;

            // Return the element at the specified index or default if none found
            return elements.Length == 0
                ? default
                : elements.GetElement(index).ConvertToElement();
        }
#pragma warning restore IDE0051, S3011

        /// <summary>
        /// Represents the data model for a segment used in UI Automation.
        /// </summary>
        private sealed class SegmentDataModel
        {
            /// <summary>
            /// Gets or sets the path segment used to locate the element.
            /// </summary>
            /// <value>A <see cref="string"/> representing the path segment.</value>
            public string PathSegment { get; set; }

            /// <summary>
            /// Gets or sets the root element to start the search from.
            /// </summary>
            /// <value>The <see cref="IUIAutomationElement"/> representing the root element.</value>
            public IUIAutomationElement RootElement { get; set; }

            /// <summary>
            /// Gets or sets the UI Automation session.
            /// </summary>
            /// <value>The <see cref="CUIAutomation8"/> instance representing the UI Automation session.</value>
            public CUIAutomation8 Session { get; set; }
        }
    }
}
