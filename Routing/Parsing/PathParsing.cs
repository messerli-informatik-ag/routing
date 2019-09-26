using System.Collections.Generic;
using System.Linq;

namespace Routing.Parsing
{
    internal static class PathParsing
    {
        public const char SegmentDelimiterToken = '/';

        internal static ICollection<string>? SplitSegments(string path)
        {
            if (!IsPathBroadlyValid(path))
            {
                return null;
            }

            const int lengthOfRoot = 1;
            var trimmedPath = path.Substring(lengthOfRoot);
            return trimmedPath.Split(SegmentDelimiterToken);
        }
        private static bool IsPathBroadlyValid(string path)
        {
            bool IsEmpty() => string.IsNullOrEmpty(path);
            bool StartsWithRoot() => path.First() == SegmentDelimiterToken;

            return !IsEmpty() && StartsWithRoot();
        }
    }
}
