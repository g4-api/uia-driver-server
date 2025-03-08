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

        /// <summary>
        /// Gets or sets the options for the scan codes operation.
        /// </summary>
        public ScanCodeOptions Options { get; set; }

        /// <summary>
        /// A class representing the options for the scan codes operation.
        /// </summary>
        public class ScanCodeOptions
        {
            /// <summary>
            /// Gets or sets the delay between each key press.
            /// </summary>
            public int Delay { get; set; }

            /// <summary>
            /// Gets or sets the keyboard layout to use for the scan codes.
            /// </summary>
            public string KeyboardLayout { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether keys are being held down first and then released.
            /// </summary>
            public bool StickyKeys { get; set; }

            /// <summary>
            /// Provides a set of constants representing various keyboard layouts.
            /// </summary>
            public static class KeyboardLayouts
            {
                /// <summary>
                /// Represents the US English keyboard layout.
                /// </summary>
                public const string EnglishUnitedStates = "en-US";

                /// <summary>
                /// Represents the Hebrew Standard keyboard layout.
                /// </summary>
                public const string HebrewStandard = "he-IL";
            }
        }
    }
}
