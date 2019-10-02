using System.Collections.Generic;
using System.Net.Http;
using Routing.SegmentVariant;

namespace Routing
{
    [Equals]
    internal class SegmentNode<TResponse, TRequest>
    {
        public SegmentNode(ISegmentVariant matcher)
        {
            Matcher = matcher;
        }

        public ISegmentVariant Matcher { get; }

        public IDictionary<HttpMethod, HandleRequest<TResponse, TRequest>> HandleRequestFunctions { get; }
            = new Dictionary<HttpMethod, HandleRequest<TResponse, TRequest>>();

        public ICollection<SegmentNode<TResponse, TRequest>> LiteralChildren { get; }
            = new OrderedSet<SegmentNode<TResponse, TRequest>>();

        public ICollection<SegmentNode<TResponse, TRequest>> ParameterChildren { get; }
            = new OrderedSet<SegmentNode<TResponse, TRequest>>();
    }
}
