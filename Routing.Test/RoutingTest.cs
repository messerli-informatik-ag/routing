using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;
using Funcky;

namespace Routing.Test
{
    public class RoutingTest
    {
        private const string RegisteredRoute = "/registered";

        private const string NameKey = "name";
        private const string AgeKey = "age";
        private static readonly string RegisteredRouteWithParam = $"{RegisteredRoute}/{{{NameKey}}}";
        private static readonly string RegisteredRouteWithParams = $"{RegisteredRouteWithParam}/ages/{{{AgeKey}}}";

        private const string RootRoute = "/";
        private const string RootRouteWithParam = RootRoute + "{name}";

        [Fact]
        public void CallsFallbackRouteWhenNoOthersAreRegistered()
        {
            AssertCallToFallbackRoute(routeRegistry =>
                    routeRegistry.Route(HttpMethod.Get, "/foo", new Unit()));
        }


        [Fact]
        public void CallsFallbackRouteWhenOtherRouteIsRegistered()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, "/bar", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, "/foo", new Unit());
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenOtherMethodIsRegistered()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Post, "/foo", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, "/foo", new Unit());
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenRegisteredRouteWasRemoved()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                const string route = "/foo";
                routeRegistry.Register(HttpMethod.Get, route, FailOnRequest);
                routeRegistry.Remove(HttpMethod.Get, route);
                routeRegistry.Route(HttpMethod.Get, route, new Unit());
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenCallingSubRouteOfRegisteredRoute()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute, FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/foo", new Unit());
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenCallingParentRouteOfRegisteredSubRoute()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute + "/foo", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute, new Unit());
            });
        }

        private static void AssertCallToFallbackRoute(Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var fallbackWasCalled = false;

            Unit HandleFallbackRequest(Unit request)
            {
                fallbackWasCalled = true;
                return new Unit();
            }

            var routeRegistry = new RouteRegistryFacade<Unit, Unit>(HandleFallbackRequest);
            stateManipulation(routeRegistry);

            Assert.True(fallbackWasCalled);
        }

        [Fact]
        public void CallsParentRoute()
        {
            AssertRouteWasCalled(RegisteredRoute, routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute + "/foo", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute, new Unit());
            });
        }

        [Fact]
        public void CallsRegisteredRootRoute()
        {
            AssertRouteWasCalled(RootRoute, routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, RootRoute, new Unit());
            });
        }

        private static void AssertRouteWasCalled(string registeredRoute, Action < IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var routeWasCalled = false;

            Unit HandleRequest(Unit request, IDictionary<string, string> routeParams)
            {
                routeWasCalled = true;
                return new Unit();
            }
            
            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, registeredRoute, HandleRequest);

            stateManipulation(routeRegistry);

            Assert.True(routeWasCalled);
        }

        [Fact]
        public void CallsRegisteredSubRouteWhenParentRouteIsRegistered()
        {
            var routeWasCalled = false;

            Unit HandleRequest(Unit request, IDictionary<string, string> routeParams)
            {
                routeWasCalled = true;
                return new Unit();
            }

            const string subRoute = RegisteredRoute + "/foo";
            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, RegisteredRoute, FailOnRequest);
            routeRegistry.Register(HttpMethod.Get, subRoute, HandleRequest);
            routeRegistry.Route(HttpMethod.Get, subRoute, new Unit());

            Assert.True(routeWasCalled);
        }

        [Theory]
        [MemberData(nameof(InvalidRoutes))]
        public void ThrowsWhenRegisteringInvalidRoute(string route)
        {
            var routeRegistry = CreateRouteRegistry();
            Assert.Throws<ArgumentException>(() =>
                routeRegistry.Register(HttpMethod.Get, route, FailOnRequest));
        }

        [Theory]
        [MemberData(nameof(InvalidRoutes))]
        public void CallsFallbackWhenRoutingInvalidRoute(string route)
        {
            AssertCallToFallbackRoute(routeRegistry =>
                routeRegistry.Route(HttpMethod.Get, route, new Unit()));
        }

        public static TheoryData<string> InvalidRoutes()
        {
            return new TheoryData<string>
            {
                "//",
                "foo",
                "foo/",
                "/foo/",
                "//foo",
                "/foo//",
                "/foo/bar/",
                "/foo//bar",
                "/foo/bar/",
                "foo/bar",
                string.Empty,
                "/ ",
                "/\t",
                "/\n",
                "/\r",
                "/\r\n",
                "/\b",
                "/😃",
                "/.",
                "/..",
                "/foo;",
                "/foo,",
                "/foo=",
                "/names/{name",
                "/names/name}",
                "/names/{name}/",
                "/names/{;}",
                "/names/{,}",
                "/names/{$}",
                "/names/{!}",
                "/names/{'}",
                "/names/{\"}",
                "/names/{name}/ages/{age",
                "/names/{name}/{name}",
                "/names/{name}/ages/{name}",
                "/names/{name/ages/age}",
                "/names/{ }",
                "/names/{}",
                "/names/{/}",
            };
        }

        [Fact]
        public void ParsesRouteParams()
        {
            const string param = "foo";
            AssertRouteWasCalledWithParams(
                CreateExpectedParams((NameKey, param)),
                RegisteredRouteWithParam,
                routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/" + param, new Unit());
            });
        }

        [Fact]
        public void ParsesNumericRouteParams()
        {
            const string param = "123";
            AssertRouteWasCalledWithParams(
                CreateExpectedParams((NameKey, param)),
                RegisteredRouteWithParam,
                routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/" + param, new Unit());
            });
        }

        [Fact]
        public void ParsesRootRouteParams()
        {
            const string param = "foo";
            AssertRouteWasCalledWithParams(
                CreateExpectedParams((NameKey, param)),
                RootRouteWithParam,
                routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, "/" + param, new Unit());
            });
        }

        [Fact]
        public void ParsesMultipleRouteParams()
        {
            const string firstParam = "foo";
            const string secondParam = "22";

            AssertRouteWasCalledWithParams(
                CreateExpectedParams((NameKey, firstParam), (AgeKey, secondParam)),
                RegisteredRouteWithParams,
                routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, $"{RegisteredRoute}/{firstParam}/ages/{secondParam}", new Unit());
            });
        }

        [Fact]
        public static void RoutesParameterRouteWhenLiteralRootIsAvailable()
        {
            const string name = "foo";
            AssertRouteWasCalledWithParams(
                CreateExpectedParams((NameKey, name)),
                RootRouteWithParam,
                routeRegistry =>
            {
                routeRegistry
                    .Register(HttpMethod.Get, RootRoute, FailOnRequest)
                    .Route(HttpMethod.Get, $"/{name}", new Unit());
            });
        }

        [Fact]
        public static void LiteralsArePreferredFromLeftToRight()
        {
            var expectedRoute = $"{RegisteredRoute}/foo/{{bar}}/{{baz}}";
            var moreLiteralRoute = $"{RegisteredRoute}/{{foo}}/bar/baz";

            AssertRouteWasCalledWithParams(
                CreateExpectedParams(("bar", "bar"), ("baz", "baz")),
                expectedRoute,
                routeRegistry =>
            {
                routeRegistry
                    .Register(HttpMethod.Get, moreLiteralRoute, FailOnRequest)
                    .Route(HttpMethod.Get, $"{RegisteredRoute}/foo/bar/baz", new Unit());
            });
        }

        [Fact]
        public static void FirstRegisteredAmbiguousRouteIsPreferred()
        {
            const string name = "foo";
            var ambiguousRoute = $"{RegisteredRoute}/{{bar}}";

            AssertRouteWasCalledWithParams(
                CreateExpectedParams((NameKey, name)),
                RegisteredRouteWithParam,
                routeRegistry =>
            {
                routeRegistry
                    .Register(HttpMethod.Get, ambiguousRoute, FailOnRequest)
                    .Route(HttpMethod.Get, $"{RegisteredRoute}/{name}", new Unit());
            });
        }

        [Fact]
        public void ParsesNearlyAmbiguousRouteParamsDifferingInPartLength()
        {
            var calledRoutes = new[] { false, false, false };

            Unit HandleFirstRequest(Unit request, IDictionary<string, string> routeParams)
            {
                calledRoutes[0] = true;
                Assert.Empty(routeParams);
                return new Unit();
            }

            Unit HandleSecondRequest(Unit request, IDictionary<string, string> routeParams)
            {
                calledRoutes[1] = true;
                Assert.Equal(CreateExpectedParams(("bar", "BAR")),  routeParams);
                return new Unit();
            }

            Unit HandleThirdRequest(Unit request, IDictionary<string, string> routeParams)
            {
                calledRoutes[2] = true;
                Assert.Equal(CreateExpectedParams(("foo", "FOO")), routeParams);
                return new Unit();
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry
                .Register(HttpMethod.Get, "/foo/bar/baz", HandleFirstRequest)
                .Register(HttpMethod.Get, "/foo/{bar}", HandleSecondRequest)
                .Register(HttpMethod.Get, "/{foo}", HandleThirdRequest);

            routeRegistry.Route(HttpMethod.Get, "/foo/bar/baz", new Unit());
            routeRegistry.Route(HttpMethod.Get, "/foo/BAR", new Unit());
            routeRegistry.Route(HttpMethod.Get, "/FOO", new Unit());

            Assert.All(calledRoutes, Assert.True);
        }

        [Theory]
        [MemberData(nameof(InvalidParams))]
        public void CallsFallbackWhenRoutingInvalidRouteParam(string param)
        {
            AssertCallToFallbackRoute(routeRegistry =>
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/" + param, new Unit()));
        }

        public static TheoryData<string> InvalidParams()
        {
            return new TheoryData<string>
            {
                "/",
                "//",
                "/foo",
                "foo/",
                "/foo/",
                "/foo//",
                "/foo/bar/",
                string.Empty,
                " ",
                "\t",
                "\n",
                "\r",
                "\r\n",
                "\b",
                "😃",
                "{ }",
                "{name}",
                "{}",
                "{/}",
            };
        }

        private static void AssertRouteWasCalledWithParams(
            IDictionary<string, string> expectedRouteParams,
            string registeredRoute,
            Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var routeWasCalled = false;

            Unit HandleRequest(Unit request, IDictionary<string, string> routeParams)
            {
                routeWasCalled = true;
                Assert.Equal(expectedRouteParams, routeParams);
                return new Unit();
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, registeredRoute, HandleRequest);

            stateManipulation(routeRegistry);

            Assert.True(routeWasCalled);
        }

        private static IRouteRegistry<Unit, Unit> CreateRouteRegistry()
        {
            return new RouteRegistryFacade<Unit, Unit>(FailOnFallbackRequest);
        }

        private static Unit FailOnFallbackRequest(Unit request)
        {
            throw new InvalidOperationException("Fallback request handler was unexpectedly called");
        }

        private static Unit FailOnRequest(Unit request, IDictionary<string, string> routeParams)
        {
            throw new InvalidOperationException("Request handler was unexpectedly called");
        }

        private static Dictionary<string, string> CreateExpectedParams(params (string, string)[] keyValuePairs)
        {
            return keyValuePairs.ToDictionary(keyValuePair => keyValuePair.Item1, keyValuePair => keyValuePair.Item2);
        }
    }
}
