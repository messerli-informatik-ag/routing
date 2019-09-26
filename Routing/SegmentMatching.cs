using System;
using Routing.SegmentVariant;

namespace Routing
{
    internal static class SegmentMatching
    {
        internal static bool SegmentMatchesIdentifier(ISegmentVariant segment, string identifier) =>
            segment switch
            {
                Literal { Identifier: var path } => path == identifier,
                Parameter _ => true,
                Root _ => false,
                _ => throw new InvalidOperationException($"Type {segment.GetType()} is not handled")
            };
    }
}
