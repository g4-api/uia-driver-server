using System;

namespace Uia.DriverServer.Attributes
{
    /// <summary>
    /// Specifies a keyboard layout for a target component or operation.
    /// </summary>
    /// <param name="layout">The keyboard layout to use, for example, "US" or "HE".</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class KeyboardLayoutAttribute(string layout) : Attribute
    {
        /// <summary>
        /// Gets the keyboard layout associated with this attribute.
        /// </summary>
        public string Layout { get; } = layout;
    }
}
