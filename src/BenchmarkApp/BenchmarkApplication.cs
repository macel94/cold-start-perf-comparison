using BenchmarkApp.Endpoints;
using BenchmarkApp.Services;
using BenchmarkApp.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BenchmarkApp;

public static class BenchmarkApplication
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        services.AddSingleton<MatrixComputeService>();
        services.AddSingleton<MatrixComputeRequestValidator>();
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        StartupEndpoint.Map(endpoints);
        MatrixComputeEndpoint.Map(endpoints);
    }
}
