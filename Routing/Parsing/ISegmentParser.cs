using System.Collections.Generic;
using Routing.SegmentVariant;

namespace Routing.Parsing
{
    public interface ISegmentParser
    {
        IEnumerable<ISegmentVariant>? Parse(string route);
    }
}
