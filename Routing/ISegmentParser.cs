using System.Collections.Generic;
using Routing.SegmentVariant;

namespace Routing
{
    internal interface ISegmentParser
    {
        IEnumerable<ISegmentVariant>? Parse(string route);
    }
}
