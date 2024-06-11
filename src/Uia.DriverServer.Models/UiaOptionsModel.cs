/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents options for UI Automation.
    /// </summary>
    public class UiaOptionsModel
    {
        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>The application path as a string.</value>
        public string App { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>An array of strings representing the arguments.</value>
        public string[] Arguments { get; set; }

        /// <summary>
        /// Gets or sets the impersonation model.
        /// </summary>
        /// <value>The <see cref="ImpersonationModel"/> instance.</value>
        public ImpersonationModel Impersonation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to mount.
        /// </summary>
        /// <value><c>true</c> if mount; otherwise, <c>false</c>.</value>
        public bool Mount { get; set; }

        /// <summary>
        /// Gets or sets the scale ratio.
        /// </summary>
        /// <value>The scale ratio as a double.</value>
        public double ScaleRatio { get; set; } = 1.0D;

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>The working directory as a string.</value>
        public string WorkingDirectory { get; set; }
    }
}
