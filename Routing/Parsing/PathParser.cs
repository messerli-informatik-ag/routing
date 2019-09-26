using System.Collections.Generic;
using System.Linq;
using static Routing.Parsing.PathParsing;

namespace Routing.Parsing
{
    internal class PathParser : IPathParser
    {
        /// <summary>
        /// ABNF spec from <a href="https://tools.ietf.org/html/rfc3986">RFC3986</a>:
        /// <code>
        /// segment = *pchar
        /// pchar = unreserved / pct-encoded / sub-delims / ":" / "@"
        /// unreserved  = ALPHA / DIGIT / "-" / "." / "_" / "~"
        /// pct-encoded = "%" HEXDIG HEXDIG
        /// sub-delims  = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
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
            var segments = SplitSegments(path);
            if (segments is null)
            {
                return null;
            }

            return AreSegmentsValid(segments)
                ? segments.Prepend("/").ToList()
                : null;
        }

        private static bool AreSegmentsValid(IEnumerable<string> segments) =>
            segments.All(IsValidSegment);

        private static bool IsValidSegment(string segment) =>
            segment.All(character => char.IsLetterOrDigit(character) || ValidCharacters.Contains(character));
    }
}
