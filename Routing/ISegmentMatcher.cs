using System.Collections.Generic;
using System.Net.Http;

namespace Routing
{
    internal interface ISegmentMatcher<TResponse, TRequest>
    {
        Match<TResponse, TRequest>? Match(SegmentNode<TResponse, TRequest> segmentNode, HttpMethod httpMethod, IList<string> segments);
    }
}
