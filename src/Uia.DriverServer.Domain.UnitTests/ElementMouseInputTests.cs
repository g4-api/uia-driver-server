using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Marshals.Models;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory("ElementMouseInput")]
    [TestCategory("UnitTest")]
    public sealed class ElementMouseInputTests
    {
        // The session element id the tests register, so the known and unknown cases share one lookup key.
        private const string KnownElementId = "known-element";

        [TestMethod(DisplayName = "Verify that a mouse input for a known element targets the element's top-left " +
            "clickable point.")]
        public void NewMouseInputKnownElementTest()
        {
            // Arrange: a session that holds one 100 x 40 element at (100, 200).
            var session = NewSession();

            // Act: build the input for the registered element.
            var input = session.NewMouseInput(MouseEvent.LeftDown, KnownElementId);

            // Assert: the input lands one pixel inside the element's top-left corner, with the requested event.
            Assert.AreEqual(expected: 101, actual: input.union.mi.dx);
            Assert.AreEqual(expected: 201, actual: input.union.mi.dy);
            Assert.AreEqual(expected: (uint)MouseEvent.LeftDown, actual: input.union.mi.dwFlags);
        }

        [TestMethod(DisplayName = "Verify that a mouse input for an unknown element falls back to the cursor " +
            "instead of throwing.")]
        public void NewMouseInputUnknownElementTest()
        {
            // Arrange: the same session, asked for an element id it never registered.
            var session = NewSession();

            // Act: build the input for the unknown element; before the fix this threw KeyNotFoundException.
            var input = session.NewMouseInput(MouseEvent.LeftUp, "unknown-element");

            // Assert: the input keeps the requested event and does not use the registered element's point.
            Assert.AreEqual(expected: (uint)MouseEvent.LeftUp, actual: input.union.mi.dwFlags);
            Assert.IsFalse(input.union.mi.dx == 101 && input.union.mi.dy == 201);
        }

        // Creates a session with one element at a fixed rectangle, so the lookup is tested without a UI
        // Automation provider.
        private static UiaSessionResponseModel NewSession()
        {
            // Register the element under the known id, the way the driver stores found elements.
            var element = new UiaElementModel
            {
                Rectangle = new RectangleModel
                {
                    Bottom = 240,
                    Left = 100,
                    Right = 200,
                    Top = 200
                }
            };

            // Return a session whose element map holds only that element.
            return new UiaSessionResponseModel
            {
                Elements = new Dictionary<string, UiaElementModel>
                {
                    [KnownElementId] = element
                }
            };
        }
    }
}
