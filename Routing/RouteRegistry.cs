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
            var segments = _pathParser.Parse(path)?.ToList();
            if (segments is null)
            {
                return _handleFallbackRequest(request);
            }

            var requestHandlingData = Match(_segmentTree, method, segments, new Dictionary<string, string>());
            return requestHandlingData is null
                ? _handleFallbackRequest(request)
                : requestHandlingData.HandleRequest(request, requestHandlingData.Parameters);
        }

        private static RequestHandlingData?
            Match(SegmentNode<TResponse, TRequest> node,
                HttpMethod method,
                ICollection<string> segments,
                IDictionary<string, string> parameters)
        {
            var head = segments.First();

            var currentParameters = node.Matcher is Parameter { Key: var key }
                ? AddToDictionary(parameters, (key, head))
                : parameters;

            switch (node.Matcher)
            {
                case Literal { Identifier: var matchingSegment } when head != matchingSegment:
                case Root _ when head != "/":
                    return null;
            }

            if (segments.Count == 1)
            {
                return node.HandleRequestFunctions.TryGetValue(method, out var handleRequest)
                    ? new RequestHandlingData(handleRequest, currentParameters)
                    : null;
            }

            var tail = segments.Skip(1).ToList();

            return node.LiteralChildren
                       .Concat(node.ParameterChildren)
                       .Select(child => Match(child, method, tail, currentParameters))
                       .FirstOrDefault(child => child != null);
        }

        private static IDictionary<TKey, TValue> AddToDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, (TKey, TValue) keyValuePair)
        {
            var (key, value) = keyValuePair;
            return new Dictionary<TKey, TValue>(dictionary)
            {
                [key] = value
            };
        }


        private static bool SegmentMatchesIdentifier(ISegmentVariant segment, string identifier) =>
            segment switch
            {
                Literal { Identifier: var path } => path == identifier,
                Parameter _ => true,
                Root _ => false,
                _ => throw new InvalidOperationException($"Type {segment.GetType()} is not handled")
            };

        private static string? ExtractKey(ISegmentVariant segment)
        {
            return segment is Parameter { Key: var key}
                ? key
                : null;
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

        private class RequestHandlingData
        {
            public RequestHandlingData(HandleRequest<TResponse, TRequest> handleRequest, IDictionary<string, string> parameters)
            {
                HandleRequest = handleRequest;
                Parameters = parameters;
            }

            public HandleRequest<TResponse, TRequest> HandleRequest { get; }

            public IDictionary<string, string> Parameters { get; }
        }
    }
}
