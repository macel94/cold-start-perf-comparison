# Implementation Plan: Cross-Cloud .NET Cold-Start Performance Benchmark

**Branch**: `001-cross-cloud-benchmark` | **Date**: 2026-04-14 | **Spec**: [`/specs/001-cross-cloud-benchmark/spec.md`](/home/runner/work/cold-start-perf-comparison/cold-start-perf-comparison/specs/001-cross-cloud-benchmark/spec.md)  
**Input**: Feature specification from `/specs/001-cross-cloud-benchmark/spec.md`

## Summary

Deliver a planning-first foundation for a reproducible v1 benchmark that compares cold-start and warm compute latency for the fixed provider set of GCP Cloud Run, AWS Lambda-hosted ASP.NET, Azure Container Apps, and Scaleway Serverless Containers. The implementation will use one shared .NET 8 benchmark app contract, one ordered workload definition, and one sequential benchmark runner that enforces a uniform 15-minute idle window before cold probes, uses only the fixed 100x100 and 200x200 matrix payloads, and produces normalized result records plus p50/p95/p99/min/max summaries.

## Technical Context

**Language/Version**: C# / .NET 8 LTS for app, hosting adapters, and runner  
**Primary Dependencies**: ASP.NET Core Minimal API, System.Text.Json, HttpClient, xUnit, FluentAssertions, Amazon.Lambda.AspNetCoreServer (AWS host adapter only), provider CLI/API integrations for deployment-state checks  
**Storage**: Versioned JSON/YAML contracts in repo; benchmark outputs written as structured JSON files; no database in v1  
**Testing**: xUnit for unit/integration/contract tests; schema validation tests for workload and results; doc-driven smoke tests for reproducibility  
**Target Platform**: Linux-based .NET workloads deployed to Cloud Run, AWS Lambda HTTP endpoint, Azure Container Apps, and Scaleway Serverless Containers; single operator-run CLI from a stable workstation/runner host  
**Project Type**: CLI + web-service benchmark harness with deployment configuration and documentation  
**Performance Goals**: Capture reproducible end-to-end latency for cold and warm intents across all four providers; emit per-provider/per-intent p50/p95/p99/min/max summaries and correctness outcomes for each step  
**Constraints**: Exactly four providers; one benchmark app contract; one workload file version per session; sequential execution only; 15-minute uniform idle window before every cold step; payload catalog fixed to 100x100 and 200x200 matrices; identical URL path structure; no auth in v1; parity exceptions recorded instead of failing the run  
**Scale/Scope**: Planning-first repository; initial deliverables are design artifacts, contracts, and repository structure for one shared app, one AWS-specific host shim, one runner, fixed deployment descriptors, and test scaffolding

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The current constitution file is still the default placeholder template and does not define enforceable project-specific principles, gates, or prohibitions yet.

### Pre-Phase-0 Gate Result

- **Status**: PASS
- **Reason**: No actionable constitutional constraints are defined beyond the requirement to document and justify the work.
- **Planning guardrails applied anyway**:
  - Keep scope tightly aligned with the approved spec.
  - Prefer the smallest viable structure because the repository is nearly empty.
  - Preserve a single benchmark app, single workload definition, and single runner.
  - Record provider-specific fairness caveats as parity exceptions instead of hidden implementation assumptions.

### Post-Phase-1 Re-Check

- **Status**: PASS
- **Reason**: Design artifacts stay within the spec-bounded v1 scope and do not introduce extra providers, concurrency modes, or unnecessary services.

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
└── scaleway-serverless/

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

- Common runtime selected: **.NET 8 LTS** for widest stable support across all four providers in 2026.
- HTTP benchmark app contract selected: **ASP.NET Core Minimal API** with identical `/api/startup` and `/api/compute/matrix` paths on every provider.
- AWS hosting pattern selected: **shared ASP.NET app + Amazon.Lambda.AspNetCoreServer adapter** behind a single HTTP entry point.
- Workload/result contract strategy selected: **OpenAPI for HTTP endpoints + JSON Schema for workload/results**.
- Cold-start fairness strategy selected: **uniform 15-minute idle window plus provider observability checks where available; annotate parity exceptions when zero-state cannot be confirmed**.

## Phase 1 Design Summary

- Defined normalized entities for providers, workload steps, payload catalog entries, benchmark runs, result records, summaries, and parity exceptions.
- Defined external contracts for the benchmark app, workload file, and result envelope.
- Produced a documentation-first quickstart that fits the current nearly empty repository while giving direct inputs for task generation.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Thin AWS-specific host project | Lambda HTTP hosting needs an adapter entry point that container-first platforms do not require | A single binary with no host shim would not support AWS Lambda’s event model while preserving one shared app codebase |
