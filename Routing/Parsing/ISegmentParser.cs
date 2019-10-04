using System.Collections.Generic;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing.Parsing
{
    public interface ISegmentParser
    {
        IEnumerable<ISegmentVariant>? Parse(string route);
    }
}
