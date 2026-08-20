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
    /// Resolves element locators against UI Automation sessions and maintains each session's element cache.
    /// </summary>
    /// <remarks>
    /// Locator resolution is compute-only until a successful result is assigned an identifier and stored in the
    /// caller-owned session model. Missing sessions, invalid hierarchies, and unresolved elements retain the
    /// repository's existing status-code and empty-result contracts.
    /// </remarks>
    /// <param name="sessions">The caller-owned session registry used for lookup and element-cache mutation.</param>
    public class ElementsRepository(IDictionary<string, UiaSessionResponseModel> sessions) : IElementsRepository
    {
        #region *** Constants    ***

        private static readonly Regex CoordinateExpression = new(
            pattern: @"(?is)(?<=Coords\().*?(?=\))",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)
        );

        private static readonly Regex DesktopPrefixExpression = new(
            pattern: @"(?is)^(\(+)?\/(root|desktop)",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)

        );

        private static readonly Regex LocatorSeparatorExpression = new(
            pattern: @"\/(?=\w+|\*)(?![^\[]*\])",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)
        );

        private static readonly Regex ObjectModelNamespaceExpression = new(
            pattern: @"(?<=^\/?)\w+:",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)
        );

#if Release_Emgu || Debug_Emgu
        private static readonly Regex OcrExpression = new(
            pattern: @"(?is)(?<=Ocr\().*?(?=\))",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)
        );
#endif

        private static readonly Regex QuotedValueExpression = new(
            pattern: "(?<==').+?(?=')",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)
        );

        private static readonly Regex SegmentKeyExpression = new(
            pattern: @"(?<=^\/?)\w+",
            options: RegexOptions.CultureInvariant,
            matchTimeout: TimeSpan.FromSeconds(30)
        );

        #endregion

        #region *** Fields       ***

        // Retains the caller-owned session registry so successful lookups can update the matching element cache.
        private readonly IDictionary<string, UiaSessionResponseModel> _sessions = sessions;

        #endregion

        #region *** Methods      ***

        /// <inheritdoc />
        public (int Status, UiaElementModel ElementModel) FindElement(
            string session,
            LocationStrategyModel locationStrategy)
        {
            // Route application-root lookups through the complete overload with no cached element context.
            return FindElement(session, element: string.Empty, locationStrategy);
        }

        /// <inheritdoc />
        public (int Status, UiaElementModel ElementModel) FindElement(
            string session,
            string element,
            LocationStrategyModel locationStrategy)
        {
            // Resolve the requested session before allocating UI Automation state or mutating its element cache.
            var isSession = _sessions.TryGetValue(key: session, value: out var uiaSession);
            if (!isSession)
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // Preserve the application root unless the caller supplied a valid cached element context.
            var uiaElement = !string.IsNullOrEmpty(value: element)
                ? GetElementBySession(sessions: _sessions, session, element)
                : default;

            // Parse the complete locator before traversal so malformed or empty hierarchies fail as a bad request.
            var (isFromDesktop, hierarchy) = FormatLocatorHierarchy(locationStrategy);
            if (hierarchy.Length == 0)
            {
                return (StatusCodes.Status400BadRequest, default);
            }

            // Select the caller-intended root once so every segment is resolved from the preceding result.
            var rootElement = GetSearchRoot(uiaSession, uiaElement, isFromDesktop);
            var outputElement = new UiaElementModel
            {
                UIAutomationElement = rootElement
            };

            // Traverse each segment in order so repeated selectors apply their position under the resolved parent.
            foreach (var pathSegment in hierarchy)
            {
                outputElement = FindElementBySegment(
                    session: new CUIAutomation8(),
                    rootElement: outputElement.UIAutomationElement,
                    pathSegment
                );

                // Stop traversal when no UIA element, rectangle, or coordinate point represents the segment result.
                var isElementMissing = outputElement?.UIAutomationElement == default;
                var isRectangleMissing = outputElement?.Rectangle == default;
                var isClickablePointMissing = outputElement?.ClickablePoint == default;
                var isResultMissing = isElementMissing && isRectangleMissing && isClickablePointMissing;

                if (isResultMissing)
                {
                    return (StatusCodes.Status404NotFound, default);
                }
            }

            // Assign a stable session key before exposing the resolved element through subsequent driver commands.
            outputElement.Id ??= Guid.NewGuid().ToString();

            // Publish the successful result to the session-owned cache so later element-relative commands can reuse it.
            uiaSession.Elements[outputElement.Id] = outputElement;

            return (StatusCodes.Status200OK, outputElement);
        }

        /// <inheritdoc />
        public IEnumerable<UiaElementModel> FindElements(
            string session,
            LocationStrategyModel locationStrategy)
        {
            // Route application-root lookups through the complete overload with no cached element context.
            return FindElements(session, element: string.Empty, locationStrategy);
        }

        /// <inheritdoc />
        public IEnumerable<UiaElementModel> FindElements(
            string session,
            string element,
            LocationStrategyModel locationStrategy)
        {
            // Resolve the requested session before allocating UI Automation state or mutating its element cache.
            var isSession = _sessions.TryGetValue(key: session, value: out var uiaSession);
            if (!isSession)
            {
                return [];
            }

            // Preserve the application root unless the caller supplied a valid cached element context.
            var uiaElement = !string.IsNullOrEmpty(value: element)
                ? GetElementBySession(sessions: _sessions, session, element)
                : default;

            // Parse the complete locator before traversal so malformed or empty hierarchies return no matches.
            var (isFromDesktop, hierarchy) = FormatLocatorHierarchy(locationStrategy);
            if (hierarchy.Length == 0)
            {
                return [];
            }

            // Resolve every ancestor segment before evaluating all matches for the final selector.
            var outputElement = GetSearchRoot(uiaSession, uiaElement, isFromDesktop);
            foreach (var pathSegment in hierarchy.Take(count: hierarchy.Length - 1))
            {
                outputElement = FindElementBySegment(
                    session: new CUIAutomation8(),
                    rootElement: outputElement,
                    pathSegment
                )?.UIAutomationElement;

                if (outputElement == default)
                {
                    return [];
                }
            }

            // Convert the terminal selector into the exact condition and scope used to determine positional rank.
            var lastPathSegment = hierarchy[^1];
            var scope = lastPathSegment.StartsWith(value: '/')
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;
            var condition = XpathParser.ConvertToCondition(xpath: lastPathSegment);

            if (condition == null)
            {
                return [];
            }

            // Materialize all exact-condition matches before applying the optional 1-based terminal position.
            var elements = outputElement.FindAll(scope, condition);
            if (elements == null || elements.Length == 0)
            {
                return [];
            }

            // Reject an invalid position without coercing zero or an out-of-range value to the first match.
            var position = XpathPosition.GetSelection(
                pathSegment: lastPathSegment,
                matchCount: elements.Length
            );

            if (position.HasPosition && position.Index < 0)
            {
                return [];
            }

            // Bound enumeration to the selected match when a position is present, or retain every match otherwise.
            var outputElements = new List<UiaElementModel>();
            var startIndex = position.HasPosition ? position.Index : 0;
            var endIndex = position.HasPosition ? position.Index + 1 : elements.Length;

            for (var index = startIndex; index < endIndex; index++)
            {
                // Convert and identify each returned element before publishing it to session-owned state.
                var elementModel = elements.GetElement(index).ConvertToElement();
                elementModel.Id ??= Guid.NewGuid().ToString();

                // Keep the session cache and returned collection synchronized for every observable result.
                uiaSession.Elements[elementModel.Id] = elementModel;
                outputElements.Add(item: elementModel);
            }

            return outputElements;
        }

        /// <inheritdoc />
        public UiaElementModel GetElement(string session, string element)
        {
            // Read the existing session cache without mutating ownership or allocating a replacement model.
            return GetElementBySession(sessions: _sessions, session, element);
        }

        /// <inheritdoc />
        public (int StatusCode, string Value) GetElementAttribute(string session, string element, string name)
        {
            // Resolve the cached element before reading provider-backed attributes.
            var elementModel = GetElementBySession(sessions: _sessions, session, element);
            if (elementModel == null)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // Normalize an absent attribute to the driver's successful empty-string response contract.
            var attribute = elementModel.GetAttribute(name);

            return string.IsNullOrEmpty(value: attribute)
                ? (StatusCodes.Status200OK, string.Empty)
                : (StatusCodes.Status200OK, attribute);
        }

        /// <inheritdoc />
        public (int StatusCode, string Text) GetElementText(string session, string element)
        {
            // Resolve the cached element before reading provider-backed text.
            var elementModel = GetElementBySession(sessions: _sessions, session, element);
            if (elementModel == null)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // Normalize absent text to the driver's successful empty-string response contract.
            var value = elementModel.GetText();

            return string.IsNullOrEmpty(value)
                ? (StatusCodes.Status200OK, string.Empty)
                : (StatusCodes.Status200OK, value);
        }

#pragma warning disable IDE0051 // Segment handlers are discovered through UiaSegmentTypeAttribute reflection.
        // Resolves a coordinate segment into a point-backed element without reading or mutating session state.
        // Invalid coordinate payloads retain the existing parsing exception behavior for the caller to surface.
        [UiaSegmentType(type: "Coords")]
        private static UiaElementModel FindElementByCoordinates(SegmentDataModel segmentData)
        {
            // Parse the coordinate payload into the two integer values consumed by point-based driver commands.
            var segment = CoordinateExpression.Match(input: segmentData.PathSegment).Value;
            var coordinates = segment.Split(separator: ',').Select(selector: int.Parse).ToArray();
            var point = new PointModel
            {
                X = coordinates[0],
                Y = coordinates[1]
            };

            // Assign an identifier before creating the XML representation returned through the element model.
            var identifier = Guid.NewGuid().ToString();
            var xml = "<PointElement " +
                $"X=\"{point.X}\" " +
                $"Y=\"{point.Y}\" " +
                $"Id=\"{identifier}\" />";

            // Return compute-only coordinate state; the public repository operation owns any later cache mutation.
            return new UiaElementModel
            {
                ClickablePoint = point,
                Id = identifier,
                Node = XDocument.Parse(text: xml).Root
            };
        }

        // Resolves an object-model XPath through a temporary XML projection, then maps its runtime ID back to UIA.
        // The projection remains local to this call and a missing runtime ID produces the existing null result.
        [UiaSegmentType(type: "ObjectModel")]
        private static UiaElementModel FindElementByObjectModel(SegmentDataModel segmentData)
        {
            // Remove the strategy namespace and anchor the XPath below the supplied root projection.
            var xpath = ObjectModelNamespaceExpression.Replace(
                input: segmentData.PathSegment,
                replacement: string.Empty
            );
            xpath = "/*/" + xpath;

            // Project the current UIA subtree only for the duration of this object-model lookup.
            var objectModel = DocumentObjectModelFactory.New(
                automation: segmentData.Session,
                element: segmentData.RootElement,
                addDesktop: false
            );

            // Extract the selected node's runtime ID so the result can be mapped back to a live UIA element.
            var idAttribute = objectModel.XPathSelectElement(expression: xpath)?.Attribute(name: "id")?.Value;
            if (idAttribute == null)
            {
                return default;
            }

            // Recreate the provider condition and scope that identify the selected live element.
            var identifier = JsonSerializer.Deserialize<int[]>(json: idAttribute);
            var condition = segmentData.Session.CreatePropertyCondition(
                propertyId: UIA_PropertyIds.UIA_RuntimeIdPropertyId,
                value: identifier
            );
            var treeScope = segmentData.PathSegment.StartsWith(value: '/')
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;

            // Return the live element without mutating the session cache owned by the public operation.
            var element = segmentData.RootElement.FindFirst(scope: treeScope, condition);

            return new UiaElementModel
            {
                UIAutomationElement = element
            };
        }

#if Release_Emgu || Debug_Emgu
        // Resolves an OCR segment through the optional image-recognition repository without mutating session state.
        // OCR parsing and recognition failures retain the optional provider's existing exception behavior.
        [UiaSegmentType(type: "Ocr")]
        private static UiaElementModel FindElementByOcr(SegmentDataModel segmentData)
        {
            // Extract the OCR payload before delegating recognition to the optional provider.
            var segment = OcrExpression.Match(input: segmentData.PathSegment).Value;
            var ocrRepository = new OcrRepository();

            return ocrRepository.FindElement(segment);
        }
#endif

        // Dispatches one locator segment to its attributed strategy while keeping handler discovery centralized.
        // The method remains instance-bound so derived repositories retain their attributed strategy set.
        private UiaElementModel FindElementBySegment(
            CUIAutomation8 session,
            IUIAutomationElement rootElement,
            string pathSegment)
        {
            // Discover strategies from the runtime repository type to preserve the existing reflection contract.
            var segmentMethods = GetType()
                .GetMethods(bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)
                .Where(method => method.GetCustomAttribute<UiaSegmentTypeAttribute>() != null)
                .ToDictionary(
                    keySelector: method => method.GetCustomAttribute<UiaSegmentTypeAttribute>().Type,
                    elementSelector: method => method,
                    comparer: StringComparer.OrdinalIgnoreCase
                );

            // Select the attributed strategy key, falling back to standard UIA for unqualified XPath segments.
            var segmentKey = SegmentKeyExpression.Match(input: pathSegment).Value;
            var isKnownSegment = segmentMethods.TryGetValue(key: segmentKey, value: out var method);
            var segmentMethod = isKnownSegment ? method : segmentMethods["Uia"];

            // Package the explicit traversal context so static handlers do not depend on repository instance state.
            var segmentData = new SegmentDataModel
            {
                PathSegment = pathSegment,
                RootElement = rootElement,
                Session = session
            };

            // Invoke the selected strategy and return its compute-only element result to the owning traversal.
            var result = segmentMethod.Invoke(obj: null, parameters: [segmentData]);

            return (UiaElementModel)result;
        }

        // Resolves a standard UIA segment and applies any terminal position to the exact condition result.
        // The helper remains compute-only; missing conditions and invalid positions return the established null result.
        [UiaSegmentType(type: "Uia")]
        private static UiaElementModel FindElementByUia(SegmentDataModel segmentData)
        {
            // Select child or descendant scope from the segment separator before creating the provider condition.
            var isDescendantScope = segmentData.PathSegment.StartsWith(value: '/');
            var treeScope = isDescendantScope
                ? TreeScope.TreeScope_Descendants
                : TreeScope.TreeScope_Children;
            var condition = XpathParser.ConvertToCondition(xpath: segmentData.PathSegment);

            if (condition == null)
            {
                return default;
            }

            // Parse the optional position first so zero and numeric overflow fail before a provider-wide search.
            var position = XpathPosition.GetSelection(
                pathSegment: segmentData.PathSegment,
                matchCount: int.MaxValue
            );

            if (!position.HasPosition)
            {
                return segmentData.RootElement.FindFirst(treeScope, condition)?.ConvertToElement();
            }

            if (position.Index < 0)
            {
                return default;
            }

            // Materialize the exact-condition result before validating the requested 1-based collection rank.
            var elements = segmentData.RootElement.FindAll(treeScope, condition);
            position = XpathPosition.GetSelection(
                pathSegment: segmentData.PathSegment,
                matchCount: elements?.Length ?? 0
            );

            // Return only a validated match so out-of-range positions never access the COM collection.
            return position.Index < 0
                ? default
                : elements.GetElement(position.Index).ConvertToElement();
        }

#pragma warning restore IDE0051

        // Parses a locator into ordered segments while preserving slashes inside quoted predicate values.
        // This compute-only transformation reports whether traversal starts from Desktop and never mutates its input.
        private static (bool FromDesktop, string[] Hierarchy) FormatLocatorHierarchy(
            LocationStrategyModel locationStrategy)
        {
            // Protect quoted values with tokens so slash characters inside predicates do not split the hierarchy.
            var values = QuotedValueExpression
                .Matches(input: locationStrategy.Value)
                .Select(match => match.Value)
                .ToArray();
            var isFromDesktop = DesktopPrefixExpression.IsMatch(input: locationStrategy.Value);
            var xpath = isFromDesktop
                ? DesktopPrefixExpression.Replace(input: locationStrategy.Value, replacement: string.Empty)
                : locationStrategy.Value;
            var tokens = new Dictionary<string, string>();

            // Replace each quoted value with a deterministic token before evaluating hierarchy separators.
            for (var index = 0; index < values.Length; index++)
            {
                var token = $"value_token_{index}";
                tokens[token] = values[index];
                xpath = xpath.Replace(oldValue: values[index], newValue: token);
            }

            // Split direct-child steps and retain descendant markers for the segment that follows each double slash.
            var hierarchy = LocatorSeparatorExpression
                .Split(input: xpath)
                .Where(segment => !string.IsNullOrEmpty(value: segment))
                .ToArray();

            for (var index = 0; index < hierarchy.Length; index++)
            {
                var segment = hierarchy[index];
                var marksDescendantScope = segment.Equals("/") || segment.EndsWith(value: '/');
                var hasFollowingSegment = index + 1 < hierarchy.Length;

                if (!marksDescendantScope || !hasFollowingSegment)
                {
                    continue;
                }

                hierarchy[index + 1] = $"/{hierarchy[index + 1]}";
            }

            // Remove consumed separator artifacts before restoring the original predicate values.
            hierarchy =
            [
                .. hierarchy
                    .Where(segment => !string.IsNullOrEmpty(value: segment) && !segment.Equals("/"))
                    .Select(segment => segment.TrimEnd(trimChar: '/'))
            ];

            // Restore caller values after structural parsing so every emitted segment retains its original predicate.
            for (var index = 0; index < hierarchy.Length; index++)
            {
                foreach (var (token, value) in tokens)
                {
                    hierarchy[index] = hierarchy[index].Replace(oldValue: token, newValue: value);
                }
            }

            return (isFromDesktop, hierarchy);
        }

        // Reads one cached element from a caller-owned session registry without allocating or mutating state.
        // Missing sessions, absent element dictionaries, and unknown identifiers all produce the established
        // null result.
        private static UiaElementModel GetElementBySession(
            IDictionary<string, UiaSessionResponseModel> sessions,
            string session,
            string element)
        {
            // Resolve the owning session before attempting to read its element cache.
            var isSession = sessions.TryGetValue(key: session, value: out var sessionModel);
            if (!isSession)
            {
                return default;
            }

            // Preserve null and missing cache entries as the same unresolved-element outcome.
            var hasElements = sessionModel.Elements != null;
            if (!hasElements)
            {
                return default;
            }

            var isElement = sessionModel.Elements.TryGetValue(key: element, value: out var elementModel);

            return isElement ? elementModel : default;
        }

        // Selects Desktop, a cached element, or the application root according to the parsed locator origin.
        // The helper creates a UIA client only for Desktop lookup and does not mutate either supplied model.
        private static IUIAutomationElement GetSearchRoot(
            UiaSessionResponseModel uiaSession,
            UiaElementModel uiaElement,
            bool isFromDesktop)
        {
            // Honor an absolute locator before considering any cached element-relative context.
            if (isFromDesktop)
            {
                return new CUIAutomation8().GetRootElement();
            }

            // Prefer a valid cached element for relative lookup, otherwise retain the session application root.
            var hasCachedRoot = uiaElement?.UIAutomationElement != null;

            return hasCachedRoot
                ? uiaElement.UIAutomationElement
                : uiaSession.ApplicationRoot;
        }

        #endregion

        #region *** Nested Types ***

        // Carries the complete compute-only context supplied to one reflected locator-segment strategy.
        private sealed class SegmentDataModel
        {
            // Gets the exact path segment consumed by the selected strategy.
            public string PathSegment { get; init; }

            // Gets the UIA element that bounds child or descendant lookup for this segment.
            public IUIAutomationElement RootElement { get; init; }

            // Gets the UI Automation client used to create conditions and object-model projections.
            public CUIAutomation8 Session { get; init; }
        }

        #endregion
    }
}
