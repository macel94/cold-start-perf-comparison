using Benchmark.Contracts.Payloads;
using BenchmarkApp.Models;
using BenchmarkApp.Services;
using BenchmarkApp.Validation;

namespace Benchmark.UnitTests;

public sealed class MatrixComputeServiceTests
{
    [Fact]
    public void Matrix_compute_service_multiplies_matrices_correctly()
    {
        var service = new MatrixComputeService();
        var request = new MatrixComputeRequest(
            "matrix-2x2",
            2,
            [[1, 2], [3, 4]],
            [[5, 6], [7, 8]]);

        var response = service.Multiply(request);

        Assert.Equal(19d, response.ResultMatrix[0][0]);
        Assert.Equal(22d, response.ResultMatrix[0][1]);
        Assert.Equal(43d, response.ResultMatrix[1][0]);
        Assert.Equal(50d, response.ResultMatrix[1][1]);
    }

    [Fact]
    public void Request_validator_accepts_the_fixed_v1_payload_shapes()
    {
        var payload = MatrixPayloadFactory.CreatePayload("matrix-100x100", 100);
        var validator = new MatrixComputeRequestValidator();

        var result = validator.Validate(new MatrixComputeRequest(payload.PayloadId, payload.Dimension, payload.LeftMatrix, payload.RightMatrix));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Request_validator_rejects_payloads_outside_the_v1_guardrails()
    {
        var validator = new MatrixComputeRequestValidator();
        var oversized = validator.Validate(new MatrixComputeRequest("matrix-300x300", 300, [[1]], [[1]]));
        var mismatchedId = validator.Validate(new MatrixComputeRequest("matrix-200x200", 100, new double[100][], new double[100][]));

        Assert.False(oversized.IsValid);
        Assert.Equal(413, oversized.StatusCode);
        Assert.False(mismatchedId.IsValid);
        Assert.Equal(400, mismatchedId.StatusCode);
    }
}
