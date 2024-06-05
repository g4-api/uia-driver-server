/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the model for impersonation settings.
    /// </summary>
    public class ImpersonationModel
    {
        /// <summary>
        /// Gets or sets the domain for the impersonation.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether impersonation is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the password for the impersonation.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the username for the impersonation.
        /// </summary>
        public string Username { get; set; }
    }
}
