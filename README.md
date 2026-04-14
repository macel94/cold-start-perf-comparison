# cold-start-perf-comparison

Cross-cloud .NET 8 cold-start benchmark for:

- GCP Cloud Run (`europe-west1`)
- AWS Lambda (`eu-west-1`)
- Azure Container Apps (`westeurope`)
- Scaleway Serverless Containers (`fr-par`)

## v1 guardrails

- One shared benchmark app plus one thin AWS Lambda shim
- One shared sequential runner
- Exactly two payloads: `matrix-100x100` and `matrix-200x200`
- Fixed paths: `GET /api/startup` and `POST /api/compute/matrix`
- Uniform 15-minute idle window before every `intent: cold` step
- No auth and no provider-specific fields in the normalized result schema
- Warm-start optimizations must remain disabled: Cloud Run min instances, Azure min replicas, AWS provisioned concurrency, and equivalent Scaleway keep-warm settings

## Repository layout

```text
src/
  Benchmark.Contracts/
  BenchmarkApp/
  BenchmarkApp.AwsLambdaHost/
  BenchmarkRunner/
deploy/
workloads/
tests/
```

## Toolchain

- .NET SDK pin: `8.0.408` (roll-forward allowed within feature band)
- Runtime baseline: ASP.NET Core `8.0.14`

## Build and test

```bash
dotnet restore
dotnet build cold-start-perf-comparison.sln
dotnet test cold-start-perf-comparison.sln
```

## Run the shared benchmark app locally

```bash
dotnet run --project src/BenchmarkApp
```

## Run the benchmark runner

The runner uses `src/BenchmarkRunner/appsettings.json` for the provider catalog and writes normalized JSON envelopes to `benchmark-results/` by default.

```bash
dotnet run --project src/BenchmarkRunner -- \
  --workload workloads/v1/cross-cloud-sequential.json \
  --providers gcp-cloud-run \
  --output benchmark-results/local-gcp-run.json \
  --network-location-label local-dev
```

Each output envelope records:

- run metadata
- workload version and workload file hash
- benchmark app contract version and result schema version
- normalized raw records
- parity exceptions
- `p50`, `p95`, `p99`, `min`, and `max` summaries per provider and intent

## Provider deployment notes

- GCP Cloud Run: `deploy/gcp-cloud-run/`
- AWS Lambda: `deploy/aws-lambda/`
- Azure Container Apps: `deploy/azure-container-apps/`
- Scaleway Serverless Containers: `deploy/scaleway-serverless/`

Each folder contains:

- `descriptor.yaml` with canonical region, runtime pin, idle policy, resource settings, and parity notes
- native deployment manifest
- provider-specific README with operator steps

## Workload artifact

`workloads/v1/cross-cloud-sequential.json` is the only v1 workload. It contains the full fixed payload catalog and an explicit cold/warm execution sequence. Do not change the file between provider runs in the same benchmark session.

## Reproducibility notes

- Keep the runtime/toolchain pins unchanged when collecting benchmark data
- Record provider descriptor changes before publishing results
- AWS Lambda cold-start intent is inferred after the same 15-minute idle window and is always annotated with a parity exception in v1

## Quickstart

See `specs/001-cross-cloud-benchmark/quickstart.md` for the end-to-end single-provider flow.
