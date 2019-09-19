using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Segments = System.Collections.Generic.IEnumerable<Routing.ISegmentVariant>;

namespace Routing
{
    public class RouteRegistry<TResponse, TRequest> : IRouteRegistry<TResponse, TRequest>
    {
        private readonly IDictionary<int, SortedDictionary<Segments, HandleRequest<TResponse, TRequest>>> _registeredRoutes
            = new Dictionary<int, SortedDictionary<Segments, HandleRequest<TResponse, TRequest>>>();

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
            var trimmedRoute = TrimRoute(path);
            var pathSegments = SplitSegments(trimmedRoute)?.ToList();

            if (pathSegments is null || pathSegments.Any(segment => segment is Parameter))
            {
                return _handleFallbackRequest(request, new Dictionary<string, string>());
            }

            var identifiers = pathSegments
                .Select(segment => ((Path)segment).Identifier)
                .ToList();

            var bucket = _registeredRoutes[pathSegments.Count];
            foreach (var (matchingSegments, requestHandler) in bucket)
            {
                var matchersAndValues = matchingSegments
                    .Zip(identifiers, (matcher, value) => new { Matcher = matcher, Value = value})
                    .ToList();
                if (matchersAndValues.All(matcherAndValue => SegmentMatchesIdentifier(matcherAndValue.Matcher, matcherAndValue.Value)))
                {
                    var parameters = matchersAndValues
                        .SelectMany(matcherAndValue => matcherAndValue.Matcher switch
                    {
                        Parameter { Key: var key} => new[] {( key, matcherAndValue.Value) },
                        Path _ => new (string, string)[0],
                        _ => throw new InvalidOperationException($"Type {matcherAndValue.Matcher.GetType()} is not handled")
                    })
                        .ToDictionary(keyValuePair => keyValuePair.Item1, keyValuePair => keyValuePair.Item2);
                    return requestHandler(request, parameters);
                }
            }
            return _handleFallbackRequest(request, new Dictionary<string, string>());
        }

        public IRouteRegistry<TResponse, TRequest> Register(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest)
        {
            var trimmedRoute = TrimRoute(route);
            var segments = SplitSegments(trimmedRoute)?.ToList();
            if (segments is null)
            {
                throw new ArgumentException(nameof(route));
            }

            if (!_registeredRoutes.ContainsKey(segments.Count))
            {
                _registeredRoutes[segments.Count] = new SortedDictionary<Segments, HandleRequest<TResponse, TRequest>>(new SegmentComparer());
            }
            _registeredRoutes[segments.Count].Add(segments, handleRequest);
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
            var segments = path
                .Split(SegmentDelimiterToken)
                .Select(ParseSegment)
                .ToList();
            return segments.All(segment => segment is { })
                ? segments.Select(segment => segment!)
                : null;
        }

        private static ISegmentVariant? ParseSegment(string segment)
        {
            if (IsParameter(segment))
            {
                const int keyDelimiterTokenCount = 2;
                var keyLength = segment.Length - keyDelimiterTokenCount;
                var parameterKey = segment.Substring(startIndex: 1, length: keyLength);
                return IsValidSpecifier(parameterKey)
                    ? new Parameter(parameterKey)
                    : null;
            }

            return IsValidSpecifier(segment)
                ? new Path(segment)
                : null;
        }

        private static bool IsParameter(string segment)
        {
            var startsParameter = segment.StartsWith(ParameterBeginToken.ToString());
            var endsParameter = segment.EndsWith(ParameterEndToken.ToString());

            return (startsParameter, endsParameter) switch
            {
                (false, false) => false,
                (true, true) => true,
                _ => throw new ArgumentException(nameof(segment))
            };
        }
        
        private static bool IsValidSpecifier(string identifier)
        {
            return !string.IsNullOrWhiteSpace(identifier)
                     && !InvalidIdentifiers.Any(identifier.Contains)
                     && identifier.All(char.IsLetterOrDigit);
        }

        private static bool SegmentMatchesIdentifier(ISegmentVariant segment, string identifier) =>
            segment switch
            {
                Path {Identifier: var path} => path == identifier,
                Parameter _ => true,
                _ => throw new InvalidOperationException($"Type {segment.GetType()} is not handled")
            };
    }

    internal class SegmentComparer : IComparer<Segments>
    {
        public int Compare(Segments left, Segments right)
        {
            var leftSpecificity = AssignSpecificity(left);
            var rightSpecificity = AssignSpecificity(right);

            if (leftSpecificity == rightSpecificity)
            {
                return 0;
            }

            if (leftSpecificity < rightSpecificity)
            {
                return -1;
            }

            return 1;
        }

        private static int AssignSpecificity(Segments segments)
        {
            var bytes = segments.Select(segment => segment switch
            {
                Parameter _ => (byte)0,
                Path _ => (byte)1,
                _ => throw new InvalidOperationException($"Type {segment.GetType()} is not handled")
            });

            var bytesInCorrectOrder = BitConverter.IsLittleEndian
                ? bytes.Reverse()
                : bytes;

            return BitConverter.ToInt32(bytesInCorrectOrder.ToArray());
        }
    }
}
