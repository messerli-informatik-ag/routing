using System.Collections.Generic;
using System.Net.Http;
using Routing.SegmentVariant;

namespace Routing
{
    internal interface ISegmentMatcher<TResponse, TRequest>
    {
        Match<TResponse, TRequest>? Match(SegmentNode<TResponse, TRequest> segmentNode, HttpMethod httpMethod, IList<ISegmentVariant> segments);
    }
}
