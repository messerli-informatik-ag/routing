namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    internal interface IRouter<TRequest, TResponse>
    {
        TResponse Route(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint, TRequest request);

        TResponse CallFallbackHandler(TRequest request);
    }
}
