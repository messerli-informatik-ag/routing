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
            return this;
        }

        public IRouteRegistry<TResponse, TRequest> Remove(HttpMethod method, string route)
        {
            return this;
        }

        private static string TrimRoute(string route)
        {
            return route.Substring(1);
        }

        
        private void AddChildSegmentMatchers(ICollection<ISegmentVariant> segments, HttpMethod method, HandleRequest<TResponse, TRequest> handleRequest)
        {
            if (!segments.Any())
            {
                return;
            }

            AddChildSegmentNodes(_segmentTree, segments, method, handleRequest);
        }

        private void AddChildSegmentNodes(SegmentNode<TResponse, TRequest> node, ICollection<ISegmentVariant> segments,
            HttpMethod method, HandleRequest<TResponse, TRequest> handleRequest)
        {
            if (segments.Count == 1)
            {
                var child = CreateNode(segments, method, handleRequest);
                AddChildNode(node, child);
                return;
            }

            var head = segments.First();
            var tail = segments.Skip(1).ToList();

            var matchingChild =
                _segmentTree
                .LiteralChildren
                .Prepend(_segmentTree)
                .Concat(_segmentTree.ParameterChildren)
                .FirstOrDefault(child => child.Matcher.Equals(head));

            if (matchingChild is null)
            {
                var child = CreateNode(segments, method, handleRequest);
                AddChildNode(node, child);
            }
            else
            {
                AddChildSegmentNodes(matchingChild, tail, method, handleRequest);
            }
        }

        private static void AddChildNode(SegmentNode<TResponse, TRequest> parent, SegmentNode<TResponse, TRequest> child)
        {
            var collection = child.Matcher switch
            {
                Literal _ => parent.LiteralChildren,
                Parameter _ => parent.ParameterChildren,
                Root _ => throw new InvalidOperationException(),
                _ => throw new NotImplementedException()
            };

            collection.Add(child);
        }

        private SegmentNode<TResponse, TRequest> CreateNode(ICollection<ISegmentVariant> segments,
            HttpMethod method, HandleRequest<TResponse, TRequest> handleRequest)
        {
            var head = segments.First();
            var node = new SegmentNode<TResponse, TRequest>(head);

            if (segments.Count == 1)
            {
                node.HandleRequestFunctions[method] = handleRequest;
                return node;
            }
            
            var tail = segments.Skip(1).ToList();

            var child = CreateNode(tail, method, handleRequest);
            AddChildNode(node, child);

            return node;
        }
    }
}
