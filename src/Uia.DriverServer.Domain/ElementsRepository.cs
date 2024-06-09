using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Uia.DriverServer.Models;

using Uia.DriverServer.Extensions;
using System.Reflection;

using UIAutomationClient;

using System.Xml.XPath;
using Uia.DriverServer.Attributes;
using System.IO;

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
                ? GetElement(_sessions, session, element)
                : default;

            // Get the locator hierarchy and determine if the root is included
            var (isRoot, hierarchy) = GetLocatorHierarchy(locationStrategy);

            // If the hierarchy is empty, return a 400 status code indicating a bad request
            if (!hierarchy.Any())
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

                // If the root element is not found at any segment, return a 404 status code
                if (outputElement.UIAutomationElement == default)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
            }

            // Add or update the element in the session's elements dictionary
            uiaSession.Elements[outputElement.Id] = outputElement;

            // Return the status code and the found element model
            return (StatusCodes.Status200OK, outputElement);
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
            return (UiaElementModel)method.Invoke(null, [session, rootElement, pathSegment]);
        }

        // Finds an element using the UI Automation (Uia) tree.
        [UiaSegmentType(type: "Uia")]
        private static UiaElementModel ByUia(CUIAutomation8 session, IUIAutomationElement rootElement, string pathSegment)
        {
            // Get the control type condition based on the path segment
            var controlTypeCondition = GetControlTypeCondition(session, pathSegment);

            // Get the property condition based on the path segment
            var propertyCondition = GetPropertyCondition(session, pathSegment);

            // Determine if the search scope is descendants or children based on the path segment
            var isDescendants = pathSegment.StartsWith('/');

            var treeScope = isDescendants
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;

            // Initialize the condition object
            IUIAutomationCondition condition;

            // Combine control type and property conditions if both are present
            if (controlTypeCondition == default && propertyCondition != default)
            {
                condition = propertyCondition;
            }
            // Use control type condition if property condition is not present
            else if (controlTypeCondition != default && propertyCondition == default)
            {
                condition = controlTypeCondition;
            }
            // Combine control type and property conditions if both are present
            else if (controlTypeCondition != default)
            {
                condition = session.CreateAndCondition(controlTypeCondition, propertyCondition);
            }
            // Return default if no conditions are met
            else
            {
                return default;
            }

            // Extract the index value from the path segment
            var indexValue = Regex.Match(input: pathSegment, pattern: @"(?<=\[)\d+(?=])").Value;

            // Try to parse the index value
            var isIndex = int.TryParse(indexValue, out int indexOut);

            // Find the first element that matches the condition if no index is specified
            if (!isIndex)
            {
                return rootElement.FindFirst(treeScope, condition).ConvertToElement();
            }

            // Find all elements that match the condition
            var elements = rootElement.FindAll(treeScope, condition);

            // Adjust the index to be zero-based
            var index = indexOut < 1 ? 0 : indexOut - 1;

            // Return the element at the specified index or default if none found
            return elements.Length == 0
                ? default
                : elements.GetElement(index).ConvertToElement();
        }

        // Finds an element using the Object Model, converting the UI Automation tree to XML and finding by XPath.
        [UiaSegmentType(type: "ObjectModel")]
        private static UiaElementModel ByObjectModel(CUIAutomation8 session, IUIAutomationElement rootElement, string pathSegment)
        {
            // Remove any namespace prefixes from the XPath
            var xpath = Regex.Replace(input: pathSegment, pattern: @"(?<=^\/?)\w+:", replacement: string.Empty);
            xpath = "/" + xpath;

            // Create a new Document Object Model (DOM) from the root element
            var objectModel = new DocumentObjectModelFactory(rootElement).New();

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
            var condition = session.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, id);

            // Determine the tree scope based on the path segment
            var treeScope = pathSegment.StartsWith('/')
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;

            // Find and return the first element that matches the condition within the specified scope
            var element = rootElement.FindFirst(treeScope, condition);

            return new UiaElementModel
            {
                UIAutomationElement = element
            };
        }

        [UiaSegmentType(type: "Ocr")]
        private static void ByOcr()
        {

        }
#pragma warning restore IDE0051, S3011






























































        private (int Status, UiaElementModel Element) FindElement(UiaSessionModel uiaSession, UiaElementModel uiaElement, LocationStrategyModel locationStrategy)
        {
            var segments = locationStrategy
                .Value
                .Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            if (locationStrategy.Value.Contains("/DOM/", StringComparison.OrdinalIgnoreCase))
            {
                return FindElement(uiaSession, locationStrategy);
            }

            foreach (var segment in segments)
            {
                var (statusCode, element) = FindElementByProperties(session: uiaSession, uiaElement, locationStrategy: new()
                {
                    Using = "xpath",
                    Value = segment
                });

                if (statusCode == StatusCodes.Status200OK)
                {
                    return (statusCode, element);
                }
            }

            // get
            return (StatusCodes.Status404NotFound, default);
        }

        private static (int Status, UiaElementModel Element) FindElementByProperties(UiaSessionModel session, UiaElementModel uiaElement, LocationStrategyModel locationStrategy)
        {
            // not implemented
            if (locationStrategy.Using != LocationStrategyModel.Xpath)
            {
                return (StatusCodes.Status501NotImplemented, default);
            }

            // not found
            if (session == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // get by Cords
            var elementByCords = GetByCords(session, locationStrategy);
            if (elementByCords.Status == StatusCodes.Status200OK)
            {
                return elementByCords;
            }

            // get by path
            return FindByProperty(session, uiaElement, locationStrategy);
        }

        private static (int Status, UiaElementModel Element) FindByProperty(UiaSessionModel session, UiaElementModel uiaElement, LocationStrategyModel locationStrategy)
        {
            // setup
            var (isRoot, hierarchy) = GetLocatorHierarchy(locationStrategy);

            // bad request
            if (!hierarchy.Any())
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // find element
            var rootElement = isRoot ? new CUIAutomation8().GetRootElement() : session.ApplicationRoot;

            if (uiaElement?.UIAutomationElement != null && !isRoot)
            {
                rootElement = uiaElement.UIAutomationElement;
            }

            var automationElement = GetElementBySegment(new CUIAutomation8(), rootElement, hierarchy.First());

            // not found
            if (automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // iterate
            foreach (var pathSegment in hierarchy.Skip(1))
            {
                automationElement = GetElementBySegment(new CUIAutomation8(), automationElement, pathSegment);
                if (automationElement == default)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
            }

            // OK
            var element = automationElement.ConvertToElement();
            session.Elements[element.Id] = element;

            // get
            return (StatusCodes.Status200OK, element);
        }




































        private static (int Status, UiaElementModel Element) Find(UiaSessionModel uiaSession, IUIAutomationElement automationElement,  string xpath)
        {
            // setup
            var (isRoot, hierarchy) = GetLocatorHierarchy(new LocationStrategyModel { Value = "xpath" });

            // bad request
            if (!hierarchy.Any())
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            var segments = hierarchy
                .Where(i => !string.IsNullOrEmpty(i) && !i.Equals("dom", StringComparison.OrdinalIgnoreCase))
                .Select(i => $"/{i}")
                .ToArray();

            var uiElementSegment = segments[0];
            var elementSegment = string.Join(string.Empty, segments.Skip(1));

            return (StatusCodes.Status404NotFound, default);
        }






        // Linear Search
        private (int Status, UiaElementModel Element) FindElement(LocationStrategyModel locationStrategy, string session)
        {
            // not found
            if (!_sessions.ContainsKey(session))
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var uiaSessionModel = _sessions[session];
            var segments = locationStrategy.Value.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            // bad request
            if (segments == null || segments.Length == 0)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // dom
            if (locationStrategy.Value.Contains("/DOM/", StringComparison.OrdinalIgnoreCase))
            {
                return FindElement(uiaSessionModel, locationStrategy);
            }

            foreach (var segment in segments)
            {
                var (statusCode, element) = FindElement(session: uiaSessionModel, string.Empty, locationStrategy: new()
                {
                    Using = "xpath",
                    Value = segment
                });

                if (statusCode == StatusCodes.Status200OK)
                {
                    return (statusCode, element);
                }
            }

            // get
            return (StatusCodes.Status404NotFound, default);
        }

        // Binary Search
        private (int Status, UiaElementModel Element) FindElement(UiaSessionModel session, LocationStrategyModel locationStrategy)
        {
            // setup
            var (isRoot, hierarchy) = GetLocatorHierarchy(locationStrategy);

            // bad request
            if (!hierarchy.Any())
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // setup
            var segments = hierarchy
                .Where(i => !string.IsNullOrEmpty(i) && !i.Equals("dom", StringComparison.OrdinalIgnoreCase))
                .Select(i => $"/{i}")
                .ToArray();

            var uiElementSegment = segments[0];
            var elementSegment = string.Join(string.Empty, segments.Skip(1));

            // TODO: allow first element to found in the dom
            // find element
            var automation = new CUIAutomation8();
            var rootElement = isRoot ? automation.GetRootElement() : session.ApplicationRoot;
            var (statusCode, element) = FindElement(new LocationStrategyModel
            {
                Using = "xpath",
                Value = (isRoot ? $"/root{uiElementSegment}" : uiElementSegment)
            }, session.SessionId);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return (statusCode, element);
            }

            // only one segment
            if (segments.Length == 1)
            {
                return (StatusCodes.Status200OK, element.UIAutomationElement.ConvertToElement());
            }

            // find
            var (status, automationElement) = GetElementFromDom(element, elementSegment);

            // not found
            if (automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // add to session state
            session.Elements[automationElement.Id] = automationElement;

            // get
            return (status, automationElement);
        }

        private static (int Status, UiaElementModel Element) FindElement(UiaSessionModel session, string element, LocationStrategyModel locationStrategy)
        {
            // not implemented
            if (locationStrategy.Using != LocationStrategyModel.Xpath)
            {
                return (StatusCodes.Status501NotImplemented, default);
            }

            // not found
            if (session == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // get by Cords
            var elementByCords = GetByCords(session, locationStrategy);
            if (elementByCords.Status == StatusCodes.Status200OK)
            {
                return elementByCords;
            }

            // get by path
            return GetByProperty(session, locationStrategy);
        }


















        private static (int Status, UiaElementModel Element) GetElementFromDom(UiaElementModel rootElement, string xpath)
        {
            // find
            try
            {
                var automation = new CUIAutomation8();
                var dom = new DocumentObjectModelFactory(rootElement.UIAutomationElement).New();
                var idAttribute = dom.XPathSelectElement(xpath)?.Attribute("id")?.Value;
                var id = JsonSerializer.Deserialize<int[]>(idAttribute);
                var condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_RuntimeIdPropertyId, id);
                var treeScope = TreeScope.TreeScope_Descendants;

                rootElement.UIAutomationElement = rootElement.UIAutomationElement.FindFirst(treeScope, condition);

                var statusCode = rootElement.UIAutomationElement == null
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status200OK;
                rootElement = rootElement.UIAutomationElement == null
                    ? default
                    : rootElement.UIAutomationElement.ConvertToElement();

                if (rootElement == default)
                {
                    return (StatusCodes.Status404NotFound, default);
                }

                return (statusCode, rootElement);
            }
            catch (Exception e) when (e != null)
            {
                return (StatusCodes.Status404NotFound, default);
            }
        }

        private static (int Status, UiaElementModel Element) GetByProperty(UiaSessionModel session, LocationStrategyModel locationStrategy)
        {
            // setup
            var (isRoot, hierarchy) = GetLocatorHierarchy(locationStrategy);

            // bad request
            if (!hierarchy.Any())
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // find element
            var rootElement = isRoot ? new CUIAutomation8().GetRootElement() : session.ApplicationRoot;
            var automationElement = GetElementBySegment(new CUIAutomation8(), rootElement, hierarchy.First());

            // not found
            if (automationElement == default)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // iterate
            foreach (var pathSegment in hierarchy.Skip(1))
            {
                automationElement = GetElementBySegment(new CUIAutomation8(), automationElement, pathSegment);
                if (automationElement == default)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
            }

            // OK
            var element = automationElement.ConvertToElement();
            session.Elements[element.Id] = element;

            // get
            return (StatusCodes.Status200OK, element);
        }

        [SuppressMessage("GeneratedRegex", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Keep it simple.")]
        private static (bool FromDesktop, IEnumerable<string> Hierarchy) GetLocatorHierarchy(LocationStrategyModel locationStrategy)
        {
            // constants
            const RegexOptions RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            // setup conditions
            var values = Regex.Matches(locationStrategy.Value, @"(?<==').+?(?=')").Select(i => i.Value).ToArray();
            var fromDesktop = Regex.IsMatch(locationStrategy.Value, @"^(\(+)?\/(root|desktop)", RegexOptions);
            var xpath = fromDesktop
                ? Regex.Replace(locationStrategy.Value, @"^(\(+)?\/(root|desktop)", string.Empty, RegexOptions)
                : locationStrategy.Value;

            // normalize tokens
            var tokens = new Dictionary<string, string>();
            for (int i = 0; i < values.Length; i++)
            {
                tokens[$"value_token_{i}"] = values[i];
                xpath = xpath.Replace(values[i], $"value_token_{i}");
            }

            // setup
            var hierarchy = Regex
                .Split(xpath, @"\/(?=\w+|\*)(?![^\[]*\])")
                .Where(i => !string.IsNullOrEmpty(i))
                .ToArray();

            // normalize
            for (int i = 0; i < hierarchy.Length; i++)
            {
                var segment = hierarchy[i];
                if (!segment.Equals("/") && !segment.EndsWith("/"))
                {
                    continue;
                }
                hierarchy[i + 1] = $"/{hierarchy[i + 1]}";
            }
            hierarchy = hierarchy
                .Where(i => !string.IsNullOrEmpty(i) && !i.Equals("/"))
                .Select(i => i.TrimEnd('/'))
                .ToArray();

            // restore tokens
            for (int i = 0; i < hierarchy.Length; i++)
            {
                foreach (var token in tokens)
                {
                    hierarchy[i] = hierarchy[i].Replace(token.Key, token.Value);
                }
            }

            // get
            return (fromDesktop, hierarchy);
        }

        private static IUIAutomationElement GetElementBySegment(
            CUIAutomation8 session,
            IUIAutomationElement rootElement,
            string pathSegment)
        {
            // setup conditions
            var controlTypeCondition = GetControlTypeCondition(session, pathSegment);
            var propertyCondition = GetPropertyCondition(session, pathSegment);
            var isDescendants = pathSegment.StartsWith("/");

            // setup condition
            var scope = isDescendants ? TreeScope.TreeScope_Descendants : TreeScope.TreeScope_Children;
            IUIAutomationCondition condition;

            if (controlTypeCondition == default && propertyCondition != default)
            {
                condition = propertyCondition;
            }
            else if (controlTypeCondition != default && propertyCondition == default)
            {
                condition = controlTypeCondition;
            }
            else if (controlTypeCondition != default && propertyCondition != default)
            {
                condition = session.CreateAndCondition(controlTypeCondition, propertyCondition);
            }
            else
            {
                return default;
            }

            // setup find all
            var index = Regex.Match(input: pathSegment, pattern: @"(?<=\[)\d+(?=])").Value;
            var isIndex = int.TryParse(index, out int indexOut);

            // get single
            if (!isIndex)
            {
                return rootElement.FindFirst(scope, condition);
            }

            // get by index
            var elements = rootElement.FindAll(scope, condition);

            // get
            return elements.Length == 0
                ? default
                : elements.GetElement(indexOut - 1 < 0 ? 0 : indexOut - 1);
        }


        private static (int Status, UiaElementModel Element) GetByCords(UiaSessionModel session, LocationStrategyModel locationStrategy)
        {
            // find
            var element = locationStrategy.NewPointElement();

            // not found
            if (element == null)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var id = $"{Guid.NewGuid()}";

            // update
            session.Elements[id] = element;

            // get
            return (StatusCodes.Status200OK, element);
        }





        private static IUIAutomationCondition GetControlTypeCondition(CUIAutomation8 session, string pathSegment)
        {
            const PropertyConditionFlags ConditionFlags = PropertyConditionFlags.PropertyConditionFlags_None;
            const BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            static int GetControlTypeId(string propertyName)
            {
                var fields = typeof(UIA_ControlTypeIds).GetFields(BindingFlags);
                var id = fields
                    .FirstOrDefault(i => i.Name.Equals($"UIA_{propertyName}ControlTypeId", Compare))?
                    .GetValue(null);
                return id == default ? -1 : (int)id;
            }

            pathSegment = pathSegment.LastIndexOf('[') == -1 ? $"{pathSegment}[]" : pathSegment;
            var typeSegment = Regex.Match(input: pathSegment, pattern: @"(?<=((\/)+)?)\w+(?=\)?\[)").Value;

            var conditionFlag = typeSegment.StartsWith("partial", Compare)
                ? PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
                : ConditionFlags;
            typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
            var controlTypeId = GetControlTypeId(typeSegment);

            if (string.IsNullOrEmpty(typeSegment))
            {
                return default;
            }

            return session
                .CreatePropertyConditionEx(UIA_PropertyIds.UIA_ControlTypePropertyId, controlTypeId, conditionFlag);
        }

        private static IUIAutomationCondition GetPropertyCondition(CUIAutomation8 session, string pathSegment)
        {
            // constants
            const PropertyConditionFlags ConditionFlags = PropertyConditionFlags.PropertyConditionFlags_None;
            const BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // local
            static int GetPropertyId(string propertyName)
            {
                var fields = typeof(UIA_PropertyIds).GetFields(BindingFlags);
                var id = fields
                    .FirstOrDefault(i => i.Name.Equals($"UIA_{propertyName}PropertyId", Compare))?
                    .GetValue(null);
                return id == default ? -1 : (int)id;
            }

            // setup
            // TODO: replace with fully functional logical parser.
            var conditions = new List<IUIAutomationCondition>();
            var segments = Regex.Match(pathSegment, @"(?<=\[).*(?=\])").Value.Split(" and ").Select(i => $"[{i}]");

            // build
            foreach (var segment in segments)
            {
                var typeSegment = Regex.Match(input: segment, pattern: @"(?<=@)\w+").Value;

                // setup
                var conditionFlag = typeSegment.StartsWith("partial", Compare)
                    ? PropertyConditionFlags.PropertyConditionFlags_MatchSubstring
                    : ConditionFlags;
                typeSegment = typeSegment.Replace("partial", string.Empty, Compare);
                var valueSegment = Regex.Match(input: segment, pattern: @"(?<=\[@\w+=('|"")).*(?=('|"")])").Value;
                var propertyId = GetPropertyId(typeSegment);

                // not found
                if (propertyId == -1)
                {
                    continue;
                }

                // get
                var condition = session.CreatePropertyConditionEx(propertyId, valueSegment, conditionFlag);

                // set
                conditions.Add(condition);
            }

            // not found
            if (conditions.Count == 0)
            {
                return default;
            }

            // no logical operators
            return conditions.Count == 1
                ? conditions.First()
                : session.CreateAndConditionFromArray(conditions.ToArray());
        }









        private static UiaElementModel GetElement(IDictionary<string, UiaSessionModel> sessions, string session, string element)
        {
            // notFound
            if (!sessions.ContainsKey(session))
            {
                return default;
            }
            if (sessions[session].Elements?.ContainsKey(element) != true)
            {
                return default;
            }

            // get
            return sessions[session].Elements[element];
        }




        public UiaElementModel GetElement(string session, string element)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Value) GetElementAttribute(string session, string element, string name)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Text) GetElementText(string session, string element)
        {
            throw new NotImplementedException();
        }
    }
}
