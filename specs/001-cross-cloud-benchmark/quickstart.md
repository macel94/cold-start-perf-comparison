# Quickstart: Cross-Cloud .NET Cold-Start Performance Benchmark

## Purpose

This quickstart is the operator-facing path for deploying one provider and running one benchmark session from repository artifacts alone.

## Inputs

- Root README: `/README.md`
- Provider descriptors in `/deploy/*/descriptor.yaml`
- Provider Terraform stacks in `/deploy/*/{versions.tf,variables.tf,main.tf,outputs.tf}`
- Workload artifact: `/workloads/v1/cross-cloud-sequential.json`
- Contracts:
  - `/specs/001-cross-cloud-benchmark/contracts/benchmark-app.openapi.yaml`
  - `/specs/001-cross-cloud-benchmark/contracts/workload.schema.json`
  - `/specs/001-cross-cloud-benchmark/contracts/results.schema.json`

## v1 Non-Negotiables

- Provider set is fixed: GCP Cloud Run, AWS Lambda-hosted ASP.NET endpoint, Azure Container Apps, Scaleway Serverless Containers, and Unikraft/KraftCloud.
- Provider regions are fixed for v1: GCP=`europe-west1`, AWS=`eu-west-1`, Azure=`westeurope`, Scaleway=`fr-par`, Unikraft=`fra`.
- Use one shared .NET benchmark app contract.
- Pin ASP.NET Core runtime `10.0.5` and .NET SDK `10.0.201`.
- Use one workload definition and one sequential runner.
- Record workload version, benchmark app contract version, and result schema version in every benchmark run envelope.
- Apply the same 15-minute idle window before every cold step.
- Use only the `100×100` and `200×200` payloads.
- Publish summary metrics as `p50`, `p95`, `p99`, `min`, and `max` for each provider and each intent category.
- Disable or leave unset warm-start optimizations such as Cloud Run min instances, Azure Container Apps minimum replicas, AWS provisioned concurrency, Scaleway keep-warm settings, and Unikraft/KraftCloud stateful scale-to-zero snapshots.
- Document benchmark resource and billing-affecting settings in each provider descriptor and deployment README.

## Single-provider benchmark flow

1. **Restore, build, and test**

   ```bash
   dotnet restore
   dotnet build cold-start-perf-comparison.sln
   dotnet test cold-start-perf-comparison.sln
   ```

2. **Choose a provider**

   Use exactly one of the canonical v1 targets:

    - GCP Cloud Run: `deploy/gcp-cloud-run/`
    - AWS Lambda: `deploy/aws-lambda/`
    - Azure Container Apps: `deploy/azure-container-apps/`
    - Scaleway Serverless Containers: `deploy/scaleway-serverless/`
    - Unikraft/KraftCloud: `deploy/unikraft-kraftcloud/`

3. **Keep warm-start optimization disabled**

    - Cloud Run `minInstanceCount` must be `0`
    - Azure `minReplicas` must be `0`
    - AWS provisioned concurrency must remain disabled
    - Scaleway keep-warm settings must remain disabled
    - Unikraft/KraftCloud documents scale-to-zero by default; stateful snapshots must remain disabled for baseline parity

4. **Prepare the deployment input**

   - For GCP, Azure, and Scaleway: publish the shared benchmark app image and set the `image` variable in the chosen provider's `terraform.tfvars`
   - For AWS Lambda: run `dotnet publish src/BenchmarkApp.AwsLambdaHost/BenchmarkApp.AwsLambdaHost.csproj -c Release`
   - For Unikraft/KraftCloud: build and publish the benchmark image from the official `.NET 10` HTTP server workflow, then set the `image` variable

5. **Deploy with Terraform**

   ```bash
   cp deploy/gcp-cloud-run/terraform.tfvars.example deploy/gcp-cloud-run/terraform.tfvars
   terraform -chdir=deploy/gcp-cloud-run init
   terraform -chdir=deploy/gcp-cloud-run plan -out tfplan
   terraform -chdir=deploy/gcp-cloud-run apply tfplan
   ```

   Swap `deploy/gcp-cloud-run` for the provider you chose in step 2 and edit `terraform.tfvars` before `plan`.

6. **Capture the deployed base URL**

   ```bash
   terraform -chdir=deploy/gcp-cloud-run output -raw service_url
   ```

   AWS Lambda exposes `api_base_url`; the other providers expose `service_url`.

7. **Update the provider base URL**

   Set the deployed provider URL in `src/BenchmarkRunner/appsettings.json` for the target provider or override via environment-managed config.

8. **Run one benchmark session**

   ```bash
   dotnet run --project src/BenchmarkRunner -- \
     --workload workloads/v1/cross-cloud-sequential.json \
     --providers gcp-cloud-run \
     --output benchmark-results/gcp-single-run.json \
     --network-location-label workstation-eu
   ```

9. **Validate the result envelope**

   Confirm the output JSON contains:

   - `workloadVersion`
   - `workloadFileHash`
   - `apiContractVersion`
   - `resultSchemaVersion`
   - `records`
   - `parityExceptions`
   - `summaryMetrics`

10. **Check cold-step parity**

    For every `intent: cold` record:

    - `scaleToZeroConfirmed` must exist
    - if it is `false`, `annotationRefs` must reference a parity exception
    - AWS Lambda is expected to produce a parity exception in v1

## Expected output checklist

- Source layout matches `plan.md`
- One workload file exists and validates
- Runner executes steps sequentially only
- Summary metrics include `p50`, `p95`, `p99`, `min`, and `max`
- Provider parity caveats are explicit in docs and results
