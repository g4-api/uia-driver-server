using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the input model for modified key operations.
    /// </summary>
    public class ModifiedKeyInputModel
    {
        /// <summary>
        /// Gets or sets the key to be pressed.
        /// </summary>
        /// <value>The key to be pressed.</value>
        [Required]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the modifier key to be used.
        /// </summary>
        /// <value>The modifier key (e.g., Ctrl, Alt) to be used.</value>
        [Required]
        public string Modifier { get; set; }
    }
}
