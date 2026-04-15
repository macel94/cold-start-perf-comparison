namespace BenchmarkApp.Models;

public sealed record MatrixComputeRequest(
    string PayloadId,
    int Dimension,
    double[][] LeftMatrix,
    double[][] RightMatrix);

public sealed record MatrixComputeResponse(
    string PayloadId,
    int Dimension,
    double[][] ResultMatrix);

public sealed record ErrorResponse(
    string Error,
    string? Detail = null);
