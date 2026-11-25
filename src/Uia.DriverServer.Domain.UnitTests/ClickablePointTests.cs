using Microsoft.VisualStudio.TestTools.UnitTesting;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory("ClickablePoint")]
    [TestCategory("UnitTest")]
    public sealed class ClickablePointTests
    {
        [TestMethod(DisplayName = "Verify that every alignment, the center included, applies the requested offsets.")]
        #region *** Data Set ***
        [DataRow("TopLeft", 100, 200)]
        [DataRow("TopCenter", 150, 200)]
        [DataRow("TopRight", 200, 200)]
        [DataRow("MiddleLeft", 100, 220)]
        [DataRow("MiddleCenter", 150, 220)]
        [DataRow("MiddleRight", 200, 220)]
        [DataRow("BottomLeft", 100, 240)]
        [DataRow("BottomCenter", 150, 240)]
        [DataRow("BottomRight", 200, 240)]
        #endregion
        public void GetClickablePointOffsetTest(string align, int anchorX, int anchorY)
        {
            // Arrange: a 100 x 40 element at (100, 200), whose alignment points are the given anchors.
            var element = NewElement();

            // Act: resolve the point with a positive and a negative offset.
            var shifted = element.GetClickablePoint(align, topOffset: 7, leftOffset: 12, scaleRatio: 1.0D);
            var pulled = element.GetClickablePoint(align, topOffset: -3, leftOffset: -5, scaleRatio: 1.0D);

            // Assert: the offsets move the anchor of every alignment by exactly their value.
            Assert.AreEqual(expected: anchorX + 12, actual: shifted.X);
            Assert.AreEqual(expected: anchorY + 7, actual: shifted.Y);
            Assert.AreEqual(expected: anchorX - 5, actual: pulled.X);
            Assert.AreEqual(expected: anchorY - 3, actual: pulled.Y);
        }

        [TestMethod(DisplayName = "Verify that an unrecognized alignment resolves to the center and keeps the " +
            "offsets.")]
        public void GetClickablePointUnknownAlignmentTest()
        {
            // Arrange: the same element, with an alignment the driver does not know.
            var element = NewElement();

            // Act: resolve the point with offsets.
            var point = element.GetClickablePoint(align: "Somewhere", topOffset: 4, leftOffset: 6, scaleRatio: 1.0D);

            // Assert: the point is the center moved by the offsets.
            Assert.AreEqual(expected: 156, actual: point.X);
            Assert.AreEqual(expected: 224, actual: point.Y);
        }

        [TestMethod(DisplayName = "Verify that an alignment without offsets lands on the center exactly and " +
            "nudges an edge one pixel.")]
        #region *** Data Set ***
        [DataRow("MiddleCenter", 150, 220)]
        [DataRow("middlecenter", 150, 220)]
        [DataRow("TopLeft", 101, 201)]
        [DataRow("BottomRight", 201, 241)]
        #endregion
        public void GetClickablePointDefaultOffsetTest(string align, int expectedX, int expectedY)
        {
            // Arrange: the same element, resolved through the overloads that name only an alignment.
            var element = NewElement();

            // Act: resolve the point with and without a scale ratio.
            var point = element.GetClickablePoint(align);
            var scaledPoint = element.GetClickablePoint(align, scaleRatio: 1.0D);

            // Assert: the center keeps its exact point, and edges keep their one-pixel nudge.
            Assert.AreEqual(expected: expectedX, actual: point.X);
            Assert.AreEqual(expected: expectedY, actual: point.Y);
            Assert.AreEqual(expected: expectedX, actual: scaledPoint.X);
            Assert.AreEqual(expected: expectedY, actual: scaledPoint.Y);
        }

        // Creates an element model with a fixed rectangle, so the point math is tested without a UI Automation
        // provider.
        private static UiaElementModel NewElement()
        {
            return new UiaElementModel
            {
                Rectangle = new RectangleModel
                {
                    Bottom = 240,
                    Left = 100,
                    Right = 200,
                    Top = 200
                }
            };
        }
    }
}
