using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Uia.DriverServer.Attributes;

namespace Uia.DriverServer.Extensions
{
    /// <summary>
    /// Provides a set of scan code maps for various keyboard layouts.
    /// </summary>
    public static class CodeMaps
    {
        /// <summary>
        /// Gets the scan code map for the English (US) keyboard layout.
        /// </summary>
        [KeyboardLayout(layout: "en-US")]
        public static Dictionary<string, ushort> EnUnitedStatesCodeMap => new(StringComparer.OrdinalIgnoreCase)
        {
            ["Esc"] = 0x01,       // Escape key
            ["1"] = 0x02,         // 1 key
            ["2"] = 0x03,         // 2 key
            ["3"] = 0x04,         // 3 key
            ["4"] = 0x05,         // 4 key
            ["5"] = 0x06,         // 5 key
            ["6"] = 0x07,         // 6 key
            ["7"] = 0x08,         // 7 key
            ["8"] = 0x09,         // 8 key
            ["9"] = 0x0A,         // 9 key
            ["0"] = 0x0B,         // 0 key
            ["-"] = 0x0C,         // Hyphen key
            ["="] = 0x0D,         // Equals key
            ["Backspace"] = 0x0E, // Backspace key
            ["Tab"] = 0x0F,       // Tab key
            ["Q"] = 0x10,         // Q key
            ["W"] = 0x11,         // W key
            ["E"] = 0x12,         // E key
            ["R"] = 0x13,         // R key
            ["T"] = 0x14,         // T key
            ["Y"] = 0x15,         // Y key
            ["U"] = 0x16,         // U key
            ["I"] = 0x17,         // I key
            ["O"] = 0x18,         // O key
            ["P"] = 0x19,         // P key
            ["["] = 0x1A,         // Open bracket key
            ["]"] = 0x1B,         // Close bracket key
            ["Enter"] = 0x1C,     // Enter key
            ["Ctrl"] = 0x1D,      // Control key
            ["A"] = 0x1E,         // A key
            ["S"] = 0x1F,         // S key
            ["D"] = 0x20,         // D key
            ["F"] = 0x21,         // F key
            ["G"] = 0x22,         // G key
            ["H"] = 0x23,         // H key
            ["J"] = 0x24,         // J key
            ["K"] = 0x25,         // K key
            ["L"] = 0x26,         // L key
            [";"] = 0x27,         // Semicolon key
            ["'"] = 0x28,         // Apostrophe key
            ["`"] = 0x29,         // Grave accent key
            ["LShift"] = 0x2A,    // Left Shift key
            [@"\"] = 0x2B,        // Backslash key
            ["Z"] = 0x2C,         // Z key
            ["X"] = 0x2D,         // X key
            ["C"] = 0x2E,         // C key
            ["V"] = 0x2F,         // V key
            ["B"] = 0x30,         // B key
            ["N"] = 0x31,         // N key
            ["M"] = 0x32,         // M key
            [","] = 0x33,         // Comma key
            ["."] = 0x34,         // Period key
            ["/"] = 0x35,         // Slash key
            ["RShift"] = 0x36,    // Right Shift key
            ["PrtSc"] = 0x37,     // Print Screen key
            ["Alt"] = 0x38,       // Alt key
            [" "] = 0x39,         // Spacebar key
            ["CapsLock"] = 0x3A,  // Caps Lock key
            ["F1"] = 0x3B,        // F1 key
            ["F2"] = 0x3C,        // F2 key
            ["F3"] = 0x3D,        // F3 key
            ["F4"] = 0x3E,        // F4 key
            ["F5"] = 0x3F,        // F5 key
            ["F6"] = 0x40,        // F6 key
            ["F7"] = 0x41,        // F7 key
            ["F8"] = 0x42,        // F8 key
            ["F9"] = 0x43,        // F9 key
            ["F10"] = 0x44,       // F10 key
            ["Num"] = 0x45,       // Num Lock key
            ["Scroll"] = 0x46,    // Scroll Lock key
            ["Home"] = 0x47,      // Home key
            ["Up"] = 0x48,        // Up arrow key
            ["PgUp"] = 0x49,      // Page Up key
            ["Left"] = 0x4B,      // Left arrow key
            ["Center"] = 0x4C,    // Center key
            ["Right"] = 0x4D,     // Right arrow key
            ["End"] = 0x4F,       // End key
            ["Down"] = 0x50,      // Down arrow key
            ["PgDn"] = 0x51,      // Page Down key
            ["Ins"] = 0x52,       // Insert key
            ["Del"] = 0x53        // Delete key
        };

        /// <summary>
        /// Gets the scan code map for the Hebrew Standard keyboard layout.
        /// </summary>
        [KeyboardLayout(layout: "he-IL")]
        public static Dictionary<string, ushort> HeStandardCodeMap => new(StringComparer.OrdinalIgnoreCase)
        {
            ["Esc"] = 0x01,
            ["1"] = 0x02,
            ["2"] = 0x03,
            ["3"] = 0x04,
            ["4"] = 0x05,
            ["5"] = 0x06,
            ["6"] = 0x07,
            ["7"] = 0x08,
            ["8"] = 0x09,
            ["9"] = 0x0A,
            ["0"] = 0x0B,
            ["-"] = 0x0C,
            ["="] = 0x0D,
            ["Backspace"] = 0x0E,
            ["Tab"] = 0x0F,
            ["/"] = 0x10,     // Q key
            ["'"] = 0x11,     // W key
            ["ק"] = 0x12,     // E key
            ["ר"] = 0x13,     // R key
            ["א"] = 0x14,     // T key
            ["ט"] = 0x15,     // Y key
            ["ו"] = 0x16,     // U key
            ["ן"] = 0x17,     // I key
            ["ם"] = 0x18,     // O key
            ["פ"] = 0x19,     // P key
            ["]"] = 0x1A,     // [
            ["["] = 0x1B,     // ]
            ["Enter"] = 0x1C,
            ["Ctrl"] = 0x1D,
            ["ש"] = 0x1E,     // A key
            ["ד"] = 0x1F,     // S key
            ["ג"] = 0x20,     // D key
            ["כ"] = 0x21,     // F key
            ["ע"] = 0x22,     // G key
            ["י"] = 0x23,     // H key
            ["ח"] = 0x24,     // J key
            ["ל"] = 0x25,     // K key
            ["ך"] = 0x26,     // L key
            ["ף"] = 0x27,     // ; key
            [","] = 0x28,     // ' key
            ["`"] = 0x29,     // Grave accent (`) key
            ["LShift"] = 0x2A,
            ["\\"] = 0x2B,
            ["ז"] = 0x2C,     // Z key
            ["ס"] = 0x2D,     // X key
            ["ב"] = 0x2E,     // C key
            ["ה"] = 0x2F,     // V key
            ["נ"] = 0x30,     // B key
            ["מ"] = 0x31,     // N key
            ["צ"] = 0x32,     // M key
            ["ת"] = 0x33,     // , key
            ["ץ"] = 0x34,     // . key
            ["."] = 0x35,     // / key
            ["RShift"] = 0x36,
            ["PrtSc"] = 0x37,
            ["Alt"] = 0x38,
            [" "] = 0x39,
            ["CapsLock"] = 0x3A,
            ["F1"] = 0x3B,
            ["F2"] = 0x3C,
            ["F3"] = 0x3D,
            ["F4"] = 0x3E,
            ["F5"] = 0x3F,
            ["F6"] = 0x40,
            ["F7"] = 0x41,
            ["F8"] = 0x42,
            ["F9"] = 0x43,
            ["F10"] = 0x44,
            ["Num"] = 0x45,
            ["Scroll"] = 0x46,
            ["Home"] = 0x47,
            ["Up"] = 0x48,
            ["PgUp"] = 0x49,
            ["Left"] = 0x4B,
            ["Center"] = 0x4C,
            ["Right"] = 0x4D,
            ["End"] = 0x4F,
            ["Down"] = 0x50,
            ["PgDn"] = 0x51,
            ["Ins"] = 0x52,
            ["Del"] = 0x53
        };

        /// <summary>
        /// Gets the scan code map for non-alphabetic keys.
        /// </summary>
        public static Dictionary<string, ushort> NonAlphabeticCodeMap => new(StringComparer.OrdinalIgnoreCase)
        {
            ["Esc"] = 0x01,        // Escape key
            ["1"] = 0x02,          // 1 key
            ["2"] = 0x03,          // 2 key
            ["3"] = 0x04,          // 3 key
            ["4"] = 0x05,          // 4 key
            ["5"] = 0x06,          // 5 key
            ["6"] = 0x07,          // 6 key
            ["7"] = 0x08,          // 7 key
            ["8"] = 0x09,          // 8 key
            ["9"] = 0x0A,          // 9 key
            ["0"] = 0x0B,          // 0 key
            ["-"] = 0x0C,          // Hyphen key
            ["="] = 0x0D,          // Equals key
            ["Backspace"] = 0x0E,  // Backspace key
            ["Tab"] = 0x0F,        // Tab key
            ["["] = 0x1A,          // Open bracket key
            ["]"] = 0x1B,          // Close bracket key
            ["Enter"] = 0x1C,      // Enter key
            ["Ctrl"] = 0x1D,       // Control key
            [";"] = 0x27,          // Semicolon key
            ["'"] = 0x28,          // Apostrophe key
            ["`"] = 0x29,          // Grave accent key
            ["LShift"] = 0x2A,     // Left Shift key
            [@"\"] = 0x2B,         // Backslash key
            [","] = 0x33,          // Comma key
            ["."] = 0x34,          // Period key
            ["/"] = 0x35,          // Slash key
            ["RShift"] = 0x36,     // Right Shift key
            ["PrtSc"] = 0x37,      // Print Screen key
            ["Alt"] = 0x38,        // Alt key
            [" "] = 0x39,          // Spacebar key
            ["CapsLock"] = 0x3A,   // Caps Lock key
            ["F1"] = 0x3B,         // F1 key
            ["F2"] = 0x3C,         // F2 key
            ["F3"] = 0x3D,         // F3 key
            ["F4"] = 0x3E,         // F4 key
            ["F5"] = 0x3F,         // F5 key
            ["F6"] = 0x40,         // F6 key
            ["F7"] = 0x41,         // F7 key
            ["F8"] = 0x42,         // F8 key
            ["F9"] = 0x43,         // F9 key
            ["F10"] = 0x44,        // F10 key
            ["Num"] = 0x45,        // Num Lock key
            ["Scroll"] = 0x46,     // Scroll Lock key
            ["Home"] = 0x47,       // Home key
            ["Up"] = 0x48,         // Up arrow key
            ["PgUp"] = 0x49,       // Page Up key
            ["Left"] = 0x4B,       // Left arrow key
            ["Center"] = 0x4C,     // Center key
            ["Right"] = 0x4D,      // Right arrow key
            ["End"] = 0x4F,        // End key
            ["Down"] = 0x50,       // Down arrow key
            ["PgDn"] = 0x51,       // Page Down key
            ["Ins"] = 0x52,        // Insert key
            ["Del"] = 0x53,        // Delete key
            ["\uE000"] = 0x01,     // Null
            ["\uE001"] = 0x02,     // Cancel
            ["\uE002"] = 0x03,     // Help
            ["\uE003"] = 0x0E,     // Backspace
            ["\uE004"] = 0x0F,     // Tab
            ["\uE005"] = 0x04,     // Clear
            ["\uE006"] = 0x1C,     // Return
            ["\uE007"] = 0x1C,     // Enter
            ["\uE008"] = 0x2A,     // Shift
            ["\uE009"] = 0x1D,     // Control
            ["\uE00A"] = 0x38,     // Alt
            ["\uE00B"] = 0x39,     // Pause
            ["\uE00C"] = 0x01,     // Escape
            ["\uE00D"] = 0x39,     // Space
            ["\uE00E"] = 0x49,     // PageUp
            ["\uE00F"] = 0x51,     // PageDown
            ["\uE010"] = 0x4F,     // End
            ["\uE011"] = 0x47,     // Home
            ["\uE012"] = 0x4B,     // Left / ArrowLeft
            ["\uE013"] = 0x48,     // Up / ArrowUp
            ["\uE014"] = 0x4D,     // Right / ArrowRight
            ["\uE015"] = 0x50,     // Down / ArrowDown
            ["\uE016"] = 0x52,     // Insert
            ["\uE017"] = 0x53,     // Delete
            ["\uE018"] = 0x27,     // Semicolon
            ["\uE019"] = 0x0D,     // Equal
            ["\uE01A"] = 0x52,     // NumberPad0
            ["\uE01B"] = 0x4F,     // NumberPad1
            ["\uE01C"] = 0x50,     // NumberPad2
            ["\uE01D"] = 0x51,     // NumberPad3
            ["\uE01E"] = 0x4B,     // NumberPad4
            ["\uE01F"] = 0x4C,     // NumberPad5
            ["\uE020"] = 0x4D,     // NumberPad6
            ["\uE021"] = 0x47,     // NumberPad7
            ["\uE022"] = 0x48,     // NumberPad8
            ["\uE023"] = 0x49,     // NumberPad9
            ["\uE024"] = 0x37,     // Multiply
            ["\uE025"] = 0x4E,     // Add
            ["\uE026"] = 0x04,     // Separator
            ["\uE027"] = 0x4A,     // Subtract
            ["\uE028"] = 0x53,     // Decimal
            ["\uE029"] = 0x35,     // Divide
            ["\uE031"] = 0x3B,     // F1
            ["\uE032"] = 0x3C,     // F2
            ["\uE033"] = 0x3D,     // F3
            ["\uE034"] = 0x3E,     // F4
            ["\uE035"] = 0x3F,     // F5
            ["\uE036"] = 0x40,     // F6
            ["\uE037"] = 0x41,     // F7
            ["\uE038"] = 0x42,     // F8
            ["\uE039"] = 0x43,     // F9
            ["\uE03A"] = 0x44,     // F10
            ["\uE03B"] = 0x57,     // F11
            ["\uE03C"] = 0x58,     // F12
            ["\uE03D"] = 0x5B,     // Meta / Command
            ["\uE040"] = 0x29      // ZenkakuHankaku
        };

        /// <summary>
        /// Gets the scan code map for keys that require a modifier. This map includes the modifier and the modified key code.
        /// </summary>
        public static Dictionary<string, (ushort Modifier, ushort ModifiedKeyCode)> ModifiedKeys => new()
        {
            ["!"] = (Modifier: 0x2A, ModifiedKeyCode: 0x02),  // Shift + 1
            ["@"] = (Modifier: 0x2A, ModifiedKeyCode: 0x03),  // Shift + 2
            ["#"] = (Modifier: 0x2A, ModifiedKeyCode: 0x04),  // Shift + 3
            ["$"] = (Modifier: 0x2A, ModifiedKeyCode: 0x05),  // Shift + 4
            ["%"] = (Modifier: 0x2A, ModifiedKeyCode: 0x06),  // Shift + 5
            ["^"] = (Modifier: 0x2A, ModifiedKeyCode: 0x07),  // Shift + 6
            ["&"] = (Modifier: 0x2A, ModifiedKeyCode: 0x08),  // Shift + 7
            ["*"] = (Modifier: 0x2A, ModifiedKeyCode: 0x09),  // Shift + 8
            ["("] = (Modifier: 0x2A, ModifiedKeyCode: 0x0A),  // Shift + 9
            [")"] = (Modifier: 0x2A, ModifiedKeyCode: 0x0B),  // Shift + 0
            ["_"] = (Modifier: 0x2A, ModifiedKeyCode: 0x0C),  // Shift + -
            ["+"] = (Modifier: 0x2A, ModifiedKeyCode: 0x0D),  // Shift + =
            [":"] = (Modifier: 0x2A, ModifiedKeyCode: 0x27),  // Shift + ';' (Colon)
            ["\""] = (Modifier: 0x2A, ModifiedKeyCode: 0x28), // Shift + '   (Quotation mark)
            ["<"] = (Modifier: 0x2A, ModifiedKeyCode: 0x33),  // Shift + ,
            [">"] = (Modifier: 0x2A, ModifiedKeyCode: 0x34),  // Shift + .
            ["?"] = (Modifier: 0x2A, ModifiedKeyCode: 0x35),  // Shift + /
            ["{"] = (Modifier: 0x2A, ModifiedKeyCode: 0x1A),  // Shift + [
            ["}"] = (Modifier: 0x2A, ModifiedKeyCode: 0x1B),  // Shift + ]
            ["|"] = (Modifier: 0x2A, ModifiedKeyCode: 0x2B),  // Shift + \
            ["~"] = (Modifier: 0x2A, ModifiedKeyCode: 0x29),  // Shift + `
            ["A"] = (Modifier: 0x2A, ModifiedKeyCode: 0x1E),  // Shift + A
            ["B"] = (Modifier: 0x2A, ModifiedKeyCode: 0x30),  // Shift + B
            ["C"] = (Modifier: 0x2A, ModifiedKeyCode: 0x2E),  // Shift + C
            ["D"] = (Modifier: 0x2A, ModifiedKeyCode: 0x20),  // Shift + D
            ["E"] = (Modifier: 0x2A, ModifiedKeyCode: 0x12),  // Shift + E
            ["F"] = (Modifier: 0x2A, ModifiedKeyCode: 0x21),  // Shift + F
            ["G"] = (Modifier: 0x2A, ModifiedKeyCode: 0x22),  // Shift + G
            ["H"] = (Modifier: 0x2A, ModifiedKeyCode: 0x23),  // Shift + H
            ["I"] = (Modifier: 0x2A, ModifiedKeyCode: 0x17),  // Shift + I
            ["J"] = (Modifier: 0x2A, ModifiedKeyCode: 0x24),  // Shift + J
            ["K"] = (Modifier: 0x2A, ModifiedKeyCode: 0x25),  // Shift + K
            ["L"] = (Modifier: 0x2A, ModifiedKeyCode: 0x26),  // Shift + L
            ["M"] = (Modifier: 0x2A, ModifiedKeyCode: 0x32),  // Shift + M
            ["N"] = (Modifier: 0x2A, ModifiedKeyCode: 0x31),  // Shift + N
            ["O"] = (Modifier: 0x2A, ModifiedKeyCode: 0x18),  // Shift + O
            ["P"] = (Modifier: 0x2A, ModifiedKeyCode: 0x19),  // Shift + P
            ["Q"] = (Modifier: 0x2A, ModifiedKeyCode: 0x10),  // Shift + Q
            ["R"] = (Modifier: 0x2A, ModifiedKeyCode: 0x13),  // Shift + R
            ["S"] = (Modifier: 0x2A, ModifiedKeyCode: 0x1F),  // Shift + S
            ["T"] = (Modifier: 0x2A, ModifiedKeyCode: 0x14),  // Shift + T
            ["U"] = (Modifier: 0x2A, ModifiedKeyCode: 0x16),  // Shift + U
            ["V"] = (Modifier: 0x2A, ModifiedKeyCode: 0x2F),  // Shift + V
            ["W"] = (Modifier: 0x2A, ModifiedKeyCode: 0x11),  // Shift + W
            ["X"] = (Modifier: 0x2A, ModifiedKeyCode: 0x2D),  // Shift + X
            ["Y"] = (Modifier: 0x2A, ModifiedKeyCode: 0x15),  // Shift + Y
            ["Z"] = (Modifier: 0x2A, ModifiedKeyCode: 0x2C),  // Shift + Z
            ["€"] = (Modifier: 0x40, ModifiedKeyCode: 0x05)   // AltGr + 5
        };

        /// <summary>
        /// Retrieves the default scan code mapping dictionary for the US English keyboard layout ("en-US").
        /// </summary>
        /// <returns>A dictionary mapping keys to their corresponding scan codes for the US English layout.</returns>
        public static Dictionary<string, ushort> GetLayoutMap()
        {
            // Delegate to the overload of GetLayoutMap, specifying "en-US" as the default layout.
            return GetLayoutMap("en-US");
        }

        /// <summary>
        /// Retrieves the scan code mapping dictionary for the specified keyboard layout.
        /// </summary>
        /// <param name="layout">The identifier for the desired keyboard layout (e.g., "en-US", "he-IL").</param>
        /// <returns>
        /// A dictionary mapping keys to their corresponding scan codes for the specified layout.
        /// If no matching layout is found, the default US English code map is returned.
        /// </returns>
        public static Dictionary<string, ushort> GetLayoutMap(string layout)
        {
            // Use ordinal (case-insensitive) comparison for matching layout identifiers.
            const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

            // Retrieve all public static properties from the CodeMaps class that are decorated with KeyboardLayoutAttribute.
            var maps = typeof(CodeMaps)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(i => i.GetCustomAttribute<KeyboardLayoutAttribute>() != null);

            // Find the property whose associated KeyboardLayoutAttribute matches the provided layout.
            var mapProperty = maps.FirstOrDefault(i => i
                .GetCustomAttribute<KeyboardLayoutAttribute>()
                .Layout.Equals(layout, comparison));

            // Retrieve the scan code mapping dictionary from the found property.
            // If no property matches, fall back to the default US English code map.
            return mapProperty?.GetValue(null) as Dictionary<string, ushort> ?? EnUnitedStatesCodeMap;
        }

        /// <summary>
        /// Retrieves a collection of all keyboard layout identifiers defined in the CodeMaps class.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/> containing all layout identifiers (e.g., "en-US", "he-IL").</returns>
        public static IEnumerable<string> GetLayouts()
        {
            // Retrieve all public static properties from the CodeMaps class.
            // For each property, attempt to get the KeyboardLayoutAttribute and select its Layout property.
            // Finally, filter out any null values.
            return typeof(CodeMaps)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Select(i => i.GetCustomAttribute<KeyboardLayoutAttribute>()?.Layout)
                .Where(i => i != null);
        }
    }
}
