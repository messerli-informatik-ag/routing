using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Funcky;
using Xunit;

namespace Messerli.Routing.Test
{
    public sealed class RoutingTest
    {
        private const string RegisteredRoute = "/registered";

        private const string NameKey = "name";
        private const string AgeKey = "age";

        private const string RootRoute = "/";
        private const string RootRouteWithParam = RootRoute + "{name}";

        private static readonly string RegisteredRouteWithParam = $"{RegisteredRoute}/{{{NameKey}}}";
        private static readonly string RegisteredRouteWithParams = $"{RegisteredRouteWithParam}/ages/{{{AgeKey}}}";

        [Fact]
        public void CallsFallbackRouteWhenNoOthersAreRegistered()
        {
            AssertCallToFallbackRoute(routeRegistry =>
                    routeRegistry.Route(HttpMethod.Get, "/foo", default));
        }

        [Fact]
        public void CallsFallbackRouteWhenOtherRouteIsRegistered()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, "/bar", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, "/foo", default);
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenOtherMethodIsRegistered()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Post, "/foo", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, "/foo", default);
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
                routeRegistry.Route(HttpMethod.Get, route, default);
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenCallingSubRouteOfRegisteredRoute()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute, FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/foo", default);
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenCallingParentRouteOfRegisteredSubRoute()
        {
            AssertCallToFallbackRoute(routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute + "/foo", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute, default);
            });
        }

        [Fact]
        public void CallsParentRoute()
        {
            AssertRouteWasCalled(RegisteredRoute, routeRegistry =>
            {
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute + "/foo", FailOnRequest);
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute, default);
            });
        }

        [Fact]
        public void CallsRegisteredRootRoute()
        {
            AssertRouteWasCalled(RootRoute, routeRegistry =>
            {
                routeRegistry.Route(HttpMethod.Get, RootRoute, default);
            });
        }

        [Fact]
        public void CallsRegisteredSubRouteWhenParentRouteIsRegistered()
        {
            var routeWasCalled = false;

            Unit HandleRequest(Unit request, IDictionary<string, string> routeParams)
            {
                routeWasCalled = true;
                return default;
            }

            const string subRoute = RegisteredRoute + "/foo";
            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, RegisteredRoute, FailOnRequest);
            routeRegistry.Register(HttpMethod.Get, subRoute, HandleRequest);
            routeRegistry.Route(HttpMethod.Get, subRoute, default);

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
                routeRegistry.Route(HttpMethod.Get, route, default));
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
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/" + param, default);
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
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/" + param, default);
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
                routeRegistry.Route(HttpMethod.Get, "/" + param, default);
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
                routeRegistry.Route(HttpMethod.Get, $"{RegisteredRoute}/{firstParam}/ages/{secondParam}", default);
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
                    .Route(HttpMethod.Get, $"/{name}", default);
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
                    .Route(HttpMethod.Get, $"{RegisteredRoute}/foo/bar/baz", default);
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
                    .Route(HttpMethod.Get, $"{RegisteredRoute}/{name}", default);
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
                return default;
            }

            Unit HandleSecondRequest(Unit request, IDictionary<string, string> routeParams)
            {
                calledRoutes[1] = true;
                Assert.Equal(CreateExpectedParams(("bar", "BAR")),  routeParams);
                return default;
            }

            Unit HandleThirdRequest(Unit request, IDictionary<string, string> routeParams)
            {
                calledRoutes[2] = true;
                Assert.Equal(CreateExpectedParams(("foo", "FOO")), routeParams);
                return default;
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry
                .Register(HttpMethod.Get, "/foo/bar/baz", HandleFirstRequest)
                .Register(HttpMethod.Get, "/foo/{bar}", HandleSecondRequest)
                .Register(HttpMethod.Get, "/{foo}", HandleThirdRequest);

            routeRegistry.Route(HttpMethod.Get, "/foo/bar/baz", default);
            routeRegistry.Route(HttpMethod.Get, "/foo/BAR", default);
            routeRegistry.Route(HttpMethod.Get, "/FOO", default);

            Assert.All(calledRoutes, Assert.True);
        }

        [Theory]
        [MemberData(nameof(InvalidParams))]
        public void CallsFallbackWhenRoutingInvalidRouteParam(string param)
        {
            AssertCallToFallbackRoute(routeRegistry =>
                routeRegistry.Route(HttpMethod.Get, RegisteredRoute + "/" + param, default));
        }

        [Fact]
        public void ThrowsWhenValidatingRouteWithNoParameters()
        {
            var routeRegistry = CreateRouteRegistry();
            Assert.Throws<ArgumentException>(() =>
                routeRegistry.Register(HttpMethod.Get, RegisteredRoute, FailOnRequest, FailOnValidation));
        }

        [Fact]
        public void AllParametersArePassedToValidation()
        {
            var validationWasCalled = false;

            bool ValidateParameters(IEnumerable<string> parameters)
            {
                var expectedParameters = new[] { NameKey, AgeKey };
                Assert.Equal(expectedParameters, parameters);
                validationWasCalled = true;
                return true;
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, RegisteredRouteWithParams, FailOnRequest, ValidateParameters);

            Assert.True(validationWasCalled);
        }

        [Fact]
        public void ThrowsOnFailedValidation()
        {
            var routeRegistry = CreateRouteRegistry();

            static bool FailValidation(IEnumerable<string> parameters) => false;

            Assert.Throws<ArgumentException>(() =>
                routeRegistry.Register(HttpMethod.Get, RegisteredRouteWithParams, FailOnRequest, FailValidation));
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

        private static void AssertCallToFallbackRoute(Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var fallbackWasCalled = false;

            Unit HandleFallbackRequest(Unit request)
            {
                fallbackWasCalled = true;
                return default;
            }

            var routeRegistry = RouteRegistryBuilder<Unit, Unit>
                .WithFallbackRequestHandler(HandleFallbackRequest)
                .Build();
            stateManipulation(routeRegistry);

            Assert.True(fallbackWasCalled);
        }

        private static void AssertRouteWasCalled(string registeredRoute, Action<IRouteRegistry<Unit, Unit>> stateManipulation)
        {
            var routeWasCalled = false;

            Unit HandleRequest(Unit request, IDictionary<string, string> routeParams)
            {
                routeWasCalled = true;
                return default;
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, registeredRoute, HandleRequest);

            stateManipulation(routeRegistry);

            Assert.True(routeWasCalled);
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
                return default;
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(HttpMethod.Get, registeredRoute, HandleRequest);

            stateManipulation(routeRegistry);

            Assert.True(routeWasCalled);
        }

        private static IRouteRegistry<Unit, Unit> CreateRouteRegistry()
        {
            return RouteRegistryBuilder<Unit, Unit>
                .WithFallbackRequestHandler(FailOnFallbackRequest)
                .Build();
        }

        private static Unit FailOnFallbackRequest(Unit request)
        {
            throw new InvalidOperationException("Fallback request handler was unexpectedly called");
        }

        private static Unit FailOnRequest(Unit request, IDictionary<string, string> routeParams)
        {
            throw new InvalidOperationException("Request handler was unexpectedly called");
        }

        private static bool FailOnValidation(IEnumerable<string> parameters)
        {
            throw new InvalidOperationException("Parameter validation was unexpectedly called");
        }

        private static Dictionary<string, string> CreateExpectedParams(params (string, string)[] keyValuePairs)
        {
            return keyValuePairs.ToDictionary(keyValuePair => keyValuePair.Item1, keyValuePair => keyValuePair.Item2);
        }
    }
}
