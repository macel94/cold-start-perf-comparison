using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BenchmarkApp.Endpoints;

public static class StartupEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/startup", () => Results.Ok(new { status = "ready" }))
            .WithName("StartupProbe");

        return endpoints;
    }
}
