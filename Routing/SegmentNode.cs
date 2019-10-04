#pragma warning disable 660,661

using System.Collections.Generic;
using System.Net.Http;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing
{
    [Equals]
    internal sealed class SegmentNode<TRequest, TResponse>
    {
        public SegmentNode(ISegmentVariant matcher)
        {
            Matcher = matcher;
        }

        public ISegmentVariant Matcher { get; }

        public IDictionary<HttpMethod, HandleRequest<TRequest, TResponse>> HandleRequestFunctions { get; }
            = new Dictionary<HttpMethod, HandleRequest<TRequest, TResponse>>();

        public ICollection<SegmentNode<TRequest, TResponse>> LiteralChildren { get; }
            = new OrderedSet<SegmentNode<TRequest, TResponse>>();

        public ICollection<SegmentNode<TRequest, TResponse>> ParameterChildren { get; }
            = new OrderedSet<SegmentNode<TRequest, TResponse>>();

        public static bool operator ==(SegmentNode<TRequest, TResponse> left, SegmentNode<TRequest, TResponse> right) => Operator.Weave(left, right);

        public static bool operator !=(SegmentNode<TRequest, TResponse> left, SegmentNode<TRequest, TResponse> right) => Operator.Weave(left, right);
    }
}
