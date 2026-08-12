using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

using UIAutomationClient;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory(nameof(XpathParser))]
    [TestCategory("UnitTest")]
    public sealed class XpathParserTests
    {
        [TestMethod(DisplayName = "Verify that a legacy concatenated pattern-property name resolves to its property id.")]
        public void LegacyConcatenatedPatternNameResolvesTest()
        {
            // Act: convert a predicate that uses the historical concatenated spelling.
            var condition = XpathParser.ConvertToCondition(xpath: "@AnnotationDateTime='2024-01-01'");

            // Assert: the predicate maps to the Annotation.DateTime property id and keeps its value.
            AssertProperty(condition, UIA_PropertyIds.UIA_AnnotationDateTimePropertyId, "2024-01-01");
        }

        [TestMethod(DisplayName = "Verify that a dotted pattern-property name resolves to its property id.")]
        public void DottedPatternNameResolvesTest()
        {
            // Act: convert a predicate that uses the dotted Inspector-style spelling (this also exercises the tokenizer).
            var condition = XpathParser.ConvertToCondition(xpath: "@Annotation.DateTime='2024-01-01'");

            // Assert: the dotted name now tokenizes as one predicate and maps to the same property id.
            AssertProperty(condition, UIA_PropertyIds.UIA_AnnotationDateTimePropertyId, "2024-01-01");
        }

        [TestMethod(DisplayName = "Verify that a Pattern-qualified pattern-property name resolves to its property id.")]
        public void PatternQualifiedNameResolvesTest()
        {
            // Act: convert a predicate that uses the Pattern-qualified spelling.
            var condition = XpathParser.ConvertToCondition(xpath: "@AnnotationPattern.DateTime='2024-01-01'");

            // Assert: the Pattern-qualified alias maps to the same property id.
            AssertProperty(condition, UIA_PropertyIds.UIA_AnnotationDateTimePropertyId, "2024-01-01");
        }

        [TestMethod(DisplayName = "Verify that the Value pattern aliases all resolve to the same property id.")]
        public void ValuePatternAliasesResolveTest()
        {
            // Act: convert each supported spelling of the Value pattern's Value property.
            var concatenated = XpathParser.ConvertToCondition(xpath: "@ValueValue='ok'");
            var dotted = XpathParser.ConvertToCondition(xpath: "@Value.Value='ok'");
            var qualified = XpathParser.ConvertToCondition(xpath: "@ValuePattern.Value='ok'");

            // Assert: every alias resolves to UIA_ValueValuePropertyId.
            AssertProperty(concatenated, UIA_PropertyIds.UIA_ValueValuePropertyId, "ok");
            AssertProperty(dotted, UIA_PropertyIds.UIA_ValueValuePropertyId, "ok");
            AssertProperty(qualified, UIA_PropertyIds.UIA_ValueValuePropertyId, "ok");
        }

        [TestMethod(DisplayName = "Verify that a plain element property name resolves to its property id.")]
        public void ElementPropertyResolvesTest()
        {
            // Act: convert predicates for the two most common element properties.
            var name = XpathParser.ConvertToCondition(xpath: "@Name='Submit'");
            var automationId = XpathParser.ConvertToCondition(xpath: "@AutomationId='submit-button'");

            // Assert: each resolves to its element property id and keeps its value.
            AssertProperty(name, UIA_PropertyIds.UIA_NamePropertyId, "Submit");
            AssertProperty(automationId, UIA_PropertyIds.UIA_AutomationIdPropertyId, "submit-button");
        }

        [TestMethod(DisplayName = "Verify that a control type name resolves to a control-type property condition.")]
        public void ControlTypeResolvesTest()
        {
            // Act: convert a bare control-type step.
            var condition = XpathParser.ConvertToCondition(xpath: "Button");

            // Assert: the step becomes a ControlType property condition carrying the Button control-type id.
            var property = CastProperty(condition);
            Assert.AreEqual(UIA_PropertyIds.UIA_ControlTypePropertyId, property.propertyId);
            Assert.AreEqual(UIA_ControlTypeIds.UIA_ButtonControlTypeId, Convert.ToInt32(property.PropertyValue));
        }

        [TestMethod(DisplayName = "Verify that the partial prefix maps to a substring match on the underlying property.")]
        public void PartialPrefixMapsToSubstringMatchTest()
        {
            // Act: convert a predicate that requests a substring match through the partial prefix.
            var condition = XpathParser.ConvertToCondition(xpath: "@partialName='Sub'");

            // Assert: the underlying property is Name and the substring flag is applied.
            var property = CastProperty(condition);
            Assert.AreEqual(UIA_PropertyIds.UIA_NamePropertyId, property.propertyId);
            Assert.AreEqual(
                PropertyConditionFlags.PropertyConditionFlags_MatchSubstring,
                property.PropertyConditionFlags);
        }

        [TestMethod(DisplayName = "Verify that an unsupported property name throws NotSupportedException.")]
        public void UnsupportedPropertyThrowsTest()
        {
            // Act + Assert: an unknown property name is rejected rather than silently ignored.
            Assert.ThrowsExactly<NotSupportedException>(
                () => XpathParser.ConvertToCondition(xpath: "@NotARealProperty='x'"));
        }

        // Casts the parser result to a property condition, failing the test if the result is not one.
        private static IUIAutomationPropertyCondition CastProperty(IUIAutomationCondition condition)
        {
            Assert.IsInstanceOfType<IUIAutomationPropertyCondition>(condition);
            return (IUIAutomationPropertyCondition)condition;
        }

        // Asserts that the parser produced a property condition with the expected property id and string value.
        private static void AssertProperty(IUIAutomationCondition condition, int expectedPropertyId, string expectedValue)
        {
            var property = CastProperty(condition);
            Assert.AreEqual(expectedPropertyId, property.propertyId);
            Assert.AreEqual(expectedValue, Convert.ToString(property.PropertyValue));
        }
    }
}
