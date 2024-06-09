/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

using UIAutomationClient;

namespace Uia.DriverServer.Extensions
{
    /// <summary>
    /// Factory class for creating a Document Object Model (DOM) representation of UI Automation elements.
    /// </summary>
    /// <param name="rootElement">The root UI Automation element.</param>
    public class DocumentObjectModelFactory(IUIAutomationElement rootElement)
    {
        // The root UI Automation element used as the starting point for creating the DOM.
        private readonly IUIAutomationElement _rootElement = rootElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentObjectModelFactory"/> class.
        /// </summary>
        public DocumentObjectModelFactory()
            : this(new CUIAutomation8().GetRootElement())
        { }

        /// <summary>
        /// Creates a new XML document representing the UI Automation element tree.
        /// </summary>
        /// <returns>A new <see cref="XDocument"/> representing the UI Automation element tree.</returns>
        public XDocument New()
        {
            // Create a new instance of the UI Automation object.
            var automation = new CUIAutomation8();

            // Use the root element if available; otherwise, get the desktop root element.
            var element = _rootElement ?? automation.GetRootElement();

            // Create the XML document from the UI Automation element tree.
            return New(automation, element);
        }

        /// <summary>
        /// Creates a new XML document representing the UI Automation element tree for the specified element.
        /// </summary>
        /// <param name="element">The UI Automation element.</param>
        /// <returns>A new <see cref="XDocument"/> representing the UI Automation element tree.</returns>
        public static XDocument New(IUIAutomationElement element)
        {
            // Create a new instance of the UI Automation object.
            var automation = new CUIAutomation8();

            // Create the XML document from the UI Automation element tree.
            return New(automation, element);
        }

        /// <summary>
        /// Creates a new XML document representing the UI Automation element tree for the specified automation object and element.
        /// </summary>
        /// <param name="automation">The UI Automation object.</param>
        /// <param name="element">The UI Automation element.</param>
        /// <returns>A new <see cref="XDocument"/> representing the UI Automation element tree.</returns>
        public static XDocument New(CUIAutomation8 automation, IUIAutomationElement element)
        {
            //var condition = automation.CreateTrueCondition();
            //var treeWalker = automation.CreateTreeWalker(condition);
            //var parentElement = treeWalker.GetParentElement(element) ?? element;

            var parentTagName = element.GetTagName();
            var parentAttributes = GetElementAttributes(element);

            // Register and generate XML data for the new DOM.
            var xmlData = Register(automation, element);

            // Construct the XML body with the tag name, attributes, and registered XML data.
            var xmlBody = $"<{parentTagName} {parentAttributes}>" + string.Join("\n", xmlData) + $"</{parentTagName}>";

            // Combine the XML data into a single XML string.
            var xml = "<Desktop>" + xmlBody + "</Desktop>";

            try
            {
                // Parse and return the XML document.
                return XDocument.Parse(xml);
            }
            catch (Exception e)
            {
                // Handle any parsing exceptions and return an error XML document.
                return XDocument.Parse($"<Desktop><Error>{e.GetBaseException().Message}</Error></Desktop>");
            }
        }

        /// <summary>
        /// Registers and generates XML data for the new DOM.
        /// </summary>
        /// <param name="automation">The UI Automation object.</param>
        /// <param name="element">The UI Automation element.</param>
        /// <returns>A list of strings representing the XML data.</returns>
        private static List<string> Register(CUIAutomation8 automation, IUIAutomationElement element)
        {
            // Initialize a list to store XML data.
            var xml = new List<string>();

            // Get the tag name and attributes of the element.
            var tagName = element.GetTagName();
            var attributes = GetElementAttributes(element);

            // Add the opening tag with attributes to the XML list.
            xml.Add($"<{tagName} {attributes}>");

            // Create a condition to find all child elements.
            var condition = automation.CreateTrueCondition();
            var treeWalker = automation.CreateTreeWalker(condition);
            var childElement = treeWalker.GetFirstChildElement(element);

            // Recursively process child elements.
            while (childElement != null)
            {
                var nodeXml = Register(automation, childElement);
                xml.AddRange(nodeXml);
                childElement = treeWalker.GetNextSiblingElement(childElement);
            }

            // Add the closing tag to the XML list.
            xml.Add($"</{tagName}>");

            // Return the complete XML data list.
            return xml;
        }

        // Gets the attributes of the specified UI Automation element as a string.
        private static string GetElementAttributes(IUIAutomationElement element)
        {
            // Get the attributes of the element.
            var attributes = element.GetAttributes();

            // Get the runtime ID of the element and serialize it to a JSON string.
            var runtime = element.GetRuntimeId().OfType<int>();
            var id = JsonSerializer.Serialize(runtime);
            attributes.Add("id", id);

            // Initialize a list to store attribute strings.
            var xmlNode = new List<string>();
            foreach (var item in attributes)
            {
                // Skip attributes with empty or whitespace-only keys or values.
                if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }

                // Add the attribute to the XML node list.
                xmlNode.Add($"{item.Key}=\"{item.Value}\"");
            }

            // Join the XML node representations into a single string and return it.
            return string.Join(" ", xmlNode);
        }
    }
}
