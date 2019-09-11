using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using Funcky;

namespace Routing.Test
{
    public class RoutingTest
    {
        private const string RegisteredRoute = "/registered";

        [Fact]
        public void CallsDefaultRouteWhenNoOthersAreRegistered()
        {
            AssertCallToDefaultRoute(routeRegistry =>
                    routeRegistry.Route(HttpMethod.Get, "/foo", new Unit()));
        }


        [Fact]
        public void CallsDefaultRouteWhenOtherRouteIsRegistered()
        {
            AssertCallToDefaultRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, "/bar", HandleDummyRoute);
                routeRegistry.Route(HttpMethod.Get, "/foo", new Unit());
            });
        }

        [Fact]
        public void CallsDefaultRouteWhenOtherMethodIsRegistered()
        {
            AssertCallToDefaultRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Post, "/foo", HandleDummyRoute);
                routeRegistry.Route(HttpMethod.Get, "/foo", new Unit());
            });
        }

        private static void AssertCallToDefaultRoute(Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var fallbackWasCalled = false;

            Unit HandleFallbackRequest(Unit request, IDictionary<string, string> routeParams)
            {
                fallbackWasCalled = true;
                return new Unit();
            }

            var routeRegistry = new RouteRegistry<Unit, Unit>(HandleFallbackRequest);
            stateManipulation(routeRegistry);

            Assert.True(fallbackWasCalled);
        }

        [Fact]
        public void CallsRegisteredRoute()
        {
            AssertRouteWasCalled(routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute, new Unit());
            });
        }

        private static void AssertRouteWasCalled(Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var routeWasCalled = false;

            Unit HandleRequest(Unit request, IDictionary<string, string> routeParams)
            {
                routeWasCalled = true;
                return new Unit();
            }
            
            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, RegisteredRoute, HandleRequest);

            stateManipulation(routeRegistry);

            Assert.True(routeWasCalled);
        }

        private static Unit HandleDummyRoute(Unit request, IDictionary<string, string> routeParams)
        {
            return new Unit();
        }

        private static IRouteRegistry<Unit, Unit> CreateRouteRegistry()
        {
            return new RouteRegistry<Unit, Unit>(FailOnRequest);
        }

        private static Unit FailOnRequest(Unit request, IDictionary<string, string> routeParams)
        {
            throw new InvalidOperationException("Fallback request handler was unexpectedly called");
        }
    }
}
