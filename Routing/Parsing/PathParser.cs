using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Messerli.Routing.Parsing.PathParsing;

namespace Messerli.Routing.Parsing
{
    internal sealed class PathParser : IPathParser
    {
        /// <summary>
        /// ABNF spec from <a href="https://tools.ietf.org/html/rfc3986">RFC3986</a>:
        /// <code>
        /// segment = *pchar
        /// pchar = unreserved / pct-encoded / sub-delims / ":" / "@"
        /// unreserved  = ALPHA / DIGIT / "-" / "." / "_" / "~"
        /// pct-encoded = "%" HEXDIG HEXDIG
        /// sub-delims  = "!" / "$" / "&amp;" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
        /// </code>
        /// </summary>
        private static IEnumerable<char> ValidCharacters => new[]
        {
            '-',
            '.',
            '_',
            '~',
            '%',
            '!',
            '$',
            '&',
            '\'',
            '(',
            ')',
            '*',
            '+',
            ',',
            ';',
            '=',
            ':',
            '@',
        };

        public IEnumerable<string>? Parse(string path)
        {
            var segments = ParseSegments(path);
            if (segments is null)
            {
                return null;
            }

            return segments.All(segment => segment is { })
                ? segments.Select(segment => segment!)
                : null;
        }

        private static ICollection<string>? ParseSegments(string path)
        {
            var segments = SplitSegments(TrimTrailingSegmentDelimiter(path));
            if (segments is null)
            {
                return null;
            }

            return AreSegmentsValid(segments)
                ? segments.Prepend("/").ToList()
                : null;
        }

        #pragma warning disable SA1003
        private static string TrimTrailingSegmentDelimiter(string path) =>
            path.Length > 1 && path.EndsWith(SegmentDelimiterToken)
                ? path[..^1]
                : path;
        #pragma warning restore SA1003

        private static bool AreSegmentsValid(IEnumerable<string> segments) =>
            segments.All(IsValidSegment);

        private static bool IsValidSegment(string segment) =>
            segment.All(character => char.IsLetterOrDigit(character) || ValidCharacters.Contains(character));
    }
}
