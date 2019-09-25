using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Routing
{
    internal class SegmentMatcher<TResponse, TRequest>: ISegmentMatcher<TResponse, TRequest>
    { 
        public Match<TResponse, TRequest>? Match(SegmentNode<TResponse, TRequest> segmentNode, HttpMethod httpMethod, IList<string> segments)
        {
            var noParams = new Dictionary<string, string>();

            if (!segments.Any())
            {
                if (!(segmentNode.Matcher is Root))
                {
                    return null;
                }

                var handleRequest = segmentNode.HandleRequestFunctions[httpMethod];
                return handleRequest is { }
                    ? new Match<TResponse, TRequest>(handleRequest, noParams)
                    : null;
            }
            var nextSegment = segments.First();
            if (!SegmentMatching.SegmentMatchesIdentifier(segmentNode.Matcher, nextSegment))
            {
                return null;
            }

            if (segments.Count == 1)
            {
                var parameters = segmentNode.Matcher switch
                {
                    Root _ => noParams,
                    Path _ => noParams,
                    Parameter parameter => new Dictionary<string, string> { { parameter.Key, segments.First() } },
                    _ => throw new NotImplementedException()
                };

                var handleRequest = segmentNode.HandleRequestFunctions[httpMethod];
                return handleRequest is { }
                    ? new Match<TResponse, TRequest>(handleRequest, parameters)
                    : null;
            }

            var tail = segments.Skip(1).ToList();

            return segmentNode.LiteralChildren
                .Concat(segmentNode.ParameterChildren)
                .Select(child => Match(child, httpMethod, tail))
                .FirstOrDefault(handleRequest => handleRequest is { });
        }
    }
}
