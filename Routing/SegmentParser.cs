using System.Collections.Generic;
using System.Linq;
using Routing.SegmentVariant;

namespace Routing
{
    internal class SegmentParser : ISegmentParser
    {

        private const char ParameterBeginToken = '{';
        private const char ParameterEndToken = '}';
        private const char SegmentDelimiterToken = '/';

        public IEnumerable<ISegmentVariant>? Parse(string route)
        {
            if (!IsRouteValid(route))
            {
                return null;
            }

            var trimmedRoute = route.Substring(1);
            var segments = ParseSegments(trimmedRoute);

            return segments.All(segment => segment is { })
                ? segments.Select(segment => segment!)
                : null;
        }

        private static List<ISegmentVariant?> ParseSegments(string route)
        {
            return route
                .Split(SegmentDelimiterToken)
                .Select(ParseSegment)
                .Prepend(new Root())
                .ToList();
        }

        private static bool IsRouteValid(string route)
        {
            bool IsEmpty() => string.IsNullOrEmpty(route);
            bool StartsWithRoot() => route.First() == SegmentDelimiterToken;

            return !IsEmpty() && StartsWithRoot();
        }

        private static ISegmentVariant? ParseSegment(string segment)
        {
            return IsParameter(segment)
                ? ParseParameter(segment)
                : ParseLiteral(segment);
        }
        private static ISegmentVariant? ParseLiteral(string segment)
        {
            return IsValidSpecifier(segment)
                ? new Literal(segment)
                : null;
        }

        private static ISegmentVariant? ParseParameter(string segment)
        {
            const int keyDelimiterTokenCount = 2;
            var keyLength = segment.Length - keyDelimiterTokenCount;
            var parameterKey = segment.Substring(startIndex: 1, length: keyLength);

            return IsValidSpecifier(parameterKey)
                ? new Parameter(parameterKey)
                : null;
        }

        private static bool IsParameter(string segment)
        {
            bool StartsParameter() => segment.StartsWith(ParameterBeginToken.ToString());
            bool EndsParameter() => segment.EndsWith(ParameterEndToken.ToString());

            return StartsParameter() && EndsParameter();
        }

        private static bool IsValidSpecifier(string identifier)
        {
            bool IsEmpty() => string.IsNullOrEmpty(identifier);
            bool IsValidCharacter(char character) => char.IsLetterOrDigit(character) || ValidSeparators.Contains(character);
            bool ContainsValidCharacters() => identifier.All(IsValidCharacter);

            return !IsEmpty() && ContainsValidCharacters();
        }

        private static IEnumerable<char> ValidSeparators => new[]
        {
            '-',
            '_',
            '.',
        };
    }
}
