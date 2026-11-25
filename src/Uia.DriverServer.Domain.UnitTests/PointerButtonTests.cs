using Microsoft.VisualStudio.TestTools.UnitTesting;

using Uia.DriverServer.Marshals.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory("PointerButton")]
    [TestCategory("UnitTest")]
    public sealed class PointerButtonTests
    {
        [TestMethod(DisplayName = "Verify that each W3C pointer button presses and releases its own mouse button.")]
        #region *** Data Set ***
        [DataRow(0, true, MouseEvent.LeftDown)]
        [DataRow(0, false, MouseEvent.LeftUp)]
        [DataRow(1, true, MouseEvent.MiddleDown)]
        [DataRow(1, false, MouseEvent.MiddleUp)]
        [DataRow(2, true, MouseEvent.RightDown)]
        [DataRow(2, false, MouseEvent.RightUp)]
        #endregion
        public void ConvertToMouseEventTest(int button, bool isPress, MouseEvent expected)
        {
            // Act: convert the pointer button and transition into a User32 mouse event.
            var mouseEvent = ActionsRepository.ConvertToMouseEvent(button, isPress);

            // Assert: each button and transition maps to its own event, so the middle button no longer acts as the left one.
            Assert.AreEqual(expected, mouseEvent);
        }

        [TestMethod(DisplayName = "Verify that a button the driver does not map keeps pressing the left button.")]
        #region *** Data Set ***
        [DataRow(3, true, MouseEvent.LeftDown)]
        [DataRow(4, false, MouseEvent.LeftUp)]
        [DataRow(-1, true, MouseEvent.LeftDown)]
        #endregion
        public void ConvertToMouseEventFallbackTest(int button, bool isPress, MouseEvent expected)
        {
            // Act: convert a button outside left, middle, and right.
            var mouseEvent = ActionsRepository.ConvertToMouseEvent(button, isPress);

            // Assert: the left button is used, as the driver always did, so existing clients keep their behavior.
            Assert.AreEqual(expected, mouseEvent);
        }
    }
}
