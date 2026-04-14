# Research: Cross-Cloud .NET Cold-Start Performance Benchmark

## Decision 1: Standardize on ASP.NET Core runtime 10.0.5 built with .NET SDK 10.0.201

- **Decision**: Use ASP.NET Core runtime `10.0.5` with .NET SDK `10.0.201` as the common runtime baseline for the shared benchmark app, runner, and AWS host adapter.
- **Rationale**: This pins an exact, reproducible runtime/toolchain combination while staying within the .NET 10 family that is broadly supported across container-based serverless platforms and AWS Lambda in 2026. It also keeps the benchmark fair by using the same runtime family everywhere.
- **Alternatives considered**:
  - **.NET 10 LTS**: rejected for v1 because provider rollout timing may vary during 2026.
  - **.NET 9 STS**: rejected because it shortens support life and adds upgrade churn during the benchmark project.

## Decision 2: Use one shared ASP.NET Core Minimal API benchmark app

- **Decision**: Model the benchmark target as one ASP.NET Core Minimal API app exposing identical paths on all providers, with only a thin AWS Lambda host shim for packaging.
- **Rationale**: This satisfies the single-app requirement while keeping the endpoint logic, payload handling, and response contracts identical across GCP Cloud Run, Azure Container Apps, and Scaleway Serverless Containers. AWS Lambda still needs an adapter layer, but the benchmark behavior remains centralized in one app codebase.
- **Alternatives considered**:
  - **Separate per-provider apps**: rejected because it would undermine fairness and duplicate logic.
  - **Pure Lambda function implementation for AWS only**: rejected because it would diverge from the shared ASP.NET contract and inflate cross-provider differences.

## Decision 3: Keep the benchmark runner as one sequential .NET CLI

- **Decision**: Implement the benchmark runner as one .NET CLI that executes the workload in exact file order with no parallelism.
- **Rationale**: Sequential execution is already fixed in the spec for v1 and eliminates concurrency noise when interpreting cold-start behavior. A single runner also reinforces fairness by ensuring the same timing, serialization, summary, and error-handling logic is used for all four providers.
- **Alternatives considered**:
  - **Provider-specific scripts**: rejected by FR-021.
  - **Concurrent execution**: rejected by FR-011 and A-004.

## Decision 4: Use a versioned JSON workload file with embedded payload catalog references

- **Decision**: Define the workload as a versioned JSON document that contains metadata, a fixed payload catalog, and an ordered list of workload steps referencing catalog entries by name.
- **Rationale**: JSON keeps parsing and hashing simple with built-in .NET serialization, which helps verify SC-001 and reduces early dependency load in a nearly empty repository. It also gives deterministic structure for contract tests and replay.
- **Alternatives considered**:
  - **YAML workload files**: rejected for v1 because they add parser complexity with little benefit at this scale.
  - **Code-defined workloads**: rejected because the workload must be versioned, shareable, and unchanged across provider runs.

## Decision 5: Document contracts with OpenAPI + JSON Schema

- **Decision**: Use one OpenAPI document for the benchmark app and JSON Schema documents for workload and result envelopes.
- **Rationale**: The project exposes both HTTP interfaces and structured data contracts. OpenAPI is the most readable and future-proof way to describe the shared endpoints, while JSON Schema is a good fit for validating the workload file and normalized results in contract tests.
- **Alternatives considered**:
  - **Markdown-only contracts**: rejected because they are less precise for automated validation.
  - **OpenAPI-only for everything**: rejected because workload/result files are not HTTP APIs.

## Decision 6: Use fixed v1 endpoints for cold and compute probes

- **Decision**: Reserve `GET /api/startup` for cold-start probes and `POST /api/compute/matrix` for compute probes.
- **Rationale**: The startup endpoint stays intentionally lightweight to isolate initialization latency from compute cost, matching A-006. The matrix compute endpoint isolates deterministic payload processing and correctness checks for the fixed 100x100 and 200x200 workloads.
- **Alternatives considered**:
  - **Single multipurpose endpoint**: rejected because it would blur cold-start and compute semantics.
  - **Cold-start probes against the compute endpoint**: rejected because it would conflate initialization and compute latency.

## Decision 7: Use provider observability checks plus parity exceptions for cold confirmation

- **Decision**: Before every cold step, always enforce the 15-minute idle window, then attempt provider-specific zero-state confirmation; if confirmation is unavailable or unsupported, record a parity exception rather than failing the run.
- **Rationale**: Cloud Run, Azure Container Apps, and Scaleway expose observable replica/instance state through monitoring or platform telemetry, while AWS Lambda does not offer deterministic public scale-to-zero confirmation. This preserves fairness and transparency without pretending the platforms expose identical guarantees.
- **Alternatives considered**:
  - **Blindly assume every 15-minute idle window produces a cold start**: rejected because the spec requires confirmation or explicit parity annotation.
  - **Abort runs on missing confirmation**: rejected by FR-016.

## Decision 8: Represent results as a normalized run envelope with raw records and summaries

- **Decision**: Emit one normalized run-level JSON result envelope containing run metadata, raw per-step records, parity exceptions, and summary metrics by provider and intent.
- **Rationale**: A single envelope makes it easy to archive, diff, validate, and publish results from a single benchmark session while still preserving raw evidence and summary aggregates in one place. It also supports the reproducibility and schema-normalization success criteria.
- **Alternatives considered**:
  - **CSV-only output**: rejected because it is weak for nested annotations and correctness detail.
  - **Separate metadata and results files only**: rejected because it complicates traceability in v1.

## Decision 9: Adopt xUnit-centered validation for the future implementation

- **Decision**: Use xUnit-based unit, integration, and contract tests once implementation begins.
- **Rationale**: xUnit is the default, well-supported testing approach in modern .NET projects and works well for CLI, HTTP, and schema-validation scenarios. It keeps the future implementation aligned with the selected .NET stack without introducing extra test runners.
- **Alternatives considered**:
  - **NUnit or MSTest**: rejected because they offer no clear benchmark-specific advantage here.

## Decision 10: Fix one European region-aligned location per provider

- **Decision**: Use `europe-west1` for GCP Cloud Run, `eu-west-1` for AWS Lambda, `westeurope` for Azure Container Apps, and `fr-par` for Scaleway Serverless Containers.
- **Rationale**: A Europe-centered region map reduces transcontinental variance, aligns naturally with the inclusion of a European provider, and gives one explicit canonical location per platform for reproducible v1 runs.
- **Alternatives considered**:
  - **Provider-default regions**: rejected because they would drift geographically and weaken comparability.
  - **Multiple regions per provider**: rejected because multi-region comparison is out of scope for v1.
