/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;

namespace Uia.DriverServer.Attributes
{
    /// <summary>
    /// Specifies the locator segment type for finding elements in different ways.
    /// </summary>
    /// <param name="type">The type of the locator segment.</param>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UiaSegmentTypeAttribute(string type) : Attribute
    {
        public string Justification { get; set; }
        /// <summary>
        /// Gets or sets the type of the locator segment, such as 'Uia', 'ObjectModel', or 'Ocr'.
        /// </summary>
        public string Type { get; set; } = type;
    }
}
