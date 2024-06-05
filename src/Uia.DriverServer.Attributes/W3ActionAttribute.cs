/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;

namespace Uia.DriverServer.Attributes
{
    /// <summary>
    /// An attribute to specify the type of a W3C action associated with a method.
    /// This attribute can be used to decorate methods to provide metadata about the
    /// type of W3C action they are intended to handle, which is useful for organized
    /// and type-safe management of actions in UI Automation.
    /// </summary>
    /// <param name="type">The type of the W3C action.</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class W3ActionAttribute(string type) : Attribute
    {
        /// <summary>
        /// Gets or sets the type of the W3C action.
        /// </summary>
        public string Type { get; set; } = type;
    }
}
