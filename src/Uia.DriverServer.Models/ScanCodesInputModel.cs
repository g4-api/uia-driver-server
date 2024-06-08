/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the input model for scan code operations.
    /// </summary>
    public class ScanCodesInputModel
    {
        /// <summary>
        /// Gets or sets the collection of scan codes.
        /// </summary>
        /// <value>A collection of scan codes required for the operation.</value>
        [Required]
        public IEnumerable<string> ScanCodes { get; set; }
    }
}
