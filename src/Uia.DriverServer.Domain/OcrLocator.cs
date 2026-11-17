/*
 * CHANGE LOG - keep only last 5 threads
 *
 * RESOURCES
 *
 * NOTES
 * This file is mirrored verbatim by the OCR Inspector (https://github.com/g4-api/ocr-inspector) so both tools
 * order, index, and match OCR words identically. Apply every change to both repositories.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Parses OCR locators and matches them against OCR words without depending on an OCR engine.
    /// </summary>
    /// <remarks>
    /// The class is compute-only and holds no state, so the driver and the OCR Inspector share the exact ordering,
    /// occurrence indexing, and matching rules. An OCR locator has the form <c>ocr(text)</c>, where the text is
    /// optionally quoted, accepts <c>|</c> separated alternatives, and accepts a 1-based occurrence suffix such as
    /// <c>ocr('Submit')[2]</c>.
    /// </remarks>
    public static class OcrLocator
    {
        #region *** Constants    ***

        // Recognizes a complete OCR locator, capturing the payload up to the last closing parenthesis so text that
        // contains parentheses or slashes stays intact, and capturing the optional 1-based occurrence index.
        private static readonly Regex LocatorExpression = new(
            pattern: @"^ocr\((?<payload>.*)\)(?:\[(?<index>\d+)\])?$",
            options: RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline,
            matchTimeout: TimeSpan.FromSeconds(30)
        );

        #endregion

        #region *** Methods      ***

        /// <summary>
        /// Finds every OCR word or phrase that satisfies the supplied OCR locator.
        /// </summary>
        /// <remarks>
        /// Matching compares whole words. An alternative with several words matches adjacent words on one line and
        /// reports their union rectangle. Exact-case matches win; a case-insensitive pass runs only when the exact
        /// pass finds nothing. Matches follow reading order, and an occurrence index selects exactly one of them.
        /// </remarks>
        /// <param name="words">The OCR words to search; the collection is not mutated.</param>
        /// <param name="locator">The OCR locator, such as <c>ocr('Save'|'Save As')[2]</c>.</param>
        /// <returns>The matches in reading order; empty when the locator is invalid or nothing matches.</returns>
        public static List<OcrWordModel> FindWords(IEnumerable<OcrWordModel> words, string locator)
        {
            // Parse the locator first so an invalid expression yields no matches before any layout work.
            var locatorModel = ResolveLocator(locator);
            var alternatives = locatorModel.Alternatives
                .Select(GetTokens)
                .Where(tokens => tokens.Length > 0)
                .ToList();

            if (alternatives.Count == 0)
            {
                return [];
            }

            // Rebuild the line layout so phrase alternatives only join words that share one visual line.
            var lines = GetLines(words: words ?? []);

            // Prefer exact-case matches and fall back to case-insensitive matches only when none exist.
            var matches = FindMatches(lines, alternatives, comparison: StringComparison.Ordinal);
            if (matches.Count == 0)
            {
                matches = FindMatches(lines, alternatives, comparison: StringComparison.OrdinalIgnoreCase);
            }

            // Publish the occurrence and locator of every match so callers can address each one individually.
            for (var index = 0; index < matches.Count; index++)
            {
                var occurrence = matches.Count > 1 ? index + 1 : 0;
                matches[index].Index = index + 1;
                matches[index].Locator = NewLocator(text: matches[index].Text, index: occurrence);
            }

            if (!locatorModel.HasIndex)
            {
                return matches;
            }

            // Apply the 1-based occurrence; zero and out-of-range values select nothing instead of the first match.
            var isIndexValid = locatorModel.Index >= 1 && locatorModel.Index <= matches.Count;

            return isIndexValid ? [matches[locatorModel.Index - 1]] : [];
        }

        /// <summary>
        /// Removes the surrounding whitespace and the XPath-shaped leading slashes from a locator value.
        /// </summary>
        /// <remarks>
        /// Parsing and classification both read the value through this method, so they always agree on whether a
        /// value such as <c>//ocr(text)</c> is an OCR locator. A <c>null</c> value yields an empty string.
        /// </remarks>
        /// <param name="locator">The locator value to normalize.</param>
        /// <returns>The locator value starting at its first meaningful character.</returns>
        public static string FormatLocator(string locator)
        {
            var value = locator ?? string.Empty;

            return value
                .Trim()
                .TrimStart('/')
                .Trim();
        }

        /// <summary>
        /// Normalizes raw OCR words into the ordered, indexed word list shared by the driver and the OCR Inspector.
        /// </summary>
        /// <remarks>
        /// The method trims each text, drops empty words, orders the words by visual line and then left to right,
        /// and assigns each word its occurrence index and ready-to-use locator. The input models are not mutated.
        /// </remarks>
        /// <param name="words">The raw OCR words in any order.</param>
        /// <returns>A new list of normalized words in reading order.</returns>
        public static List<OcrWordModel> FormatWords(IEnumerable<OcrWordModel> words)
        {
            // Copy and trim the usable words so the caller-owned models stay untouched.
            var usableWords = (words ?? [])
                .Where(word => word?.Rectangle != null && !string.IsNullOrWhiteSpace(word.Text))
                .Select(word => new OcrWordModel
                {
                    Confidence = word.Confidence,
                    Rectangle = NewRectangle(word.Rectangle),
                    Text = word.Text.Trim()
                });

            // Flatten the visual lines into the reading order that defines every occurrence index.
            var orderedWords = GetLines(words: usableWords)
                .SelectMany(line => line)
                .ToList();

            // Count identical texts first so the locator carries an index only when the text is ambiguous.
            var totals = orderedWords
                .GroupBy(word => word.Text, comparer: StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), comparer: StringComparer.Ordinal);
            var occurrences = new Dictionary<string, int>(comparer: StringComparer.Ordinal);

            // Assign the running occurrence and its locator in reading order.
            foreach (var word in orderedWords)
            {
                occurrences.TryGetValue(key: word.Text, value: out var occurrence);
                occurrences[word.Text] = occurrence + 1;

                word.Index = occurrence + 1;
                word.Locator = NewLocator(text: word.Text, index: totals[word.Text] > 1 ? word.Index : 0);
            }

            return orderedWords;
        }

        /// <summary>
        /// Creates the OCR locator that addresses the supplied text and occurrence.
        /// </summary>
        /// <param name="text">The text to address.</param>
        /// <param name="index">The 1-based occurrence, or zero to omit the occurrence suffix.</param>
        /// <returns>An OCR locator such as <c>ocr('Submit')</c> or <c>ocr('Submit')[2]</c>.</returns>
        public static string NewLocator(string text, int index)
        {
            // Quote with the character the text does not contain; text holding both quote kinds stays unquoted.
            var hasSingleQuote = text.Contains(value: '\'');
            var hasDoubleQuote = text.Contains(value: '"');
            var quote = "'";

            if (hasSingleQuote)
            {
                quote = hasDoubleQuote ? string.Empty : "\"";
            }

            // Append the occurrence suffix only for an explicit positive index.
            var suffix = index > 0 ? $"[{index}]" : string.Empty;

            return $"ocr({quote}{text}{quote}){suffix}";
        }

        /// <summary>
        /// Parses an OCR locator into its text alternatives and optional occurrence index.
        /// </summary>
        /// <remarks>
        /// Leading slashes are ignored so XPath-shaped values such as <c>//ocr(text)</c> parse. A value that is not
        /// an OCR locator returns a model with no alternatives instead of throwing.
        /// </remarks>
        /// <param name="locator">The locator value to parse.</param>
        /// <returns>The parsed locator, whose alternatives are empty when the value is invalid.</returns>
        public static LocatorModel ResolveLocator(string locator)
        {
            // Normalize the XPath-shaped prefix before matching the complete OCR expression.
            var match = LocatorExpression.Match(input: FormatLocator(locator));

            if (!match.Success)
            {
                return new LocatorModel();
            }

            // Read the occurrence; a value too large for an integer can never be in range, so it selects nothing.
            var hasIndex = match.Groups["index"].Success;
            var isIndex = int.TryParse(s: match.Groups["index"].Value, result: out var index);

            return new LocatorModel
            {
                Alternatives = GetAlternatives(payload: match.Groups["payload"].Value),
                HasIndex = hasIndex,
                Index = hasIndex && isIndex ? index : 0
            };
        }

        /// <summary>
        /// Tests whether the supplied value is a complete OCR locator.
        /// </summary>
        /// <param name="locator">The locator value to test.</param>
        /// <returns><c>true</c> when the whole value is an OCR locator; otherwise, <c>false</c>.</returns>
        public static bool TestLocator(string locator)
        {
            // Apply the same prefix normalization as parsing so both operations agree on every value.
            return LocatorExpression.IsMatch(input: FormatLocator(locator));
        }

        // Collects every phrase match across the supplied lines in reading order using one string comparison.
        // At each word position the first alternative that matches wins, so overlapping alternatives report the
        // position once. The helper allocates new result models and never mutates the layout it reads.
        private static List<OcrWordModel> FindMatches(
            List<List<OcrWordModel>> lines,
            List<string[]> alternatives,
            StringComparison comparison)
        {
            var matches = new List<OcrWordModel>();

            // Scan every start position of every line so phrases never cross a line boundary.
            foreach (var line in lines)
            {
                for (var start = 0; start < line.Count; start++)
                {
                    var words = line.Skip(count: start);
                    var tokens = alternatives.Find(alternative => TestPhrase(words, tokens: alternative, comparison));
                    if (tokens == null)
                    {
                        continue;
                    }

                    // Merge the matched words into one result that spans the complete phrase.
                    matches.Add(item: NewPhrase(words: line.GetRange(index: start, count: tokens.Length)));
                }
            }

            return matches;
        }

        // Splits a locator payload into trimmed alternatives on pipes that sit outside a quoted alternative.
        // A quote opens only at the start of an alternative, so apostrophes inside unquoted text stay literal.
        // Matching outer quotes are removed from each alternative and empty alternatives are dropped.
        private static string[] GetAlternatives(string payload)
        {
            var alternatives = new List<string>();
            var current = new StringBuilder();
            var quote = '\0';

            // Walk the payload once, tracking whether the current alternative is inside its opening quote.
            foreach (var character in payload)
            {
                var isQuoteCharacter = character == '\'' || character == '"';
                var isOpening = quote == '\0' && isQuoteCharacter && current.ToString().Trim().Length == 0;
                var isClosing = quote != '\0' && character == quote;
                var isSeparator = quote == '\0' && character == '|';

                // Close the alternative at an unquoted pipe and start collecting the next one.
                if (isSeparator)
                {
                    alternatives.Add(item: current.ToString());
                    current.Clear();
                    continue;
                }

                if (isOpening)
                {
                    quote = character;
                }
                else if (isClosing)
                {
                    quote = '\0';
                }

                current.Append(value: character);
            }

            alternatives.Add(item: current.ToString());

            // Remove the optional outer quotes after splitting so quoted and unquoted forms compare equally.
            return [.. alternatives
                .Select(alternative => RemoveQuotes(value: alternative.Trim()).Trim())
                .Where(alternative => alternative.Length > 0)];
        }

        // Groups words into visual lines and orders them top to bottom, then left to right inside each line.
        // A word joins the first line whose anchor word it overlaps vertically by at least half of the smaller
        // height; otherwise it starts a new line. The helper returns new lists and never mutates the words.
        private static List<List<OcrWordModel>> GetLines(IEnumerable<OcrWordModel> words)
        {
            var lines = new List<List<OcrWordModel>>();
            var orderedWords = words
                .Where(word => word?.Rectangle != null)
                .OrderBy(word => word.Rectangle.Top)
                .ThenBy(word => word.Rectangle.Left);

            // Assign each word to its line; the first word of a line stays the anchor for later comparisons.
            foreach (var word in orderedWords)
            {
                var rectangle = word.Rectangle;
                var line = lines.Find(candidate => TestSameLine(anchor: candidate[0].Rectangle, rectangle));
                if (line == null)
                {
                    lines.Add(item: [word]);
                    continue;
                }

                line.Add(item: word);
            }

            // Order each line left to right so phrase matching and occurrence indexing follow reading order.
            return [.. lines.Select(line => line.OrderBy(word => word.Rectangle.Left).ToList())];
        }

        // Splits one alternative into the whitespace-separated words that phrase matching compares one by one.
        private static string[] GetTokens(string alternative)
        {
            return alternative.Split(separator: (char[])null, options: StringSplitOptions.RemoveEmptyEntries);
        }

        // Creates one result model from the adjacent words of a matched phrase.
        // The result reports the joined text, the union rectangle, and the lowest confidence among the words.
        private static OcrWordModel NewPhrase(List<OcrWordModel> words)
        {
            return new OcrWordModel
            {
                Confidence = words.Min(word => word.Confidence),
                Rectangle = new RectangleModel
                {
                    Bottom = words.Max(word => word.Rectangle.Bottom),
                    Left = words.Min(word => word.Rectangle.Left),
                    Right = words.Max(word => word.Rectangle.Right),
                    Top = words.Min(word => word.Rectangle.Top)
                },
                Text = string.Join(separator: ' ', values: words.Select(word => word.Text))
            };
        }

        // Copies a rectangle so normalized words never share mutable state with caller-owned models.
        private static RectangleModel NewRectangle(RectangleModel rectangle)
        {
            return new RectangleModel
            {
                Bottom = rectangle.Bottom,
                Left = rectangle.Left,
                Right = rectangle.Right,
                Top = rectangle.Top
            };
        }

        // Removes one pair of matching outer quotes from an alternative; any other value is returned unchanged.
        private static string RemoveQuotes(string value)
        {
            var isLongEnough = value.Length >= 2;
            var isQuoted = isLongEnough && (value[0] == '\'' || value[0] == '"') && value[^1] == value[0];

            return isQuoted ? value[1..^1] : value;
        }

        // Tests whether the tokens equal the leading words of the supplied sequence, one word per token.
        // The sequence is the remainder of one line, so a phrase never continues onto the next line.
        private static bool TestPhrase(IEnumerable<OcrWordModel> words, string[] tokens, StringComparison comparison)
        {
            // Take only as many words as the phrase needs; fewer words mean the phrase runs past the line end.
            var texts = words
                .Take(count: tokens.Length)
                .Select(word => word.Text)
                .ToArray();

            if (texts.Length != tokens.Length)
            {
                return false;
            }

            // Compare word by word so every token must equal one complete recognized word.
            for (var index = 0; index < tokens.Length; index++)
            {
                if (!string.Equals(a: texts[index], b: tokens[index], comparisonType: comparison))
                {
                    return false;
                }
            }

            return true;
        }

        // Tests whether a rectangle shares a visual line with a line's anchor rectangle.
        // The rectangles share a line when their vertical overlap covers at least half of the smaller height.
        private static bool TestSameLine(RectangleModel anchor, RectangleModel rectangle)
        {
            var overlap = Math.Min(anchor.Bottom, rectangle.Bottom) - Math.Max(anchor.Top, rectangle.Top);
            var anchorHeight = Math.Max(1, anchor.Bottom - anchor.Top);
            var rectangleHeight = Math.Max(1, rectangle.Bottom - rectangle.Top);

            return overlap * 2 >= Math.Min(anchorHeight, rectangleHeight);
        }

        #endregion

        #region *** Nested Types ***

        /// <summary>
        /// Represents a parsed OCR locator.
        /// </summary>
        public class LocatorModel
        {
            /// <summary>
            /// Gets or sets the text alternatives without their optional quotes; empty when the locator is invalid.
            /// </summary>
            public string[] Alternatives { get; set; } = [];

            /// <summary>
            /// Gets or sets a value indicating whether the locator carries an occurrence suffix.
            /// </summary>
            public bool HasIndex { get; set; }

            /// <summary>
            /// Gets or sets the 1-based occurrence requested by the locator.
            /// </summary>
            /// <remarks>Zero when no suffix is present, or when the suffix cannot select any occurrence.</remarks>
            public int Index { get; set; }
        }

        #endregion
    }
}
