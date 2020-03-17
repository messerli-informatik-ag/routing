using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Messerli.Routing.Parsing;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    internal sealed class Router<TRequest, TResponse> : IRouter<TRequest, TResponse>
    {
        private readonly IPathParser _pathParser;

        private readonly Func<TRequest, TResponse> _handleFallbackRequest;

        public Router(
            IPathParser pathParser,
            Func<TRequest, TResponse> handleFallbackRequest)
        {
            _pathParser = pathParser;
            _handleFallbackRequest = handleFallbackRequest;
        }

        public TResponse Route(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint, TRequest request)
        {
            var segments = _pathParser.Parse(endpoint.Route)?.ToList();
            if (segments is null)
            {
                return _handleFallbackRequest(request);
            }

            var requestHandlingData = Match(segmentTree, endpoint.Method, segments, new Dictionary<string, string>());
            return requestHandlingData is null
                ? _handleFallbackRequest(request)
                : requestHandlingData(request);
        }

        public TResponse CallFallbackHandler(TRequest request) =>
            _handleFallbackRequest(request);

        private static Func<TRequest, TResponse>?
            Match(
                SegmentNode<TRequest, TResponse> node,
                HttpMethod method,
                ICollection<string> segments,
                IDictionary<string, string> parameters)
        {
            var head = segments.First();

            var currentParameters = node.Matcher is Parameter { Key: var key }
                ? AddToDictionary(parameters, (key, head))
                : parameters;

            if (!NodeMatchesSegment(node, head))
            {
                return null;
            }

            if (segments.Count == 1)
            {
                return node.HandleRequestFunctions.TryGetValue(method, out var handleRequest)
                    ? CurryParameters(handleRequest, currentParameters)
                    : null;
            }

            var tail = segments.Skip(1).ToList();

            return node.LiteralChildren
                .Concat(node.ParameterChildren)
                .Select(child => Match(child, method, tail, currentParameters))
                .FirstOrDefault(child => child != null);
        }

        private static Func<TRequest, TResponse> CurryParameters(
            HandleRequest<TRequest, TResponse> handleRequest,
            IDictionary<string, string> parameters) =>
            request => handleRequest(request, parameters);

        private static bool NodeMatchesSegment(SegmentNode<TRequest, TResponse> node, string segment) =>
            !(node.Matcher is Literal { Identifier: var matchingSegment } && segment != matchingSegment);

        private static IDictionary<TKey, TValue> AddToDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, (TKey Key, TValue Value) keyValuePair)
        {
            var (key, value) = keyValuePair;
            return new Dictionary<TKey, TValue>(dictionary)
            {
                [key] = value,
            };
        }
    }
}
