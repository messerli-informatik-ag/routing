using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using Funcky;

namespace Routing.Test
{
    public class RoutingTest
    {
        [Fact]
        public void CallsDefaultRouteWhenNoOthersAreRegistered()
        {
            TestCallToDefaultRoute(routeRegistry =>
                    routeRegistry.Route(HttpMethod.Get, "/foo", new Unit()));
        }


        [Fact]
        public void CallsDefaultRouteWhenOtherRouteIsRegistered()
        {
            TestCallToDefaultRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, "/bar", HandleDummyRoute);
                routeRegistry.Route(HttpMethod.Get, "/foo", new Unit());
            });
        }

        [Fact]
        public void CallsDefaultRouteWhenOtherMethodIsRegistered()
        {
            TestCallToDefaultRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Post, "/foo", HandleDummyRoute);
                routeRegistry.Route(HttpMethod.Get, "/foo", new Unit());
            });
        }

        private static void TestCallToDefaultRoute(Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var fallbackWasCalled = false;

            Unit HandleFallbackRequest(Unit request, IDictionary<string, string> @params)
            {
                fallbackWasCalled = true;
                return new Unit();
            }

            var routeRegistry = new RouteRegistry<Unit, Unit>(HandleFallbackRequest);
            stateManipulation(routeRegistry);

            Assert.True(fallbackWasCalled);
        }

        private static Unit HandleDummyRoute(Unit request, IDictionary<string, string> routeParams)
        {
            return new Unit();
        }
    }
}
