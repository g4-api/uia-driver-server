/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://www.w3.org/TR/webdriver1/#locator-strategies
 */
using System.Runtime.Serialization;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents a model for location strategies used in UI Automation.
    /// </summary>
    [DataContract]
    public class LocationStrategyModel
    {
        /// <summary>
        /// Locator strategy for CSS selector.
        /// </summary>
        public const string CssSelector = "css selector";

        /// <summary>
        /// Locator strategy for link text.
        /// </summary>
        public const string LinkText = "link text";

        /// <summary>
        /// Locator strategy for optical character recognition (OCR).
        /// </summary>
        public const string Ocr = "ocr";

        /// <summary>
        /// Locator strategy for partial link text.
        /// </summary>
        public const string PartialLinkText = "partial link text";

        /// <summary>
        /// Locator strategy for tag name.
        /// </summary>
        public const string TagName = "tag name";

        /// <summary>
        /// Locator strategy for XPath.
        /// </summary>
        public const string Xpath = "xpath";

        /// <summary>
        /// Gets or sets the strategy used for locating elements.
        /// </summary>
        [DataMember]
        public string Using { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the location strategy.
        /// </summary>
        [DataMember]
        public string Value { get; set; }
    }
}
