using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory("KeyboardInput")]
    [TestCategory("UnitTest")]
    public sealed class KeyboardInputTests
    {
        // Flags expected on a scan-code key down input for a navigation or system key.
        private const uint ExtendedKeyDownFlags = (uint)(KeyEvent.Scancode | KeyEvent.ExtendedKey);

        // Flags expected on a scan-code key up input for a navigation or system key.
        private const uint ExtendedKeyUpFlags = (uint)(KeyEvent.Scancode | KeyEvent.KeyUp | KeyEvent.ExtendedKey);

        // Flags expected on a scan-code key down input for a key that is not extended.
        private const uint PlainKeyDownFlags = (uint)KeyEvent.Scancode;

        #region *** Data Set ***
        [DataRow("en-US")]
        [DataRow("he-IL")]
        #endregion
        [TestMethod(DisplayName = "Verify that the extended-key flag applies in every keyboard layout.")]
        public void ConvertToInputsExtendedKeyLayoutTest(string keyboardLayout)
        {
            // Act: convert the Up arrow in the requested layout.
            var down = "Up".ConvertToInputs(keyboardLayout, KeyEvent.KeyDown).Single();

            // Assert: verify that the layout does not change the extended-key decision.
            Assert.AreEqual(expected: (ushort)0x48, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: ExtendedKeyDownFlags, actual: down.union.ki.dwFlags);
        }

        #region *** Data Set ***
        [DataRow("Up", (ushort)0x48)]
        [DataRow("Down", (ushort)0x50)]
        [DataRow("Left", (ushort)0x4B)]
        [DataRow("Right", (ushort)0x4D)]
        [DataRow("Home", (ushort)0x47)]
        [DataRow("End", (ushort)0x4F)]
        [DataRow("PgUp", (ushort)0x49)]
        [DataRow("PgDn", (ushort)0x51)]
        [DataRow("Ins", (ushort)0x52)]
        [DataRow("Del", (ushort)0x53)]
        [DataRow("PrtSc", (ushort)0x37)]
        [DataRow("Win", (ushort)0x5B)]
        [DataRow("LWin", (ushort)0x5B)]
        [DataRow("RWin", (ushort)0x5C)]
        [DataRow("up", (ushort)0x48)]
        [DataRow("win", (ushort)0x5B)]
        #endregion
        [TestMethod(DisplayName = "Verify that navigation and system keys carry the extended-key flag " +
            "on the non-sticky path.")]
        public void ConvertToInputsExtendedKeyTest(string key, ushort scanCode)
        {
            // Act: convert the key name to its key down and key up inputs.
            var down = key.ConvertToInputs(keyboardLayout: "en-US", KeyEvent.KeyDown).Single();
            var up = key.ConvertToInputs(keyboardLayout: "en-US", KeyEvent.KeyUp).Single();

            // Assert: verify the scan code and that both inputs carry the extended-key flag.
            Assert.AreEqual(expected: scanCode, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: ExtendedKeyDownFlags, actual: down.union.ki.dwFlags);
            Assert.AreEqual(expected: ExtendedKeyUpFlags, actual: up.union.ki.dwFlags);
        }

        [TestMethod(DisplayName = "Verify that a modified character still produces its Shift and key inputs.")]
        public void ConvertToInputsModifiedKeyTest()
        {
            // Act: convert a Shift-modified character on the non-sticky path.
            var inputs = "!".ConvertToInputs(keyboardLayout: "en-US", KeyEvent.KeyDown).ToArray();

            // Assert: verify that Shift (0x2A) and 1 (0x02) are pressed as plain scan-code inputs.
            Assert.HasCount(expected: 2, collection: inputs);
            Assert.AreEqual(expected: (ushort)0x2A, actual: inputs[0].union.ki.wScan);
            Assert.AreEqual(expected: (ushort)0x02, actual: inputs[1].union.ki.wScan);
            Assert.AreEqual(expected: PlainKeyDownFlags, actual: inputs[0].union.ki.dwFlags);
            Assert.AreEqual(expected: PlainKeyDownFlags, actual: inputs[1].union.ki.dwFlags);
        }

        #region *** Data Set ***
        [DataRow("\\uE01A", (ushort)0x52)]
        [DataRow("\\uE01B", (ushort)0x4F)]
        [DataRow("\\uE01C", (ushort)0x50)]
        [DataRow("\\uE01D", (ushort)0x51)]
        [DataRow("\\uE01E", (ushort)0x4B)]
        [DataRow("\\uE01F", (ushort)0x4C)]
        [DataRow("\\uE020", (ushort)0x4D)]
        [DataRow("\\uE021", (ushort)0x47)]
        [DataRow("\\uE022", (ushort)0x48)]
        [DataRow("\\uE023", (ushort)0x49)]
        [DataRow("\\uE028", (ushort)0x53)]
        #endregion
        [TestMethod(DisplayName = "Verify that numeric keypad tokens stay keypad keys although they share " +
            "navigation scan codes.")]
        public void ConvertToInputsNumericKeypadTokenTest(string token, ushort scanCode)
        {
            // Act: convert the WebDriver numeric keypad token on the functional-key path.
            var inputs = token.ConvertToInputs(keyboardLayout: "en-US").ToArray();

            // Assert: verify the shared scan code is sent without the extended-key flag.
            Assert.HasCount(expected: 2, collection: inputs);
            Assert.AreEqual(expected: scanCode, actual: inputs[0].union.ki.wScan);
            Assert.AreEqual(expected: PlainKeyDownFlags, actual: inputs[0].union.ki.dwFlags);
        }

        #region *** Data Set ***
        [DataRow("Ctrl", (ushort)0x1D)]
        [DataRow("Alt", (ushort)0x38)]
        [DataRow("LShift", (ushort)0x2A)]
        [DataRow("RShift", (ushort)0x36)]
        [DataRow("Enter", (ushort)0x1C)]
        [DataRow("Tab", (ushort)0x0F)]
        [DataRow("F5", (ushort)0x3F)]
        [DataRow("F11", (ushort)0x57)]
        [DataRow("F12", (ushort)0x58)]
        [DataRow("Center", (ushort)0x4C)]
        [DataRow("Num", (ushort)0x45)]
        [DataRow("c", (ushort)0x2E)]
        [DataRow("1", (ushort)0x02)]
        #endregion
        [TestMethod(DisplayName = "Verify that keys outside the extended set keep plain scan-code flags.")]
        public void ConvertToInputsPlainKeyTest(string key, ushort scanCode)
        {
            // Act: convert the key name to its key down input.
            var down = key.ConvertToInputs(keyboardLayout: "en-US", KeyEvent.KeyDown).Single();

            // Assert: verify the scan code is sent without the extended-key flag.
            Assert.AreEqual(expected: scanCode, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: PlainKeyDownFlags, actual: down.union.ki.dwFlags);
        }

        #region *** Data Set ***
        [DataRow("\\uE013", (ushort)0x48)]
        [DataRow("\\uE017", (ushort)0x53)]
        [DataRow("\\uE03D", (ushort)0x5B)]
        #endregion
        [TestMethod(DisplayName = "Verify that WebDriver navigation and Meta tokens carry the extended-key flag.")]
        public void ConvertToInputsWebDriverExtendedTokenTest(string token, ushort scanCode)
        {
            // Act: convert the WebDriver token on the functional-key path.
            var inputs = token.ConvertToInputs(keyboardLayout: "en-US").ToArray();

            // Assert: verify that both the key down and key up inputs carry the extended-key flag.
            Assert.HasCount(expected: 2, collection: inputs);
            Assert.AreEqual(expected: scanCode, actual: inputs[0].union.ki.wScan);
            Assert.AreEqual(expected: ExtendedKeyDownFlags, actual: inputs[0].union.ki.dwFlags);
            Assert.AreEqual(expected: ExtendedKeyUpFlags, actual: inputs[1].union.ki.dwFlags);
        }

        [TestMethod(DisplayName = "Verify that a sticky navigation key carries the extended-key flag " +
            "on press and release.")]
        public void ConvertToScanCodeInputExtendedKeyTest()
        {
            // Act: convert the Up arrow for the sticky press and release steps.
            var down = "Up".ConvertToScanCodeInput(keyboardLayout: "en-US", KeyEvent.KeyDown);
            var up = "Up".ConvertToScanCodeInput(keyboardLayout: "en-US", KeyEvent.KeyUp);

            // Assert: verify that both steps send the Up scan code as an extended key.
            Assert.AreEqual(expected: (ushort)0x48, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: ExtendedKeyDownFlags, actual: down.union.ki.dwFlags);
            Assert.AreEqual(expected: ExtendedKeyUpFlags, actual: up.union.ki.dwFlags);
        }

        [TestMethod(DisplayName = "Verify that a sticky modifier key keeps plain scan-code flags.")]
        public void ConvertToScanCodeInputPlainKeyTest()
        {
            // Act: convert the Ctrl key for the sticky press step.
            var down = "Ctrl".ConvertToScanCodeInput(keyboardLayout: "en-US", KeyEvent.KeyDown);

            // Assert: verify that the modifier is sent without the extended-key flag.
            Assert.AreEqual(expected: (ushort)0x1D, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: PlainKeyDownFlags, actual: down.union.ki.dwFlags);
        }

        [TestMethod(DisplayName = "Verify that an unknown sticky key keeps the existing invalid scan code.")]
        public void ConvertToScanCodeInputUnknownKeyTest()
        {
            // Act: convert a key name that no layout map contains.
            var down = "ArrowUp".ConvertToScanCodeInput(keyboardLayout: "en-US", KeyEvent.KeyDown);

            // Assert: verify that the unknown key resolves to ushort.MaxValue without the extended-key flag.
            Assert.AreEqual(expected: ushort.MaxValue, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: PlainKeyDownFlags, actual: down.union.ki.dwFlags);
        }

        #region *** Data Set ***
        [DataRow("en-US")]
        [DataRow("he-IL")]
        #endregion
        [TestMethod(DisplayName = "Verify that a sticky Windows key is extended on press and release " +
            "in every keyboard layout.")]
        public void ConvertToScanCodeInputWindowsKeyTest(string keyboardLayout)
        {
            // Act: convert the Windows key for the sticky press and release steps of a chord such as Win+R.
            var down = "Win".ConvertToScanCodeInput(keyboardLayout, KeyEvent.KeyDown);
            var up = "Win".ConvertToScanCodeInput(keyboardLayout, KeyEvent.KeyUp);

            // Assert: verify that both steps send the left Windows scan code as an extended key.
            Assert.AreEqual(expected: (ushort)0x5B, actual: down.union.ki.wScan);
            Assert.AreEqual(expected: ExtendedKeyDownFlags, actual: down.union.ki.dwFlags);
            Assert.AreEqual(expected: ExtendedKeyUpFlags, actual: up.union.ki.dwFlags);
        }
    }
}
