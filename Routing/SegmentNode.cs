using System.Collections.Generic;
using System.Net.Http;

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

        public ISet<SegmentNode<TResponse, TRequest>> LiteralChildren { get; }
            = new HashSet<SegmentNode<TResponse, TRequest>>();

        public ISet<SegmentNode<TResponse, TRequest>> ParameterChildren { get; }
            = new HashSet<SegmentNode<TResponse, TRequest>>();
    }
}
