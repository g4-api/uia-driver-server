/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;

namespace Uia.DriverServer.Attributes
{
    /// <summary>
    /// An attribute to define a UI Automation (UIA) constant value and its associated name.
    /// This attribute can be applied to any target to provide metadata about UIA constants.
    /// </summary>
    /// <param name="constant">The UIA constant value.</param>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class UiaConstantAttribute(int constant) : Attribute
    {
        /// <summary>
        /// Gets the UIA constant value.
        /// </summary>
        public int Constant { get; } = constant;

        /// <summary>
        /// Gets or sets the name associated with the UIA constant.
        /// </summary>
        public string Name { get; set; }
    }
}
