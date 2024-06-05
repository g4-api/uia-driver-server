/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Linq;

using UIAutomationClient;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents a UI Automation session model.
    /// </summary>
    public class UiaSessionModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiaSessionModel"/> class.
        /// </summary>
        public UiaSessionModel()
            : this(new CUIAutomation8())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UiaSessionModel"/> class with the specified automation.
        /// </summary>
        /// <param name="automation">The UI Automation object.</param>
        public UiaSessionModel(CUIAutomation8 automation)
            : this(automation, default)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UiaSessionModel"/> class with the specified automation and application.
        /// </summary>
        /// <param name="automation">The UI Automation object.</param>
        /// <param name="application">The process of the application.</param>
        public UiaSessionModel(CUIAutomation8 automation, Process application)
            : this(automation, application, TreeScope.TreeScope_Children)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UiaSessionModel"/> class with the specified automation, application, and tree scope.
        /// </summary>
        /// <param name="automation">The UI Automation object.</param>
        /// <param name="application">The process of the application.</param>
        /// <param name="treeScope">The tree scope for UI Automation.</param>
        public UiaSessionModel(CUIAutomation8 automation, Process application, TreeScope treeScope)
        {
            // Assign the UI Automation object to the Automation property.
            Automation = automation;

            // Assign the application process to the Application property.
            Application = application;

            // Initialize the Elements dictionary to store UI elements.
            Elements = new ConcurrentDictionary<string, UiaElementModel>();

            // Get the application process ID or use -1 if the application is null.
            var id = application?.Id == null ? -1 : application.Id;

            // Create a condition to find the application's root element based on the process ID.
            var condition = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ProcessIdPropertyId, id);

            // Find the application's root element using the specified tree scope and condition.
            var applicationRoot = automation.GetRootElement().FindFirst(treeScope, condition);

            // Set the ApplicationRoot property to the found root element or the overall root element if not found.
            ApplicationRoot = applicationRoot ?? automation.GetRootElement();

            // Assign the session ID based on the application process ID.
            SessionId = $"{id}";
        }

        /// <summary>
        /// Gets or sets the process of the application.
        /// </summary>
        [JsonIgnore]
        public Process Application { get; set; }

        /// <summary>
        /// Gets or sets the root element of the application.
        /// </summary>
        [JsonIgnore]
        public IUIAutomationElement ApplicationRoot { get; set; }

        /// <summary>
        /// Gets the UI Automation object.
        /// </summary>
        [JsonIgnore]
        public CUIAutomation8 Automation { get; }

        /// <summary>
        /// Gets or sets the capabilities of the session.
        /// </summary>
        public IDictionary<string, object> Capabilities { get; set; }

        /// <summary>
        /// Gets or sets the document object model of the application.
        /// </summary>
        [JsonIgnore]
        public XDocument DocumentObjectModel { get; set; }

        /// <summary>
        /// Gets or sets the elements in the session.
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, UiaElementModel> Elements { get; set; }

        /// <summary>
        /// Gets or sets the runtime IDs of the session.
        /// </summary>
        public IEnumerable<int> Runtime { get; set; }

        /// <summary>
        /// Gets or sets the scale ratio of the session.
        /// </summary>
        public double ScaleRatio { get; set; }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the timeout for the session.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the tree scope for UI Automation.
        /// </summary>
        [JsonIgnore]
        public TreeScope TreeScope { get; set; } = TreeScope.TreeScope_Descendants;
    }
}
