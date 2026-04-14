using BenchmarkApp.Models;
using BenchmarkApp.Services;
using BenchmarkApp.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BenchmarkApp.Endpoints;

public static class MatrixComputeEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/compute/matrix",
            (MatrixComputeRequest request, MatrixComputeRequestValidator validator, MatrixComputeService computeService) =>
            {
                var validation = validator.Validate(request);
                if (!validation.IsValid)
                {
                    return Results.Json(
                        new ErrorResponse("invalid-payload", validation.Detail),
                        statusCode: validation.StatusCode);
                }

                var result = computeService.Multiply(request);
                return Results.Ok(result);
            })
            .WithName("MatrixCompute");

        return endpoints;
    }
}
