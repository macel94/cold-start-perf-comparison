using Benchmark.Contracts.Workloads;

namespace Benchmark.Contracts.Payloads;

public static class MatrixPayloadFactory
{
    public static IReadOnlyList<PayloadDefinition> CreateCatalog() =>
    [
        CreatePayload("matrix-100x100", 100),
        CreatePayload("matrix-200x200", 200)
    ];

    public static PayloadDefinition CreatePayload(string payloadId, int dimension)
    {
        var payload = new PayloadDefinition
        {
            PayloadId = payloadId,
            Dimension = dimension,
            LeftMatrix = CreateMatrix(dimension, left: true),
            RightMatrix = CreateMatrix(dimension, left: false)
        };

        return payload with
        {
            ContentHash = MatrixHashCalculator.ComputePayloadHash(payload)
        };
    }

    public static double[][] Multiply(PayloadDefinition payload) =>
        Multiply(payload.LeftMatrix, payload.RightMatrix);

    public static double[][] Multiply(double[][] leftMatrix, double[][] rightMatrix)
    {
        var dimension = leftMatrix.Length;
        var result = new double[dimension][];

        for (var row = 0; row < dimension; row++)
        {
            result[row] = new double[dimension];
            for (var column = 0; column < dimension; column++)
            {
                double sum = 0;
                for (var inner = 0; inner < dimension; inner++)
                {
                    sum += leftMatrix[row][inner] * rightMatrix[inner][column];
                }

                result[row][column] = sum;
            }
        }

        return result;
    }

    private static double[][] CreateMatrix(int dimension, bool left)
    {
        var matrix = new double[dimension][];

        for (var row = 0; row < dimension; row++)
        {
            matrix[row] = new double[dimension];
            for (var column = 0; column < dimension; column++)
            {
                matrix[row][column] = left
                    ? ((row + 1) * 3 + (column + 1) * 2) % 17 + 1
                    : ((row + 1) * 5 + (column + 1)) % 19 + 1;
            }
        }

        return matrix;
    }
}
