using System.Collections.Generic;

namespace Routing
{
    internal class Match<TResponse, TRequest>
    {
        internal Match(HandleRequest<TResponse, TRequest> handleRequest, IDictionary<string, string> parameters)
        {
            HandleRequest = handleRequest;
            Parameters = parameters;
        }

        internal HandleRequest<TResponse, TRequest> HandleRequest { get; }

        internal IDictionary<string, string> Parameters { get; }
    }
}
