using System;
using System.Collections.Generic;
using Messerli.Routing.Parsing;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    internal static class ParsingUtility
    {
        public static IEnumerable<ISegmentVariant> ParseRoute(ISegmentParser segmentParser, string route) =>
            segmentParser.Parse(route)
            ?? throw new ArgumentException($"Invalid route: {route}", nameof(route));
    }
}
