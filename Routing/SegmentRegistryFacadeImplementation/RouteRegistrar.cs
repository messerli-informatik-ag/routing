using System;
using System.Collections.Generic;
using System.Linq;
using Messerli.Routing.Parsing;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing.SegmentRegistryFacadeImplementation
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
            var segments = ParseRoute(endpoint);
            RegisterRequestHandler(segmentTree, endpoint, handleRequest, segments);
        }

        public void Register(
            SegmentNode<TRequest, TResponse> segmentTree,
            Endpoint endpoint,
            HandleRequest<TRequest, TResponse> handleRequest,
            ValidateParameterKeys validateParameterKeys)
        {
            var segments = ParseRoute(endpoint).ToList();
            RunParameterKeyValidation(validateParameterKeys, segments);
            RegisterRequestHandler(segmentTree, endpoint, handleRequest, segments);
        }

        private IEnumerable<ISegmentVariant> ParseRoute(Endpoint endpoint) =>
            ParsingUtility.ParseRoute(_segmentParser, endpoint.Route);

        private static void RegisterRequestHandler(
            SegmentNode<TRequest, TResponse> segmentTree,
            Endpoint endpoint,
            HandleRequest<TRequest, TResponse> handleRequest,
            IEnumerable<ISegmentVariant> segments)
        {
            var targetNode = FindTargetNode(segmentTree, segments);
            RegisterRequestHandlerOnNode(endpoint, handleRequest, targetNode);
        }

        private static SegmentNode<TRequest, TResponse> FindTargetNode(SegmentNode<TRequest, TResponse> segmentTree, IEnumerable<ISegmentVariant> segments) =>
            segments
                .Aggregate(segmentTree, (node, segment) =>
                    segment switch
                    {
                        Root _ => node,
                        Literal literal => FindOrInsertNode(node.LiteralChildren, literal),
                        Parameter parameter => FindOrInsertNode(node.ParameterChildren, parameter),
                        _ => throw new InvalidOperationException()
                    });

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

        private static void RegisterRequestHandlerOnNode(Endpoint endpoint, HandleRequest<TRequest, TResponse> handleRequest, SegmentNode<TRequest, TResponse> targetNode)
        {
            targetNode.HandleRequestFunctions[endpoint.Method] = handleRequest;
        }

        private static void RunParameterKeyValidation(ValidateParameterKeys validateParameterKeys, IEnumerable<ISegmentVariant> segments)
        {
            var parameters = segments
                .OfType<Parameter>()
                .Select(parameter => parameter.Key)
                .ToList();

            if (!parameters.Any())
            {
                throw new ArgumentException(
                    "Passing a parameter validation callback to the registration of a route with no parameters is useless.");
            }

            try
            {
                validateParameterKeys(parameters);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Parameter validation failed", exception);
            }
        }
    }
}
