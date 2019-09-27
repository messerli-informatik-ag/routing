using System;
using System.Collections.Generic;
using System.Linq;
using Routing.SegmentVariant;
using static Routing.Parsing.PathParsing;

namespace Routing.Parsing
{
    internal class SegmentParser : ISegmentParser
    {
        private const char ParameterBeginToken = '{';
        private const char ParameterEndToken = '}';

        private static IEnumerable<char> ValidSeparators => new[]
        {
            '-',
            '_',
            '.',
        };

        public IEnumerable<ISegmentVariant>? Parse(string route)
        {
            var segments = ParseSegments(route);
            if (segments is null)
            {
                return null;
            }

            if (segments.All(segment => segment is { }))
            {
                var nonNullSegments = (ICollection<ISegmentVariant>) segments;
                CheckForDuplicatedParameters(nonNullSegments);
                return nonNullSegments;
            }

            return null;
        }

        private static ICollection<ISegmentVariant?>? ParseSegments(string route)
        {
            return SplitSegments(route)?
                .Select(ParseSegment)
                .Prepend(new Root())
                .ToList();
        }

        private static void CheckForDuplicatedParameters(IEnumerable<ISegmentVariant> segments)
        {
            var duplicatedParameters = FindDuplicatedParameters(segments).ToList();
            if (duplicatedParameters.Any())
            {
                throw new ArgumentException($"Duplicated parameters are not allowed in route: {string.Join(", ", duplicatedParameters)}");
            }
        }

        private static IEnumerable<string> FindDuplicatedParameters(IEnumerable<ISegmentVariant> segments)
        {
            return segments
                .OfType<Parameter>()
                .GroupBy(param => param.Key)
                .Where(group => group.Count() > 1)
                .Select(group => group.First().Key);
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
            bool ContainsValidCharacters() => identifier.All(IsValidCharacter);
            bool ContainsOnlySeparators() => identifier.All(IsValidSeparator);

            return !IsEmpty() && ContainsValidCharacters() && !ContainsOnlySeparators();
        }

        private static bool IsValidCharacter(char character) => char.IsLetterOrDigit(character) || IsValidSeparator(character);

        private static bool IsValidSeparator(char separator) => ValidSeparators.Contains(separator);
    }
}
