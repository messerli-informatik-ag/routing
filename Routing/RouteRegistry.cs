using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Routing.Parsing;
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
        private readonly IPathParser _pathParser = new PathParser();

        public RouteRegistry(Func<TRequest, TResponse> handleFallbackRequest)
        {
            _handleFallbackRequest = handleFallbackRequest;
        }

        public TResponse Route(HttpMethod method, string path, TRequest request)
        {
            var segments = _pathParser.Parse(path);
            if (segments is null)
            {
                return _handleFallbackRequest(request);
            }
            // Todo
            return _handleFallbackRequest(request);
        }

        public IRouteRegistry<TResponse, TRequest> Register(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest)
        {
            var segments = ParseRoute(route);
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
            var targetNode = FindNodeByRoute(route);
            targetNode?.HandleRequestFunctions?.Remove(method);
            RemoveUnusedNodes(_segmentTree, () => {});
            return this;
        }

        private SegmentNode<TResponse, TRequest>? FindNodeByRoute(string route)
        {
            var segments = ParseRoute(route);
            return segments.Aggregate<ISegmentVariant, SegmentNode<TResponse, TRequest>?>(_segmentTree, FindSegmentInNode);
        }

        private IEnumerable<ISegmentVariant> ParseRoute(string route)
        {
            return _segmentParser.Parse(route) ?? throw new ArgumentException($"Invalid route: {route}", nameof(route));
        }

        private static void RemoveUnusedNodes(SegmentNode<TResponse, TRequest> node, Action removeFromParent)
        {
            bool HasParameterChildren() => node.ParameterChildren.Any();
            bool HasLiteralChildren() => node.LiteralChildren.Any();
            bool HasHandlersRegistered() => node.HandleRequestFunctions.Any();
            bool IsNodeUsed() => HasHandlersRegistered() || HasLiteralChildren() || HasParameterChildren();

            RemoveUnusedChildNodes(node.ParameterChildren);
            RemoveUnusedChildNodes(node.LiteralChildren);

            if (!IsNodeUsed())
            {
                removeFromParent();
            }
        }

        private static void RemoveUnusedChildNodes(ICollection<SegmentNode<TResponse, TRequest>> children)
        {
            var readonlyListOfChildren = children.ToList();
            foreach (var child in readonlyListOfChildren)
            {
                RemoveUnusedNodes(child, () => children.Remove(child));
            }
        }

        private static SegmentNode<TResponse, TRequest>? FindSegmentInNode(SegmentNode<TResponse, TRequest>?  node, ISegmentVariant segment)
        {
            return segment switch
            {
                Root _ => node,
                Literal literal => node?.LiteralChildren.FirstOrDefault(element => element.Matcher.Equals(literal)),
                Parameter parameter => node?.ParameterChildren.FirstOrDefault(element => element.Matcher.Equals(parameter)),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
