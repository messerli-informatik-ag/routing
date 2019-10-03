namespace Routing.SegmentRegistryFacadeImplementation
{
    internal interface IRouteRegistrar<TRequest, TResponse>
    {
        void Register(
            SegmentNode<TRequest, TResponse> segmentTree,
            Endpoint endpoint,
            HandleRequest<TRequest, TResponse> handleRequest);
    }
}
