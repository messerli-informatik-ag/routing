using System.Net.Http;

namespace Routing.SegmentRegistryFacadeImplementation
{
    internal interface IRouter<TRequest, TResponse>
    {
        TResponse Route(SegmentNode<TRequest, TResponse> segmentTree, HttpMethod method, string path, TRequest request);
    }
}
