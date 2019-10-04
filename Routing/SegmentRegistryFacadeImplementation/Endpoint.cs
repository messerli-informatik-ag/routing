#pragma warning disable 660,661

using System.Net.Http;

namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    [Equals]
    public sealed class Endpoint
    {
        public Endpoint(HttpMethod method, string route)
        {
            Method = method;
            Route = route;
        }

        public HttpMethod Method { get; }

        public string Route { get; }

        public static bool operator ==(Endpoint left, Endpoint right) => Operator.Weave(left, right);

        public static bool operator !=(Endpoint left, Endpoint right) => Operator.Weave(left, right);
    }
}
