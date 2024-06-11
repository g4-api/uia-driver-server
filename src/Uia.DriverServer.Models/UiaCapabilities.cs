/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Provides constants for various UI Automation capabilities.
    /// </summary>
    public static class UiaCapabilities
    {
        /// <summary>
        /// The always match setting.
        /// </summary>
        public const string AlwaysMatch = "alwaysMatch";

        /// <summary>
        /// The application to be automated.
        /// </summary>
        public const string Application = "app";

        /// <summary>
        /// The arguments to be passed to the application.
        /// </summary>
        public const string Arguments = "arguments";

        /// <summary>
        /// The path to the driver.
        /// </summary>
        public const string DriverPath = "driverPath";

        /// <summary>
        /// The impersonation settings.
        /// </summary>
        public const string Impersonation = "impersonation";

        /// <summary>
        /// The mount point for the application.
        /// </summary>
        public const string Mount = "mount";

        /// <summary>
        /// Additional options for UI Automation.
        /// </summary>
        public const string Options = "uia:options";

        /// <summary>
        /// The name of the platform.
        /// </summary>
        public const string PlatformName = "platformName";

        /// <summary>
        /// The platform version.
        /// </summary>
        public const string PlatformVersion = "platformVersion";

        /// <summary>
        /// The scale ratio for the application.
        /// </summary>
        public const string ScaleRatio = "scaleRatio";

        /// <summary>
        /// The working directory for the application.
        /// </summary>
        public const string WorkingDirectory = "workingDirectory";
    }
}
