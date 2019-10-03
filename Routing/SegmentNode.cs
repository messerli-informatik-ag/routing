#pragma warning disable 660,661

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

        public static bool operator ==(SegmentNode<TResponse, TRequest> left, SegmentNode<TResponse, TRequest> right) => Operator.Weave(left, right);

        public static bool operator !=(SegmentNode<TResponse, TRequest> left, SegmentNode<TResponse, TRequest> right) => Operator.Weave(left, right);
    }
}
