/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the input model for text input operations.
    /// </summary>
    public class TextInputModel
    {
        /// <summary>
        /// Gets or sets the text to be input.
        /// </summary>
        /// <value>The text to be input.</value>
        [Required]
        public string Text { get; set; }
    }
}
