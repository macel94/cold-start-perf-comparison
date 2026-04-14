# Quickstart: Cross-Cloud .NET Cold-Start Performance Benchmark

## Purpose

This quickstart is for the next implementation phase. The repository is still planning-first, so these steps focus on turning the approved spec and design artifacts into actionable engineering work without introducing source code yet.

## Inputs

- Feature spec: `/specs/001-cross-cloud-benchmark/spec.md`
- Implementation plan: `/specs/001-cross-cloud-benchmark/plan.md`
- Research decisions: `/specs/001-cross-cloud-benchmark/research.md`
- Data model: `/specs/001-cross-cloud-benchmark/data-model.md`
- Contracts:
  - `/specs/001-cross-cloud-benchmark/contracts/benchmark-app.openapi.yaml`
  - `/specs/001-cross-cloud-benchmark/contracts/workload.schema.json`
  - `/specs/001-cross-cloud-benchmark/contracts/results.schema.json`

## v1 Non-Negotiables

- Provider set is fixed: GCP Cloud Run, AWS Lambda-hosted ASP.NET endpoint, Azure Container Apps, Scaleway Serverless Containers.
- Provider regions are fixed for v1: GCP=`europe-west1`, AWS=`eu-west-1`, Azure=`westeurope`, Scaleway=`fr-par`.
- Use one shared .NET benchmark app contract.
- Pin ASP.NET Core runtime `8.0.14` and .NET SDK `8.0.408`.
- Use one workload definition and one sequential runner.
- Record workload version, benchmark app contract version, and result schema version in every benchmark run envelope.
- Apply the same 15-minute idle window before every cold step.
- Use only the `100×100` and `200×200` payloads.
- Publish summary metrics as `p50`, `p95`, `p99`, `min`, and `max` for each provider and each intent category.
- Disable or leave unset warm-start optimizations such as Cloud Run min instances, Azure Container Apps minimum replicas, AWS provisioned concurrency, and equivalent Scaleway keep-warm settings.
- Document benchmark resource and billing-affecting settings in each provider descriptor and deployment README.

## Recommended Implementation Order

1. **Generate tasks**
   - Run `/speckit.tasks` for feature `001-cross-cloud-benchmark`.
   - Keep the first task slice documentation-and-contract driven.

2. **Scaffold repository structure**
   - Create `src/`, `tests/`, `deploy/`, and `workloads/` exactly as defined in `plan.md`.
   - Keep `BenchmarkApp` as the single source of endpoint behavior.
   - Add only a thin `BenchmarkApp.AwsLambdaHost` shim for Lambda packaging.

3. **Lock contracts first**
   - Treat the OpenAPI and JSON Schema files as the source of truth.
   - Ensure endpoint paths stay exactly:
     - `GET /api/startup`
     - `POST /api/compute/matrix`
    - Keep workload/result shapes backward-compatible with the approved v1 contracts.
    - Enforce unique workload step IDs, contiguous sequence numbers, and the fixed two-entry payload catalog in the custom workload validator as well as in schema-level checks.

4. **Create the first workload artifact**
    - Add `workloads/v1/cross-cloud-sequential.json`.
    - Include exactly two payload catalog entries: `matrix-100x100` and `matrix-200x200`.
    - Encode the ordered cold/warm steps explicitly, including compute steps from the start; do not derive them dynamically.

5. **Implement runner orchestration**
   - Read the workload file.
   - Execute steps in file order only.
   - Before each cold step:
     - wait 15 minutes,
     - query provider-specific scale evidence where supported,
     - attach a parity exception if zero-state cannot be confirmed.

6. **Implement result normalization**
   - Emit one run envelope matching `results.schema.json`.
   - Record raw step results before computing summary metrics.
   - Compute summary metrics per `(provider, intent)` slice only after all records are collected.

7. **Add validation**
   - Contract tests: verify app responses and result payloads against the documented schemas.
   - Integration tests: verify runner sequencing, idle-window handling, and parity-exception recording.
   - Documentation smoke test: follow docs to configure one provider end-to-end.

## Suggested Milestones

### Milestone 1: Contracts and skeletons

- Contracts reviewed and frozen
- Repository structure scaffolded
- Initial workload file added
- Test projects created

### Milestone 2: Shared benchmark app + runner

- Startup and compute endpoints implemented
- Runner executes sequential workload locally
- Result envelope generation implemented

### Milestone 3: Provider packaging

- Benchmark metadata descriptors added for all four providers
- Cloud Run container deployment path added
- Azure Container Apps deployment path added
- Scaleway Serverless Containers deployment path added
- AWS Lambda host adapter added

### Milestone 4: Benchmark reproducibility

- Provider configuration docs written
- One-region-per-provider values documented exactly as GCP=`europe-west1`, AWS=`eu-west-1`, Azure=`westeurope`, Scaleway=`fr-par`
- Reproducible run captured with summary metrics and parity annotations

## Definition of Done for the first implementation slice

- Source layout matches `plan.md`.
- Contracts are unchanged or intentionally versioned.
- One workload file exists and validates.
- Runner design still enforces sequential execution and the uniform 15-minute cold-step idle window.
- No extra providers, payload sizes, or concurrency modes are introduced.
