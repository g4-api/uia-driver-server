using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain.UnitTests
{
    [TestClass]
    [TestCategory(nameof(OcrLocator))]
    [TestCategory("UnitTest")]
    public sealed class OcrLocatorTests
    {
        [TestMethod(DisplayName = "Verify that pipe-separated alternatives match any of their texts in reading order.")]
        public void FindWordsAlternativesTest()
        {
            // Arrange: recognize a screen where only the fallback spellings of the requested text exist.
            var words = NewScreen();

            // Act: request a text that is absent together with two fallbacks that are present.
            var matches = OcrLocator.FindWords(words, locator: "ocr('Missing'|'Zebra'|'Submit')");

            // Assert: both fallbacks are reported, ordered by their position on the screen.
            CollectionAssert.AreEqual(
                expected: new[] { "Zebra", "Submit" },
                actual: matches.Select(match => match.Text).ToArray());
        }

        [TestMethod(DisplayName = "Verify that matching ignores case only when no exact-case match exists.")]
        public void FindWordsCaseFallbackTest()
        {
            // Arrange: recognize a screen holding "Save" twice and "SAVE" once.
            var words = NewScreen();

            // Act: request one spelling that exists exactly and one that exists only in another case.
            var exactMatches = OcrLocator.FindWords(words, locator: "ocr(SAVE)");
            var fallbackMatches = OcrLocator.FindWords(words, locator: "ocr(zebra)");

            // Assert: the exact spelling excludes other casings, and the fallback finds the differently cased word.
            Assert.HasCount(expected: 1, collection: exactMatches);
            Assert.AreEqual(expected: "SAVE", actual: exactMatches[0].Text);
            Assert.HasCount(expected: 1, collection: fallbackMatches);
            Assert.AreEqual(expected: "Zebra", actual: fallbackMatches[0].Text);
        }

        [TestMethod(DisplayName = "Verify that an occurrence index selects exactly one match " +
            "and rejects invalid positions.")]
        #region *** Data Set ***
        [DataRow("ocr('Save')[1]", 300)]
        [DataRow("ocr('Save')[2]", 10)]
        [DataRow("ocr(Save)[2]", 10)]
        [DataRow("ocr('Save')[0]", -1)]
        [DataRow("ocr('Save')[3]", -1)]
        [DataRow("ocr('Save')[99999999999999999999]", -1)]
        #endregion
        public void FindWordsIndexTest(string locator, int expectedLeft)
        {
            // Arrange: recognize a screen holding the exact text "Save" at two different horizontal positions.
            var words = NewScreen();

            // Act: resolve the indexed locator against the screen.
            var matches = OcrLocator.FindWords(words, locator);

            // Assert: a valid index yields the occurrence at the expected position; an invalid index yields nothing.
            var actualLeft = matches.Count == 1 ? matches[0].Rectangle.Left : -1;
            Assert.AreEqual(expected: expectedLeft, actual: actualLeft);
        }

        [TestMethod(DisplayName = "Verify that a locator matching no text returns no words.")]
        #region *** Data Set ***
        [DataRow("ocr(NoSuchWord)")]
        [DataRow("ocr('Hello Zebra')")]
        [DataRow("ocr()")]
        [DataRow("ocr('')")]
        [DataRow("//Button[@Name='Save']")]
        [DataRow("")]
        [DataRow(null)]
        #endregion
        public void FindWordsNoMatchTest(string locator)
        {
            // Arrange: recognize a screen that does not contain the requested text on a single line.
            var words = NewScreen();

            // Act: resolve the locator against the screen.
            var matches = OcrLocator.FindWords(words, locator);

            // Assert: no word is reported, which the driver publishes as a not-found result.
            Assert.IsEmpty(collection: matches);
        }

        [TestMethod(DisplayName = "Verify that a multi-word phrase matches adjacent words on one line " +
            "and reports their union.")]
        public void FindWordsPhraseTest()
        {
            // Arrange: recognize a screen whose first line reads "Hello from UiaDriver Server".
            var words = NewScreen();

            // Act: request the complete phrase using the documented quoted form.
            var matches = OcrLocator.FindWords(words, locator: "ocr('Hello from UiaDriver Server'|'Hello from Uia')");

            // Assert: one match spans the first word's left edge to the last word's right edge.
            Assert.HasCount(expected: 1, collection: matches);
            Assert.AreEqual(expected: "Hello from UiaDriver Server", actual: matches[0].Text);
            Assert.AreEqual(expected: 10, actual: matches[0].Rectangle.Left);
            Assert.AreEqual(expected: 470, actual: matches[0].Rectangle.Right);
            Assert.AreEqual(expected: 89.2, actual: matches[0].Confidence);
        }

        [TestMethod(DisplayName = "Verify that quoted and unquoted forms of one text resolve to the same word.")]
        #region *** Data Set ***
        [DataRow("ocr(Submit)")]
        [DataRow("ocr('Submit')")]
        [DataRow("ocr(\"Submit\")")]
        [DataRow("ocr( 'Submit' )")]
        [DataRow("//ocr('Submit')")]
        [DataRow("OCR('Submit')")]
        #endregion
        public void FindWordsQuotedFormsTest(string locator)
        {
            // Arrange: recognize a screen holding the text "Submit" once.
            var words = NewScreen();

            // Act: resolve one of the accepted spellings of the locator.
            var matches = OcrLocator.FindWords(words, locator);

            // Assert: every accepted spelling resolves to the single occurrence and reports its canonical locator.
            Assert.HasCount(expected: 1, collection: matches);
            Assert.AreEqual(expected: "Submit", actual: matches[0].Text);
            Assert.AreEqual(expected: "ocr('Submit')", actual: matches[0].Locator);
        }

        [TestMethod(DisplayName = "Verify that text containing slashes, parentheses, and pipes is matched literally.")]
        #region *** Data Set ***
        [DataRow("ocr('and/or')", "and/or")]
        [DataRow("ocr(and/or)", "and/or")]
        [DataRow("ocr('(Ctrl+S)')", "(Ctrl+S)")]
        [DataRow("ocr('a|b')", "a|b")]
        #endregion
        public void FindWordsSpecialCharactersTest(string locator, string expected)
        {
            // Arrange: recognize a screen holding words built from locator syntax characters.
            var words = NewScreen();

            // Act: resolve the locator whose text contains those characters.
            var matches = OcrLocator.FindWords(words, locator);

            // Assert: the complete text is matched instead of being split at the syntax characters.
            Assert.HasCount(expected: 1, collection: matches);
            Assert.AreEqual(expected, actual: matches[0].Text);
        }

        [TestMethod(DisplayName = "Verify that every formatted word resolves back to itself through its own locator.")]
        public void FormatWordsLocatorRoundTripTest()
        {
            // Arrange: recognize a screen holding unique, repeated, and special-character words.
            var words = NewScreen();

            foreach (var word in words)
            {
                // Act: resolve the locator the inspector displays for the word.
                var matches = OcrLocator.FindWords(words, locator: word.Locator);

                // Assert: the locator addresses exactly the word it was generated for.
                Assert.HasCount(expected: 1, collection: matches, message: word.Locator);
                Assert.AreEqual(expected: word.Rectangle.Left, actual: matches[0].Rectangle.Left, message: word.Locator);
                Assert.AreEqual(expected: word.Rectangle.Top, actual: matches[0].Rectangle.Top, message: word.Locator);
            }
        }

        [TestMethod(DisplayName = "Verify that formatting indexes repeated texts " +
            "and adds the index to their locators only.")]
        public void FormatWordsOccurrenceTest()
        {
            // Arrange: recognize a screen holding "Save" twice and "Submit" once.
            var words = NewScreen();

            // Act: collect the locators of the repeated and the unique text.
            var saveLocators = words.Where(word => word.Text == "Save").Select(word => word.Locator).ToArray();
            var submit = words.Single(word => word.Text == "Submit");

            // Assert: repeated texts carry their occurrence, and a unique text carries no index.
            CollectionAssert.AreEqual(expected: new[] { "ocr('Save')[1]", "ocr('Save')[2]" }, actual: saveLocators);
            Assert.AreEqual(expected: "ocr('Submit')", actual: submit.Locator);
            Assert.AreEqual(expected: 1, actual: submit.Index);
        }

        [TestMethod(DisplayName = "Verify that formatting trims texts, drops empty words, " +
            "and orders words by line and position.")]
        public void FormatWordsReadingOrderTest()
        {
            // Arrange: supply raw words out of order, one padded with whitespace and one empty.
            var rawWords = new[]
            {
                NewWord(text: "second", left: 200, top: 12),
                NewWord(text: "   ", left: 500, top: 10),
                NewWord(text: "third", left: 10, top: 60),
                NewWord(text: " first ", left: 10, top: 10)
            };

            // Act: normalize the raw words into the shared reading order.
            var words = OcrLocator.FormatWords(words: rawWords);

            // Assert: the empty word is gone, texts are trimmed, and slightly offset words stay on one line.
            CollectionAssert.AreEqual(
                expected: new[] { "first", "second", "third" },
                actual: words.Select(word => word.Text).ToArray());

            // Assert: the caller-owned models keep their original text.
            Assert.AreEqual(expected: " first ", actual: rawWords[3].Text);
        }

        [TestMethod(DisplayName = "Verify that a new locator quotes its text with a character the text does not contain.")]
        #region *** Data Set ***
        [DataRow("Submit", 0, "ocr('Submit')")]
        [DataRow("Submit", 2, "ocr('Submit')[2]")]
        [DataRow("don't", 0, "ocr(\"don't\")")]
        [DataRow("say \"don't\"", 0, "ocr(say \"don't\")")]
        #endregion
        public void NewLocatorTest(string text, int index, string expected)
        {
            // Act: create the locator for the text and occurrence.
            var locator = OcrLocator.NewLocator(text, index);

            // Assert: the locator uses a safe quote and carries the suffix only for a positive index.
            Assert.AreEqual(expected, actual: locator);
        }

        [TestMethod(DisplayName = "Verify that a locator is parsed into its alternatives and occurrence index.")]
        public void ResolveLocatorTest()
        {
            // Arrange: combine quoted, unquoted, and pipe-carrying alternatives with an occurrence suffix.
            const string locator = "ocr('Save As' | Save | \"a|b\" | 'it's')[3]";

            // Act: parse the locator.
            var model = OcrLocator.ResolveLocator(locator);

            // Assert: quotes are removed, whitespace is trimmed, and a quoted pipe stays inside its alternative.
            CollectionAssert.AreEqual(expected: new[] { "Save As", "Save", "a|b", "it's" }, actual: model.Alternatives);
            Assert.IsTrue(condition: model.HasIndex);
            Assert.AreEqual(expected: 3, actual: model.Index);
        }

        [TestMethod(DisplayName = "Verify that only a complete OCR expression is recognized as an OCR locator.")]
        #region *** Data Set ***
        [DataRow("ocr(Submit)", true)]
        [DataRow("ocr('and/or')", true)]
        [DataRow("//ocr('Submit')[2]", true)]
        [DataRow(" Ocr( 'Submit' ) ", true)]
        [DataRow("ocr()", true)]
        [DataRow("//Button[@Name='ocr(Submit)']", false)]
        [DataRow("/Window/ocr(Submit)", false)]
        [DataRow("Coords(10,20)", false)]
        [DataRow("ocr(Submit", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        #endregion
        public void TestLocatorTest(string locator, bool expected)
        {
            // Act: classify the locator value.
            var isLocator = OcrLocator.TestLocator(locator);

            // Assert: only a value that is entirely an OCR expression bypasses the UIA hierarchy parser.
            Assert.AreEqual(expected, actual: isLocator);
        }

        // Creates the normalized word list of a small fixed screen shared by the matching tests.
        // Line one reads "Hello from UiaDriver Server", line two "Zebra Submit Save", line three "Save SAVE",
        // and line four holds words built from locator syntax characters.
        private static List<OcrWordModel> NewScreen()
        {
            var rawWords = new[]
            {
                NewWord(text: "Hello", left: 10, top: 10, confidence: 96.8),
                NewWord(text: "from", left: 110, top: 11, confidence: 95.6),
                NewWord(text: "UiaDriver", left: 200, top: 10, confidence: 89.2),
                NewWord(text: "Server", left: 370, top: 9, confidence: 96.5),
                NewWord(text: "Zebra", left: 10, top: 60),
                NewWord(text: "Submit", left: 150, top: 60),
                NewWord(text: "Save", left: 300, top: 61),
                NewWord(text: "Save", left: 10, top: 110),
                NewWord(text: "SAVE", left: 150, top: 110),
                NewWord(text: "and/or", left: 10, top: 160),
                NewWord(text: "(Ctrl+S)", left: 150, top: 160),
                NewWord(text: "a|b", left: 300, top: 160)
            };

            return OcrLocator.FormatWords(words: rawWords);
        }

        // Creates one raw word with a fixed confidence for tests that do not assert confidence.
        private static OcrWordModel NewWord(string text, int left, int top)
        {
            return NewWord(text, left, top, confidence: 90);
        }

        // Creates one raw word occupying a 100 by 25 pixel rectangle at the supplied position.
        private static OcrWordModel NewWord(string text, int left, int top, double confidence)
        {
            return new OcrWordModel
            {
                Confidence = confidence,
                Rectangle = new RectangleModel
                {
                    Bottom = top + 25,
                    Left = left,
                    Right = left + 100,
                    Top = top
                },
                Text = text
            };
        }
    }
}
