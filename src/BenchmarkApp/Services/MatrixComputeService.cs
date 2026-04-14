using Benchmark.Contracts.Payloads;
using BenchmarkApp.Models;

namespace BenchmarkApp.Services;

public sealed class MatrixComputeService
{
    public MatrixComputeResponse Multiply(MatrixComputeRequest request)
    {
        var result = MatrixPayloadFactory.Multiply(request.LeftMatrix, request.RightMatrix);
        return new MatrixComputeResponse(request.PayloadId, request.Dimension, result);
    }
}
