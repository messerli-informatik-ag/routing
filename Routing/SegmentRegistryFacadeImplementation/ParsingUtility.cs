using System;
using System.Collections.Generic;
using Routing.Parsing;
using Routing.SegmentVariant;

namespace Routing.SegmentRegistryFacadeImplementation
{
    internal static class ParsingUtility
    {
        public static IEnumerable<ISegmentVariant> ParseRoute(ISegmentParser segmentParser, string route)
        {
            return segmentParser.Parse(route) ?? throw new ArgumentException($"Invalid route: {route}", nameof(route));
        }
    }
}
