using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Routing.AspNetCore.Test
{
    public class MiddlewareTest
    {
        private const string FallbackResponse = "not found";

        [Fact]
        public async Task FallbackRouteIsCalled()
        {
            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("/");
                var responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(FallbackResponse, responseBody);
            }
        }

        private static TestServer CreateTestServer()
        {
            return new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>());
        }

        public class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
                app.UseRouting(
                    context => context,
                    ApplyResponseToContext,
                    HandleFallbackRequest);
            }

            private static async Task ApplyResponseToContext(HttpContext context, string response) =>
                await context.Response.WriteAsync(response);

            private static string HandleFallbackRequest(HttpContext request) => FallbackResponse;
        }
    }
}
