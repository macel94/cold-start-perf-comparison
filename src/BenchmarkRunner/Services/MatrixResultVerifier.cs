using Benchmark.Contracts.Payloads;
using Benchmark.Contracts.Workloads;
using BenchmarkApp.Models;

namespace BenchmarkRunner.Services;

public sealed class MatrixResultVerifier
{
    public bool Verify(PayloadDefinition payload, MatrixComputeResponse response)
    {
        if (response.Dimension != payload.Dimension || response.PayloadId != payload.PayloadId)
        {
            return false;
        }

        var expected = MatrixPayloadFactory.Multiply(payload);
        return Matches(expected, response.ResultMatrix);
    }

    private static bool Matches(double[][] expected, double[][] actual)
    {
        if (expected.Length != actual.Length)
        {
            return false;
        }

        for (var row = 0; row < expected.Length; row++)
        {
            if (expected[row].Length != actual[row].Length)
            {
                return false;
            }

            for (var column = 0; column < expected[row].Length; column++)
            {
                if (Math.Abs(expected[row][column] - actual[row][column]) > 0.001d)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
