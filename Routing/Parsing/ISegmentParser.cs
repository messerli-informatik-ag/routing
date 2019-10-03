using System.Collections.Generic;
using Routing.SegmentVariant;

namespace Routing.Parsing
{
    internal interface ISegmentParser
    {
        IEnumerable<ISegmentVariant>? Parse(string route);
    }
}
