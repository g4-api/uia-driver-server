using Microsoft.VisualStudio.TestTools.UnitTesting;

using Uia.DriverServer.Domain;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory(nameof(XpathPosition))]
    [TestCategory("UnitTest")]
    public sealed class XpathPositionTests
    {
        [TestMethod(DisplayName = "Verify that position one selects collection index zero.")]
        public void GetSelectionFirstPositionTest()
        {
            // Arrange: use the first repeated recorder selector within a two-element condition result.
            const string segment = "Pane[@AutomationId='TwinPanel'][1]";

            // Act: parse and validate the recorder's 1-based position.
            var selection = XpathPosition.GetSelection(pathSegment: segment, matchCount: 2);

            // Assert: verify that position one maps to collection index zero without changing predicate presence.
            Assert.IsTrue(condition: selection.HasPosition);
            Assert.AreEqual(expected: 0, actual: selection.Index);
        }

        [TestMethod(DisplayName = "Verify that a segment without a position preserves first-match behavior.")]
        public void GetSelectionMissingPositionTest()
        {
            // Arrange: use a property selector without a terminal numeric predicate.
            const string segment = "Pane[@AutomationId='TwinPanel']";

            // Act: parse the optional position against the available condition result.
            var selection = XpathPosition.GetSelection(pathSegment: segment, matchCount: 2);

            // Assert: verify that callers can retain FindFirst only when no position was supplied.
            Assert.IsFalse(condition: selection.HasPosition);
            Assert.AreEqual(expected: -1, actual: selection.Index);
        }

        [TestMethod(DisplayName = "Verify that an out-of-range position cannot access the collection.")]
        public void GetSelectionOutOfRangePositionTest()
        {
            // Arrange: request a third match from a condition result containing only two elements.
            const string segment = "Pane[@AutomationId='TwinPanel'][3]";

            // Act: validate the supplied position against the exact result count.
            var selection = XpathPosition.GetSelection(pathSegment: segment, matchCount: 2);

            // Assert: verify that predicate presence is retained while collection access is rejected.
            Assert.IsTrue(condition: selection.HasPosition);
            Assert.AreEqual(expected: -1, actual: selection.Index);
        }

        [TestMethod(DisplayName = "Verify that position two selects collection index one.")]
        public void GetSelectionSecondPositionTest()
        {
            // Arrange: use the second repeated recorder selector within a two-element condition result.
            const string segment = "Pane[@AutomationId='TwinPanel'][2]";

            // Act: parse and validate the recorder's 1-based position.
            var selection = XpathPosition.GetSelection(pathSegment: segment, matchCount: 2);

            // Assert: verify that the second sibling maps to collection index one.
            Assert.IsTrue(condition: selection.HasPosition);
            Assert.AreEqual(expected: 1, actual: selection.Index);
        }

        [TestMethod(DisplayName = "Verify that zero cannot select the first collection element.")]
        public void GetSelectionZeroPositionTest()
        {
            // Arrange: use a numeric predicate outside the recorder's 1-based position contract.
            const string segment = "Pane[@AutomationId='TwinPanel'][0]";

            // Act: validate the supplied zero position against an otherwise valid result count.
            var selection = XpathPosition.GetSelection(pathSegment: segment, matchCount: 2);

            // Assert: verify that zero remains an invalid supplied predicate instead of being coerced.
            Assert.IsTrue(condition: selection.HasPosition);
            Assert.AreEqual(expected: -1, actual: selection.Index);
        }

        [TestMethod(DisplayName = "Verify that XpathParser accepts the recorder's indexed step syntax.")]
        public void XpathPositionParserCompatibilityTest()
        {
            // Arrange: use the canonical indexed property selector emitted for a repeated branch.
            const string segment = "Pane[@AutomationId='TwinPanel'][2]";

            // Act: convert the indexed step through the production UIA condition parser.
            var condition = XpathParser.ConvertToCondition(xpath: segment);

            // Assert: verify that the position can coexist with the property condition consumed by the repository.
            Assert.IsNotNull(value: condition);
        }
    }
}
