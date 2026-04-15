using BenchmarkApp.Models;

namespace BenchmarkApp.Validation;

public sealed class MatrixComputeRequestValidator
{
    public MatrixComputeValidationResult Validate(MatrixComputeRequest request)
    {
        if (request is null)
        {
            return MatrixComputeValidationResult.Invalid(400, "Request body is required.");
        }

        if (request.Dimension is not (100 or 200))
        {
            return MatrixComputeValidationResult.Invalid(413, "Only 100x100 and 200x200 payloads are supported in v1.");
        }

        var expectedPayloadId = request.Dimension == 100 ? "matrix-100x100" : "matrix-200x200";
        if (!string.Equals(request.PayloadId, expectedPayloadId, StringComparison.Ordinal))
        {
            return MatrixComputeValidationResult.Invalid(400, "payloadId does not match the declared dimension.");
        }

        if (!IsSquareMatrix(request.LeftMatrix, request.Dimension) || !IsSquareMatrix(request.RightMatrix, request.Dimension))
        {
            return MatrixComputeValidationResult.Invalid(400, "Matrices must be square and match the declared dimension.");
        }

        return MatrixComputeValidationResult.Valid();
    }

    private static bool IsSquareMatrix(double[][] matrix, int dimension) =>
        matrix.Length == dimension && matrix.All(row => row is not null && row.Length == dimension);
}

public sealed record MatrixComputeValidationResult(bool IsValid, int StatusCode, string? Detail)
{
    public static MatrixComputeValidationResult Valid() => new(true, StatusCode: 200, Detail: null);

    public static MatrixComputeValidationResult Invalid(int statusCode, string detail) => new(false, statusCode, detail);
}
