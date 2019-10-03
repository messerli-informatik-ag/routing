using System;
using System.Collections.Generic;
using System.Linq;
using Routing.Parsing;
using Routing.SegmentVariant;
using static Routing.SegmentRegistryFacadeImplementation.ParsingUtility;

namespace Routing.SegmentRegistryFacadeImplementation
{
    internal sealed class RouteRemover<TRequest, TResponse> : IRouteRemover<TRequest, TResponse>
    {
        private readonly ISegmentParser _segmentParser;

        public RouteRemover(ISegmentParser segmentParser)
        {
            _segmentParser = segmentParser;
        }

        public void Remove(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint)
        {
            var targetNode = FindNodeByRoute(segmentTree, endpoint.Route);
            targetNode?.HandleRequestFunctions?.Remove(endpoint.Method);
            RemoveUnusedNodes(segmentTree, () => { });
        }

        private SegmentNode<TRequest, TResponse>? FindNodeByRoute(SegmentNode<TRequest, TResponse> segmentTree, string route)
        {
            var segments = ParseRoute(_segmentParser, route);
            return segments.Aggregate<ISegmentVariant, SegmentNode<TRequest, TResponse>?>(segmentTree, FindSegmentInNode);
        }

        private static void RemoveUnusedNodes(SegmentNode<TRequest, TResponse> node, Action removeFromParent)
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

        private static void RemoveUnusedChildNodes(ICollection<SegmentNode<TRequest, TResponse>> children)
        {
            var readonlyListOfChildren = children.ToList();
            foreach (var child in readonlyListOfChildren)
            {
                RemoveUnusedNodes(child, () => children.Remove(child));
            }
        }

        private static SegmentNode<TRequest, TResponse>? FindSegmentInNode(SegmentNode<TRequest, TResponse>? node, ISegmentVariant segment)
        {
            return segment switch
            {
                Root _ => node,
                Literal literal => FindSegmentInNodeList(node?.LiteralChildren, literal),
                Parameter parameter => FindSegmentInNodeList(node?.ParameterChildren, parameter),
                _ => throw new InvalidOperationException()
            };
        }

        private static SegmentNode<TRequest, TResponse>? FindSegmentInNodeList(IEnumerable<SegmentNode<TRequest, TResponse>>? node, ISegmentVariant segment)
        {
            return node?.FirstOrDefault(element => element.Matcher.Equals(segment));
        }
    }
}
