using System.Text.RegularExpressions;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Parses the driver's supported terminal numeric predicate and validates it against an exact UIA result set.
    /// </summary>
    /// <remarks>
    /// The helper is compute-only and represents an invalid supplied position with index <c>-1</c>, allowing
    /// repository callers to distinguish an absent predicate from a rejected one without accessing a COM collection.
    /// </remarks>
    internal static class XpathPosition
    {
        #region *** Constants    ***

        private static readonly Regex PositionExpression = new(
            pattern: @"\[(?<position>\d+)\]\s*$",
            options: RegexOptions.CultureInvariant
        );

        #endregion

        #region *** Methods      ***

        /// <summary>
        /// Converts a supported 1-based position into a validated zero-based collection index.
        /// </summary>
        /// <param name="pathSegment">The UIA path segment that may contain a terminal position.</param>
        /// <param name="matchCount">The number of elements matching the segment condition.</param>
        /// <returns>The presence of a position and its validated collection index.</returns>
        public static PositionSelection GetSelection(string pathSegment, int matchCount)
        {
            // Treat an absent segment as a missing optional predicate so callers retain their first-match path.
            if (string.IsNullOrEmpty(pathSegment))
            {
                return new PositionSelection(HasPosition: false, Index: -1);
            }

            // Match only a terminal numeric predicate so digits inside property values cannot affect selection.
            var match = PositionExpression.Match(input: pathSegment);
            if (!match.Success)
            {
                return new PositionSelection(HasPosition: false, Index: -1);
            }

            // Convert the 1-based XPath value and validate it before exposing a zero-based collection index.
            var value = match.Groups["position"].Value;
            var isParsedPosition = int.TryParse(s: value, result: out var position);
            var index = position - 1;
            var isInRange = isParsedPosition && position > 0 && index < matchCount;

            // Preserve predicate presence even when invalid so callers reject it instead of using FindFirst.
            return new PositionSelection(HasPosition: true, Index: isInRange ? index : -1);
        }

        #endregion

        #region *** Nested Types ***

        /// <summary>
        /// Represents predicate presence separately from its validated zero-based collection index.
        /// </summary>
        /// <param name="HasPosition">Indicates whether a terminal numeric predicate was supplied.</param>
        /// <param name="Index">The validated zero-based index, or <c>-1</c> when invalid.</param>
        internal readonly record struct PositionSelection(bool HasPosition, int Index);

        #endregion
    }
}
