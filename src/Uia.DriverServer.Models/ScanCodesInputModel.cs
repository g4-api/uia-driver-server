﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the input model for scan code operations.
    /// </summary>
    public class ScanCodesInputModel
    {
        /// <summary>
        /// Gets or sets the options for the scan codes operation.
        /// </summary>
        public User32KeyboardInputOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the collection of scan codes.
        /// </summary>
        /// <value>A collection of scan codes required for the operation.</value>
        [Required]
        public IEnumerable<string> ScanCodes { get; set; }
    }
}
