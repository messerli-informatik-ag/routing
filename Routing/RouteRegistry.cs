using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Segments = System.Collections.Generic.IEnumerable<Routing.ISegmentVariant>;

namespace Routing
{
    public class RouteRegistry<TResponse, TRequest> : IRouteRegistry<TResponse, TRequest>
    {
        private readonly ISegmentMatcher<TResponse, TRequest> _segmentMatcher = new SegmentMatcher<TResponse, TRequest>();

        private readonly SegmentNode<TResponse, TRequest> _rootSegmentNode = new SegmentNode<TResponse, TRequest>(new Root());

        private readonly HandleRequest<TResponse, TRequest> _handleFallbackRequest;

        private const char ParameterBeginToken = '{';
        private const char ParameterEndToken = '}';
        private const char SegmentDelimiterToken = '/';

        private static IEnumerable<string> InvalidIdentifiers => new[]
        {
            ParameterBeginToken.ToString(),
            ParameterEndToken.ToString(),
            SegmentDelimiterToken.ToString()
        };

        public RouteRegistry(HandleRequest<TResponse, TRequest> handleFallbackRequest)
        {
            _handleFallbackRequest = handleFallbackRequest;
        }

        public TResponse Route(HttpMethod method, string path, TRequest request)
        {
            var noParameters = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(path))
            {
                return _handleFallbackRequest(request, noParameters);
            }

            var trimmedRoute = TrimRoute(path);
            var pathSegments = SplitSegments(trimmedRoute)?.ToList();

            if (pathSegments is null)
            {
                return _handleFallbackRequest(request, noParameters);
            }
            
            var match = _segmentMatcher.Match(_rootSegmentNode, method, pathSegments);
            return match is null
                ? _handleFallbackRequest(request, noParameters)
                : match.HandleRequest(request, match.Parameters);
        }

        public IRouteRegistry<TResponse, TRequest> Register(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest)
        {
            var trimmedRoute = TrimRoute(route);
            var segments = SplitSegments(trimmedRoute)?.ToList();
            if (segments is null)
            {
                throw new ArgumentException(nameof(route));
            }
            
            AddChildSegmentMatchers(segments, method, handleRequest);
            
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

        private static Segments? SplitSegments(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new[] { new Root() };
            }

            var segments = path
                .Split(SegmentDelimiterToken)
                .Select(ParseSegment)
                .Prepend(new Root())
                .ToList();
            return segments.All(segment => segment is { })
                ? segments.Select(segment => segment!)
                : null;
        }

        private static ISegmentVariant? ParseSegment(string segment)
        {
            if (!IsParameter(segment))
            {
                return IsValidSpecifier(segment)
                    ? new Path(segment)
                    : null;
            }

            const int keyDelimiterTokenCount = 2;
            var keyLength = segment.Length - keyDelimiterTokenCount;
            var parameterKey = segment.Substring(startIndex: 1, length: keyLength);

            return IsValidSpecifier(parameterKey)
                ? new Parameter(parameterKey)
                : null;
        }

        private static bool IsParameter(string segment)
        {
            var startsParameter = segment.StartsWith(ParameterBeginToken.ToString());
            var endsParameter = segment.EndsWith(ParameterEndToken.ToString());

            return startsParameter && endsParameter;
        }
        
        private static bool IsValidSpecifier(string identifier)
        {
            return !string.IsNullOrWhiteSpace(identifier)
                     && !InvalidIdentifiers.Any(identifier.Contains)
                     && identifier.All(char.IsLetterOrDigit);
        }
        
        private void AddChildSegmentMatchers(ICollection<ISegmentVariant> segments, HttpMethod method, HandleRequest<TResponse, TRequest> handleRequest)
        {
            if (!segments.Any())
            {
                return;
            }

            AddChildSegmentNodes(_rootSegmentNode, segments, method, handleRequest);
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
                _rootSegmentNode
                .LiteralChildren
                .Prepend(_rootSegmentNode)
                .Concat(_rootSegmentNode.ParameterChildren)
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
                Path _ => parent.LiteralChildren,
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
