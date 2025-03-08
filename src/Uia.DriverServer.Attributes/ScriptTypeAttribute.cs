using System;

namespace Uia.DriverServer.Attributes
{
    /// <summary>
    /// An attribute used to specify the type of script associated with a method.
    /// This attribute can be applied to methods to provide metadata about the
    /// type of script they are intended to handle, enabling more organized and
    /// type-safe script management.
    /// </summary>
    /// <param name="type">The type of the script.</param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScriptTypeAttribute(string type) : Attribute
    {

        /// <summary>
        /// Gets or sets the type of the script.
        /// </summary>
        public string Type { get; set; } = type;
    }
}
