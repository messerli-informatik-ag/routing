using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Routing.SegmentVariant;
using Segments = System.Collections.Generic.IEnumerable<Routing.SegmentVariant.ISegmentVariant>;

namespace Routing
{
    public class RouteRegistry<TResponse, TRequest> : IRouteRegistry<TResponse, TRequest>
    {
        private readonly ISegmentMatcher<TResponse, TRequest> _segmentMatcher = new SegmentMatcher<TResponse, TRequest>();

        private readonly SegmentNode<TResponse, TRequest> _segmentTree
            = new SegmentNode<TResponse, TRequest>(new Root());

        private readonly Func<TRequest, TResponse> _handleFallbackRequest;

        private readonly ISegmentParser _segmentParser = new SegmentParser();

        public RouteRegistry(Func<TRequest, TResponse> handleFallbackRequest)
        {
            _handleFallbackRequest = handleFallbackRequest;
        }

        public TResponse Route(HttpMethod method, string path, TRequest request)
        {
            return _handleFallbackRequest(request);
        }

        public IRouteRegistry<TResponse, TRequest> Register(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest)
        {
            var segments = _segmentParser.Parse(route)
                           ?? throw new ArgumentException($"Invalid route: {route}", nameof(route));
            var targetNode = segments
                .Aggregate(_segmentTree, (node, segment) =>
                    segment switch
                    {
                        Root _ => node,
                        Literal literal => FindOrInsertNode(node.LiteralChildren, literal),
                        Parameter parameter => FindOrInsertNode(node.ParameterChildren, parameter),
                        _ => throw new InvalidOperationException()
                    }
                );
            targetNode.HandleRequestFunctions[method] = handleRequest;

            return this;
        }

        private static SegmentNode<TResponse, TRequest> FindOrInsertNode(ICollection<SegmentNode<TResponse, TRequest>> collection, ISegmentVariant segment)
        {
            var existingNode = collection.FirstOrDefault(element => element.Matcher.Equals(segment));
            return existingNode ?? InsertNode(collection, segment);
        }
        private static SegmentNode<TResponse, TRequest> InsertNode(ICollection<SegmentNode<TResponse, TRequest>> collection, ISegmentVariant segment)
        {
            var newNode = new SegmentNode<TResponse, TRequest>(segment);
            collection.Add(newNode);
            return newNode;
        }

        public IRouteRegistry<TResponse, TRequest> Remove(HttpMethod method, string route)
        {
            return this;
        }
    }
}
