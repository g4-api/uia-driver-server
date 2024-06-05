/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Xml.Linq;

using UIAutomationClient;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents a UI Automation element with associated properties.
    /// </summary>
    public class UiaElementModel
    {
        /// <summary>
        /// Gets or sets the clickable point of the element.
        /// </summary>
        public PointModel ClickablePoint { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the element.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the XML representation of the element.
        /// </summary>
        public XNode Node { get; set; }

        /// <summary>
        /// Gets or sets the rectangle (bounding box) of the element on the screen.
        /// </summary>
        public RectangleModel Rectangle { get; set; }

        /// <summary>
        /// Gets or sets the UI Automation element associated with this element.
        /// </summary>
        public IUIAutomationElement UIAutomationElement { get; set; }
    }
}
