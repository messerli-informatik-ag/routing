namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    public interface IRouteRegistrar<TRequest, TResponse>
    {
        void Register(
            SegmentNode<TRequest, TResponse> segmentTree,
            Endpoint endpoint,
            HandleRequest<TRequest, TResponse> handleRequest);
    }
}
