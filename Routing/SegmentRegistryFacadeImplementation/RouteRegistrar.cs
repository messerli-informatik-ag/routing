using System;
using System.Collections.Generic;
using System.Linq;
using Routing.Parsing;
using Routing.SegmentVariant;

namespace Routing.SegmentRegistryFacadeImplementation
{
    internal sealed class RouteRegistrar<TRequest, TResponse> : IRouteRegistrar<TRequest, TResponse>
    {
        private readonly ISegmentParser _segmentParser;

        public RouteRegistrar(ISegmentParser segmentParser)
        {
            _segmentParser = segmentParser;
        }

        public void Register(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint, HandleRequest<TRequest, TResponse> handleRequest)
        {
            var segments = ParsingUtility.ParseRoute(_segmentParser, endpoint.Route);
            var targetNode = segments
                .Aggregate(segmentTree, (node, segment) =>
                    segment switch
                    {
                        Root _ => node,
                        Literal literal => FindOrInsertNode(node.LiteralChildren, literal),
                        Parameter parameter => FindOrInsertNode(node.ParameterChildren, parameter),
                        _ => throw new InvalidOperationException()
                    }
                );
            targetNode.HandleRequestFunctions[endpoint.Method] = handleRequest;
        }

        private static SegmentNode<TRequest, TResponse> FindOrInsertNode(ICollection<SegmentNode<TRequest, TResponse>> collection, ISegmentVariant segment)
        {
            var existingNode = collection.FirstOrDefault(element => element.Matcher.Equals(segment));
            return existingNode ?? InsertNode(collection, segment);
        }

        private static SegmentNode<TRequest, TResponse> InsertNode(ICollection<SegmentNode<TRequest, TResponse>> collection, ISegmentVariant segment)
        {
            var newNode = new SegmentNode<TRequest, TResponse>(segment);
            collection.Add(newNode);
            return newNode;
        }
    }
}
