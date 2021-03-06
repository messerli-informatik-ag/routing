﻿using System;
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
        public void FallbackHandlerCanBeCalled()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
                routeRegistry.CallFallbackHandler(default));
        }

        [Fact]
        public void CallsFallbackRequestHandlerWhenNoOthersAreRegistered()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
                    routeRegistry.Route(new Endpoint(HttpMethod.Get, "/foo"), default));
        }

        [Fact]
        public void CallsFallbackRequestHandlerWhenOtherRouteIsRegistered()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Get, "/bar"), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, "/foo"), default);
            });
        }

        [Fact]
        public void CallsFallbackRequestHandlerWhenOtherMethodIsRegistered()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Post, "/foo"), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, "/foo"), default);
            });
        }

        [Fact]
        public void CallsFallbackRequestHandlerWhenRegisteredRouteWasRemoved()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
            {
                const string route = "/foo";
                routeRegistry.Register(new Endpoint(HttpMethod.Get, route), FailOnRequest);
                routeRegistry.Remove(new Endpoint(HttpMethod.Get, route));
                routeRegistry.Route(new Endpoint(HttpMethod.Get, route), default);
            });
        }

        [Fact]
        public void CallsFallbackRequestHandlerWhenCallingSubRouteOfRegisteredRoute()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRoute), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RegisteredRoute + "/foo"), default);
            });
        }

        [Fact]
        public void CallsFallbackRequestHandlerWhenCallingParentRouteOfRegisteredSubRoute()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRoute + "/foo"), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RegisteredRoute), default);
            });
        }

        [Fact]
        public void CallsParentRoute()
        {
            AssertRouteWasCalled(RegisteredRoute, routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRoute + "/foo"), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RegisteredRoute), default);
            });
        }

        [Fact]
        public void CallsRegisteredRootRoute()
        {
            AssertRouteWasCalled(RootRoute, routeRegistry =>
            {
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RootRoute), default);
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
            routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRoute), FailOnRequest);
            routeRegistry.Register(new Endpoint(HttpMethod.Get, subRoute), HandleRequest);
            routeRegistry.Route(new Endpoint(HttpMethod.Get, subRoute), default);

            Assert.True(routeWasCalled);
        }

        [Theory]
        [MemberData(nameof(InvalidRoutes))]
        public void ThrowsWhenRegisteringInvalidRoute(string route)
        {
            var routeRegistry = CreateRouteRegistry();
            Assert.Throws<ArgumentException>(() =>
                routeRegistry.Register(new Endpoint(HttpMethod.Get, route), FailOnRequest));
        }

        [Theory]
        [MemberData(nameof(InvalidRoutes))]
        public void CallsFallbackWhenRoutingInvalidRoute(string route)
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
                routeRegistry.Route(new Endpoint(HttpMethod.Get, route), default));
        }

        [Fact]
        public void CallsParentRouteWhenRequestPathEndsWithSegmentDelimiter()
        {
            AssertRouteWasCalled("/echo", routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Get, "/echo/{message}"), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, "/echo/"), default);
            });
        }

        [Fact]
        public void CallsFallbackRouteWhenRequestPathEndsWithMoreThanOneSegmentDelimiter()
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
            {
                routeRegistry.Register(new Endpoint(HttpMethod.Get, "/echo"), FailOnRequest);
                routeRegistry.Route(new Endpoint(HttpMethod.Get, "/echo///"), default);
            });
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
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RegisteredRoute + "/" + param), default);
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
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RegisteredRoute + "/" + param), default);
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
                routeRegistry.Route(new Endpoint(HttpMethod.Get, "/" + param), default);
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
                routeRegistry.Route(new Endpoint(HttpMethod.Get, $"{RegisteredRoute}/{firstParam}/ages/{secondParam}"), default);
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
                    .Register(new Endpoint(HttpMethod.Get, RootRoute), FailOnRequest)
                    .Route(new Endpoint(HttpMethod.Get, $"/{name}"), default);
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
                    .Register(new Endpoint(HttpMethod.Get, moreLiteralRoute), FailOnRequest)
                    .Route(new Endpoint(HttpMethod.Get, $"{RegisteredRoute}/foo/bar/baz"), default);
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
                    .Register(new Endpoint(HttpMethod.Get, ambiguousRoute), FailOnRequest)
                    .Route(new Endpoint(HttpMethod.Get, $"{RegisteredRoute}/{name}"), default);
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
                .Register(new Endpoint(HttpMethod.Get, "/foo/bar/baz"), HandleFirstRequest)
                .Register(new Endpoint(HttpMethod.Get, "/foo/{bar}"), HandleSecondRequest)
                .Register(new Endpoint(HttpMethod.Get, "/{foo}"), HandleThirdRequest);

            routeRegistry.Route(new Endpoint(HttpMethod.Get, "/foo/bar/baz"), default);
            routeRegistry.Route(new Endpoint(HttpMethod.Get, "/foo/BAR"), default);
            routeRegistry.Route(new Endpoint(HttpMethod.Get, "/FOO"), default);

            Assert.All(calledRoutes, Assert.True);
        }

        [Theory]
        [MemberData(nameof(InvalidParams))]
        public void CallsFallbackWhenRoutingInvalidRouteParam(string param)
        {
            AssertCallToFallbackRequestHandler(routeRegistry =>
                routeRegistry.Route(new Endpoint(HttpMethod.Get, RegisteredRoute + "/" + param), default));
        }

        [Fact]
        public void AllowValidatingRouteWithNoParameters()
        {
            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRoute), FailOnRequest, EmptyValidation);
        }

        [Fact]
        public void AllParametersArePassedToValidation()
        {
            var validationWasCalled = false;

            void ValidateParameters(IEnumerable<string> parameters)
            {
                var expectedParameters = new[] { NameKey, AgeKey };
                Assert.Equal(expectedParameters, parameters);
                validationWasCalled = true;
            }

            var routeRegistry = CreateRouteRegistry();
            routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRouteWithParams), FailOnRequest, ValidateParameters);

            Assert.True(validationWasCalled);
        }

        [Fact]
        public void WrapsExceptionOfFailedValidation()
        {
            var routeRegistry = CreateRouteRegistry();

            var exception = Assert.Throws<ArgumentException>(() =>
                routeRegistry.Register(new Endpoint(HttpMethod.Get, RegisteredRouteWithParams), FailOnRequest, FailOnValidation));

            Assert.IsType<InvalidOperationException>(exception.InnerException);
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

        private static void AssertCallToFallbackRequestHandler(Action<IRouteRegistry<Unit, Unit>> stateManipulation)
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
            routeRegistry.Register(new Endpoint(HttpMethod.Get, registeredRoute), HandleRequest);

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
            routeRegistry.Register(new Endpoint(HttpMethod.Get, registeredRoute), HandleRequest);

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

        private static void EmptyValidation(IEnumerable<string> parameters)
        {
        }

        private static void FailOnValidation(IEnumerable<string> parameters)
        {
            throw new InvalidOperationException("Parameter validation has failed");
        }

        private static Dictionary<string, string> CreateExpectedParams(params (string, string)[] keyValuePairs)
        {
            return keyValuePairs.ToDictionary(keyValuePair => keyValuePair.Item1, keyValuePair => keyValuePair.Item2);
        }
    }
}
