namespace Uia.DriverServer.Models
{
    /// <summary>
    /// A class representing the options for text input operation.
    /// </summary>
    public class User32KeyboardInputOptions
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
