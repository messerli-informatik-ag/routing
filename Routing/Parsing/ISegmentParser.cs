using System.Collections.Generic;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing.Parsing
{
    internal interface ISegmentParser
    {
        IEnumerable<ISegmentVariant>? Parse(string route);
    }
}
