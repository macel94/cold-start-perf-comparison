# Implementation Plan: Cross-Cloud .NET Cold-Start Performance Benchmark

**Branch**: `001-cross-cloud-benchmark` | **Date**: 2026-04-14 | **Spec**: [`/specs/001-cross-cloud-benchmark/spec.md`](/home/runner/work/cold-start-perf-comparison/cold-start-perf-comparison/specs/001-cross-cloud-benchmark/spec.md)  
**Input**: Feature specification from `/specs/001-cross-cloud-benchmark/spec.md`

## Summary

Deliver a planning-first foundation for a reproducible v1 benchmark that compares cold-start and warm compute latency for the fixed provider set of GCP Cloud Run, AWS Lambda-hosted ASP.NET, Azure Container Apps, Scaleway Serverless Containers, and Unikraft/KraftCloud. The implementation will use one shared ASP.NET Core runtime `10.0.5` benchmark app contract built with .NET SDK `10.0.201`, one ordered workload definition, and one sequential benchmark runner that enforces a uniform 15-minute idle window before cold probes, uses only the fixed 100x100 and 200x200 matrix payloads, and produces normalized result records plus p50/p95/p99/min/max summaries.

## Technical Context

**Language/Version**: C# with ASP.NET Core runtime `10.0.5` and .NET SDK `10.0.201`  
**Primary Dependencies**: ASP.NET Core Minimal API, System.Text.Json, HttpClient, xUnit, FluentAssertions, Amazon.Lambda.AspNetCoreServer (AWS host adapter only), provider CLI/API integrations for deployment-state checks  
**Storage**: Versioned JSON/YAML contracts in repo; benchmark outputs written as structured JSON files; no database in v1  
**Testing**: xUnit for unit/integration/contract tests; schema validation tests for workload and results; doc-driven smoke tests for reproducibility  
**Target Platform**: Linux-based .NET workloads deployed to Cloud Run (`europe-west1`), AWS Lambda HTTP endpoint (`eu-west-1`), Azure Container Apps (`westeurope`), Scaleway Serverless Containers (`fr-par`), and Unikraft/KraftCloud (`fra`); single operator-run CLI from a stable workstation/runner host  
**Project Type**: CLI + web-service benchmark harness with deployment configuration and documentation  
**Performance Goals**: Capture reproducible end-to-end latency for cold and warm intents across all five providers; emit per-provider/per-intent p50/p95/p99/min/max summaries and correctness outcomes for each step  
**Constraints**: Exactly five providers; one benchmark app contract; one workload file version per session; explicit benchmark app contract and result schema versions recorded in run metadata; sequential execution only; 15-minute uniform idle window before every cold step; payload catalog fixed to 100x100 and 200x200 matrices; identical URL path structure; no auth in v1; parity exceptions recorded instead of failing the run; runtime/toolchain pinned to ASP.NET Core `10.0.5` and SDK `10.0.201`; regions fixed to `europe-west1`, `eu-west-1`, `westeurope`, `fr-par`, and `fra`  
**Scale/Scope**: Planning-first repository; initial deliverables are design artifacts, contracts, and repository structure for one shared app, one AWS-specific host shim, one runner, fixed deployment descriptors, and test scaffolding

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The constitution is ratified at `.specify/memory/constitution.md` version `1.0.0` and defines five enforceable principles: cross-cloud fairness, reproducibility and explicit versioning, contract-first development, transparency of parity exceptions, and minimal v1 scope.

### Pre-Phase-0 Gate Result

- **Status**: PASS
- **Reason**:
  - The plan preserves one shared benchmark app, one shared runner, one ordered workload definition, one measurement point at the runner, one 15-minute cold-step idle window, and one canonical region per provider.
  - Runtime/toolchain pins, workload/version pins, and canonical provider regions are explicitly documented.
  - Contracts are defined before implementation in OpenAPI and JSON Schema artifacts.
  - Provider-specific behavior is limited to packaging/evidence collection and is documented as parity metadata instead of benchmark-semantic drift.
  - Scope remains within the approved v1 boundaries of five providers, two payload sizes, sequential execution, and no auth.
- **Planning guardrails applied anyway**:
  - Keep scope tightly aligned with the approved spec.
  - Prefer the smallest viable structure because the repository is nearly empty.
  - Preserve a single benchmark app, single workload definition, and single runner.
  - Record provider-specific fairness caveats as parity exceptions instead of hidden implementation assumptions.

### Post-Phase-1 Re-Check

- **Status**: PASS
- **Reason**: Design artifacts stay within the constitution and spec-bounded v1 scope, keep contracts ahead of implementation, preserve reproducibility pins, and route platform differences into transparent parity annotations instead of divergent benchmark behavior.

## Project Structure

### Documentation (this feature)

```text
specs/001-cross-cloud-benchmark/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── benchmark-app.openapi.yaml
│   ├── workload.schema.json
│   └── results.schema.json
└── tasks.md              # Created later by /speckit.tasks
```

### Source Code (repository root)

```text
src/
├── BenchmarkApp/                 # Shared ASP.NET Core benchmark endpoints and contracts
├── BenchmarkApp.AwsLambdaHost/   # Thin AWS Lambda adapter over the shared app
├── BenchmarkRunner/              # Single CLI runner for workload execution and summary generation
└── Benchmark.Contracts/          # Shared DTOs/schema helpers used by app and runner

deploy/
├── gcp-cloud-run/
├── aws-lambda/
├── azure-container-apps/
├── scaleway-serverless/
└── unikraft-kraftcloud/

workloads/
└── v1/
    └── cross-cloud-sequential.json

tests/
├── contract/
├── integration/
└── unit/
```

**Structure Decision**: Use a single backend-oriented repository layout with one shared benchmark application, one thin AWS host adapter, one runner, and provider-specific deployment directories. This keeps the logical application count at one while leaving enough separation to package the same app for Lambda and for container-based providers without fragmenting the benchmark logic.

## Phase 0 Research Summary

- Common runtime selected: **ASP.NET Core runtime `10.0.5` built with .NET SDK `10.0.201`** for widest stable support across all five providers in 2026.
- HTTP benchmark app contract selected: **ASP.NET Core Minimal API** with identical `/api/startup` and `/api/compute/matrix` paths on every provider.
- AWS hosting pattern selected: **shared ASP.NET app + Amazon.Lambda.AspNetCoreServer adapter** behind a single HTTP entry point.
- Workload/result contract strategy selected: **OpenAPI for HTTP endpoints + JSON Schema for workload/results**.
- Cold-start fairness strategy selected: **uniform 15-minute idle window plus provider observability checks where available; annotate parity exceptions when zero-state cannot be confirmed**.

## Canonical v1 Provider Map

| Provider | Deployment Target | Region | Runtime Baseline |
|----------|-------------------|--------|------------------|
| GCP | Cloud Run | `europe-west1` | ASP.NET Core `10.0.5` |
| AWS | Lambda-hosted ASP.NET endpoint | `eu-west-1` | ASP.NET Core `10.0.5` |
| Azure | Container Apps | `westeurope` | ASP.NET Core `10.0.5` |
| Scaleway | Serverless Containers | `fr-par` | ASP.NET Core `10.0.5` |
| Unikraft | KraftCloud | `fra` | ASP.NET Core `10.0.5` |

## Phase 1 Design Summary

- Defined normalized entities for providers, workload steps, payload catalog entries, benchmark runs, result records, summaries, and parity exceptions.
- Defined external contracts for the benchmark app, workload file, and result envelope.
- Produced a documentation-first quickstart that fits the current nearly empty repository while giving direct inputs for task generation.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Thin AWS-specific host project | Lambda HTTP hosting needs an adapter entry point that container-first platforms do not require | A single binary with no host shim would not support AWS Lambda’s event model while preserving one shared app codebase |
