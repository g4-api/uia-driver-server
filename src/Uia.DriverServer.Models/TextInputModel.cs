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
        [Required(AllowEmptyStrings = true)]
        public string Text { get; set; }
    }
}
