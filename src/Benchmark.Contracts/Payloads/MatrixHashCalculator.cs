using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Benchmark.Contracts.Workloads;

namespace Benchmark.Contracts.Payloads;

public static class MatrixHashCalculator
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string ComputePayloadHash(PayloadDefinition payload)
    {
        var canonicalPayload = new
        {
            payload.PayloadId,
            payload.Dimension,
            payload.LeftMatrix,
            payload.RightMatrix
        };

        var json = JsonSerializer.Serialize(canonicalPayload, Options);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string ComputeFileHash(byte[] fileContents)
    {
        var bytes = SHA256.HashData(fileContents);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
