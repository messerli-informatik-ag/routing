using System;
using Xunit;

namespace Routing.Test
{
    public class RoutingTest
    {
        [Fact]
        public void CallsDefaultRouteWhenNoOthersAreRegistered()
        {
            var fallbackWasCalled = false;
        }
    }
}
