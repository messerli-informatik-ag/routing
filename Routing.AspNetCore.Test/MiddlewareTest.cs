using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Messerli.Routing.AspNetCore.Test
{
    public class MiddlewareTest
    {
        private const string FallbackResponse = "not found";

        private const string ErrorRoute = "/error";

        [Fact]
        public async Task FallbackRouteIsCalled()
        {
            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                using var response = await client.GetAsync("/");
                var responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(FallbackResponse, responseBody);
            }
        }

        [Fact]
        public async Task ErrorsArePropagated()
        {
            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                await Assert.ThrowsAsync<CustomException>(async () =>
                {
                    await client.GetAsync(ErrorRoute);
                });
            }
        }

        private static TestServer CreateTestServer() =>
            new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>());

        private class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
                var routeRegistry = RouteRegistryBuilder<HttpContext, string>
                    .WithFallbackRequestHandler(HandleFallbackRequest)
                    .Build();

                RegisterErrorRoute(routeRegistry);

                app.UseRouting(
                    routeRegistry,
                    Identity,
                    ApplyResponseToContext);
            }

            private static void RegisterErrorRoute(IRouteRegistry<HttpContext, string> routeRegistry)
            {
                routeRegistry.Register(
                    new Endpoint(HttpMethod.Get, ErrorRoute),
                    (context, parameters) => throw new CustomException());
            }

            private static T Identity<T>(T value) =>
                value;

            private static async Task ApplyResponseToContext(HttpContext context, string response) =>
                await context.Response.WriteAsync(response);

            private static string HandleFallbackRequest(HttpContext request) =>
                FallbackResponse;
        }

        private class CustomException : Exception
        {
        }
    }
}
