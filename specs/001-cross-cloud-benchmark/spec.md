# Feature Specification: Cross-Cloud .NET Cold-Start Performance Benchmark

**Feature Branch**: `001-cross-cloud-benchmark`  
**Created**: 2026-04-14  
**Status**: Approved  
**Input**: User description: "Implement planning artifacts for a .NET Web API cold-start performance comparison benchmark across GCP Cloud Run, AWS Lambda, Azure Container Apps, Scaleway, and Unikraft/KraftCloud."

## Clarifications

### Session 2026-04-14

- Q: What idle window duration should be used before firing cold-start probes in v1? → A: 15 minutes, identical for all providers in v1.
- Q: What fixed matrix payload dimensions should the payload catalog define in v1? → A: 100×100 and 200×200.
- Q: Which summary metrics must the benchmark output include? → A: p50, p95, p99, min, and max.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Run a Reproducible Cross-Provider Cold-Start Benchmark (Priority: P1)

A benchmark operator wants to execute a standardised, reproducible benchmark run against all five cloud providers in a single session and receive a set of structured results that are directly comparable across providers.

**Why this priority**: This is the core deliverable of the project. All other stories depend on having a working, fair, end-to-end benchmark run. Without this story there is no usable output.

**Independent Test**: Can be fully tested by running the benchmark workload against any single provider using the versioned workload file and verifying that a structured result set is produced with provider, region, step ID, latency, status, and cold/warm intent fields.

**Acceptance Scenarios**:

1. **Given** a benchmark operator has deployed all five provider benchmark apps, **When** they execute the benchmark runner with the versioned workload file, **Then** each provider returns results for every workload step in the defined order, containing all required result fields.
2. **Given** the same versioned workload file is used for all five providers, **When** benchmark results are collected, **Then** the step IDs and payload contents are identical across all five providers' result sets.
3. **Given** a benchmark run has completed, **When** the operator inspects the results, **Then** cold-start steps and warm/compute steps are clearly identified and separated in the output.
4. **Given** a result schema is defined, **When** any provider emits a result record, **Then** the record conforms to the normalised result schema without provider-specific extensions.

---

### User Story 2 — Enforce and Confirm Cold-Start Measurement (Priority: P2)

A benchmark analyst wants to be certain that the cold-start probe steps genuinely measure a cold start — not a warm container resume — so that the reported cold-start latency figures are trustworthy.

**Why this priority**: Cold-start measurement is the primary differentiating metric. Inaccurate cold-start capture renders the entire comparison meaningless. This must be addressed before any results are published.

**Independent Test**: Can be tested independently by verifying that, for a single provider, the 15-minute idle window elapses and a provider reset signal (teardown/scale-to-zero confirmation) is observed before the cold-start probe request is sent, and the result is labelled `cold`.

**Acceptance Scenarios**:

1. **Given** a uniform 15-minute idle window is configured for v1, **When** that idle window expires after the last request, **Then** the provider's compute instance is confirmed to have scaled to zero or been discarded before the cold-start probe is fired.
2. **Given** a cold-start probe request is sent, **When** the response is received, **Then** the result record carries `intent: cold` and the measured latency includes the full initialisation time.
3. **Given** provider documentation states a scale-to-zero behaviour that differs from the standard idle window, **When** the benchmark is run, **Then** that provider-specific caveat and any parity exception are recorded alongside its result set.
4. **Given** a provider does not support true scale-to-zero, **When** a cold-start run is attempted, **Then** the step is marked with a parity exception and the results note explains the deviation.

---

### User Story 3 — Execute and Capture Compute-Probe Results (Priority: P3)

A benchmark operator wants to run a fixed-payload compute probe (matrix multiplication) against each provider and capture structured results that allow comparison of sustained compute throughput under warm-instance conditions.

**Why this priority**: The compute probe is the secondary measurement track and is only meaningful after cold-start behaviour is validated. It provides context for interpreting cold-start overhead relative to total compute cost.

**Independent Test**: Can be tested independently by sending a fixed-payload POST request to a single provider's compute endpoint and verifying the response matches the expected result shape, matrix output, and result-record fields without stopping the run on malformed payload errors.

**Acceptance Scenarios**:

1. **Given** the benchmark runner sends a compute-probe step with a fixed matrix payload, **When** the provider responds, **Then** the result record carries `intent: warm`, the step ID, provider, region, latency, and status.
2. **Given** the same 100×100 and 200×200 matrix payloads are used across all providers, **When** results are collected, **Then** compute-probe latency figures are directly comparable without normalisation.
3. **Given** a provider's compute endpoint receives a malformed or oversized payload, **When** the request is processed, **Then** the result is recorded as an error step with status and error detail, and the benchmark run continues.

---

### User Story 4 — Reproduce Benchmark from Documentation Alone (Priority: P4)

A third-party reviewer wants to independently reproduce the benchmark by following the project documentation without any undocumented tribal knowledge.

**Why this priority**: Reproducibility from documentation is a stated success criterion and is required for peer review and credibility of published results.

**Independent Test**: Can be tested by a team member unfamiliar with the codebase following only the written documentation to deploy a single provider benchmark app and execute a complete run, obtaining results that match the reference run within documented tolerance.

**Acceptance Scenarios**:

1. **Given** only the project documentation is available, **When** a reviewer follows the deployment guide for any single provider, **Then** they can deploy the benchmark app and execute the workload without requesting additional information.
2. **Given** the versioned workload file and result schema are published, **When** a reviewer runs the benchmark, **Then** the results can be validated against the reference output structure without access to internal tooling.

---

### Edge Cases

- What happens when a provider fails to scale to zero within the configured idle window? (The benchmark should log a timeout, mark the step as a parity exception, and continue rather than blocking the entire run.)
- What happens when a cold-start probe request times out? (The result should be recorded as a failed cold-start step with the timeout as latency upper bound; the run must not halt.)
- What happens when the compute endpoint returns an incorrect matrix result? (The result schema must capture correctness status separately from latency so errors are detectable without disrupting benchmark flow.)
- What happens if the same workload file version is run twice in the same session? (Each run should produce a distinct result set identified by a unique run ID and timestamp, preserving both sets.)
- What happens when one provider's region is unavailable? (The benchmark run for that provider is skipped or aborted for that region; other providers are unaffected.)
- How are sub-millisecond clock differences between runner and provider handled? (Latency is always measured end-to-end at the runner, not inside the provider, ensuring a consistent measurement point.)

## Requirements *(mandatory)*

### Functional Requirements

**Benchmark Scope and Providers**

- **FR-001**: The benchmark MUST cover exactly five cloud providers in v1: GCP Cloud Run, AWS Lambda (ASP.NET-hosted endpoint), Azure Container Apps, Scaleway Serverless Containers, and Unikraft/KraftCloud.
- **FR-002**: Each provider MUST have exactly one documented benchmark region for v1; cross-region comparisons are out of scope. The canonical v1 regions are `europe-west1` for GCP Cloud Run, `eu-west-1` for AWS Lambda, `westeurope` for Azure Container Apps, `fr-par` for Scaleway Serverless Containers, and `fra` for Unikraft/KraftCloud.
- **FR-003**: Each provider MUST have a dedicated benchmark application deployment that is independent of the other providers.

**Benchmark API Contract**

- **FR-004**: Every benchmark app MUST expose a lightweight startup/readiness endpoint that returns a success response with minimal compute overhead, used as the cold-start probe target.
- **FR-005**: Every benchmark app MUST expose a POST endpoint that accepts a fixed matrix multiplication payload and returns the computation result, used as the compute probe target.
- **FR-006**: All five benchmark apps MUST expose endpoints at the same URL path structure so the benchmark runner can target any provider by changing only the base URL.
- **FR-007**: The .NET runtime version used MUST be identical across all five provider deployments; the exact v1 baseline is ASP.NET Core runtime `10.0.5`, built with .NET SDK `10.0.201`, and any later change MUST be versioned and documented before benchmark results are published.

**Workload Definition**

- **FR-008**: The benchmark workload MUST be defined in a versioned workload file that specifies each step in an exact, ordered sequence.
- **FR-009**: The workload file MUST include, for each step: a unique step ID, the endpoint target (startup or compute), the request payload (or a reference to a named payload from the payload catalog), and the declared intent (`cold` or `warm`).
- **FR-010**: The same workload file version MUST be used for all five provider runs within a single benchmark session; the file MUST NOT be modified between provider runs.
- **FR-011**: The benchmark runner MUST execute workload steps in the exact order defined in the workload file, without reordering or parallelising steps (single-request sequential execution in v1).
- **FR-012**: The request payload catalog MUST be a documented, fixed set of named payloads; the same payload catalog MUST be used across all providers.

**Cold-Start Measurement**

- **FR-013**: Before any workload step marked `intent: cold`, the benchmark runner MUST enforce a 15-minute idle window for all providers in v1 to allow the provider to scale to zero; any inability to apply or confirm that window MUST be recorded as a parity exception.
- **FR-014**: The benchmark runner MUST record, for each cold-start step, whether scale-to-zero was confirmed before the probe was sent or whether a parity exception applies.
- **FR-015**: Each provider's scale-to-zero behaviour, idle policy, and any known cold-start caveats MUST be documented; deviations from standard idle-window behaviour MUST be marked as parity exceptions.
- **FR-016**: Parity exceptions MUST NOT cause a benchmark run to fail; they MUST be recorded as annotations in the result set and noted in the run metadata.

**Result Schema**

- **FR-017**: Every result record MUST include: provider identifier, region, step ID, end-to-end latency (measured at the runner), HTTP status code, and cold/warm intent.
- **FR-018**: The result schema MUST be identical across all five providers; provider-specific fields are not permitted in the normalised schema (they may appear in a separate annotations block).
- **FR-019**: The benchmark run MUST produce a run metadata record that includes: run ID, timestamp, workload file version, benchmark app contract version, result schema version, and the list of providers included in the run.
- **FR-020**: Matrix computation results (actual output values) MUST be captured in the result record to allow correctness verification separate from latency measurement.

**Fairness Constraints**

- **FR-021**: The benchmark runner MUST be a single shared tool used identically for all five providers; provider-specific runner scripts are not permitted in v1.
- **FR-022**: The minimum idle period before every cold-start probe MUST be 15 minutes and MUST be applied identically to all providers in v1; provider-specific idle-window overrides are out of scope for v1.
- **FR-023**: The payload catalog MUST define exactly two fixed matrix payloads in v1: 100×100 and 200×200; those same payload definitions MUST be used across all providers.

**Summary Metrics**

- **FR-024**: Benchmark summary output MUST include p50, p95, p99, min, and max latency for each provider and each intent category (`cold` and `warm`).

### Key Entities

- **Provider Deployment**: A benchmark app instance hosted on a specific cloud provider in a specific region. Has attributes: provider identifier, region, base URL, .NET runtime version, scale-to-zero capability, idle window duration (15 minutes in v1), parity exception flag.
- **Workload File**: A versioned, ordered list of benchmark steps. Has a version identifier and an ordered sequence of steps.
- **Workload Step**: A single instruction in the workload file. Has: step ID, endpoint type (startup or compute), payload reference, and declared intent (cold or warm).
- **Payload Catalog**: A fixed, named set of request payloads. Each entry has a name, matrix dimensions (100×100 or 200×200 in v1), and a byte representation.
- **Benchmark Run**: A single execution of the full workload against one or more providers. Has: run ID, timestamp, workload file version, list of participating providers, and collected result records.
- **Result Record**: A single measured outcome for one workload step against one provider. Has: provider identifier, region, step ID, latency (ms), HTTP status, cold/warm intent, computation result (for compute steps), and optional annotations.
- **Parity Exception**: A documented deviation from the standard cold-start measurement protocol for a specific provider. Has: provider, exception type, description, and impact on result interpretation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The identical workload file version is used, without modification, across all five providers in a benchmark session — verifiable by comparing the workload file hash recorded in each run metadata record.
- **SC-002**: Cold-start result records and warm/compute result records are unambiguously separated in the output; a reviewer can filter each category without inspecting raw request logs.
- **SC-003**: All five providers emit result records that conform to the same normalised schema, such that a single result-processing workflow can consume all five result sets without provider-specific transformation.
- **SC-004**: A third party can reproduce a complete benchmark run against any single provider by following only the project documentation, without requiring additional guidance from the original authors.
- **SC-005**: Every cold-start probe step is preceded by a confirmed or documented idle period; no result record labelled `intent: cold` is produced from a request sent to a warm instance without a parity exception annotation.
- **SC-006**: Parity exceptions, if any, are fully documented before benchmark results are published, including the provider, the nature of the exception, and its impact on result comparability.
- **SC-007**: The benchmark runner completes a full five-provider run without operator intervention beyond initial configuration; any failure is recorded in the result set and does not require a manual restart of the entire run.
- **SC-008**: The published summary for each benchmark run includes p50, p95, p99, min, and max latency for every provider and for both `cold` and `warm` intent categories.

## Assumptions

- **A-001**: AWS Lambda with an ASP.NET-compatible hosting adapter is the AWS hosting path for v1, as stated in the feature request. Alternative AWS compute options (e.g., ECS Fargate, App Runner) are out of scope for v1.
- **A-002**: Scaleway Serverless Containers remains in v1 from the original European-provider requirement, and Unikraft/KraftCloud is additionally included because official `.NET 10` and scale-to-zero guidance is available. Hetzner is out of scope for v1.
- **A-003**: Each provider will be deployed to a single, documented region; multi-region or multi-zone deployments are out of scope for v1. The canonical region map is GCP Cloud Run=`europe-west1`, AWS Lambda=`eu-west-1`, Azure Container Apps=`westeurope`, Scaleway Serverless Containers=`fr-par`, and Unikraft/KraftCloud=`fra`.
- **A-004**: The benchmark workload is executed sequentially (one request at a time, no concurrent requests) in v1. Concurrency experiments are deferred to a future version.
- **A-005**: The benchmark runner operates from a single stable network location; network variance between the runner and providers is not controlled in v1 but is documented as a known source of variance.
- **A-006**: The cold-start probe target is the lightweight startup/readiness endpoint, not the compute endpoint. This avoids conflating initialisation latency with compute latency in cold-start measurements.
- **A-007**: Latency is measured end-to-end at the benchmark runner (wall-clock time from request send to response received), not reported by the provider's internal instrumentation.
- **A-008**: Each provider's billing and resource configuration for the benchmark app is documented but is not standardised to be cost-equivalent; the comparison is on measured latency, not cost.
- **A-009**: The benchmark is a developer/research tool; it does not require authentication, authorisation, or access control for the benchmark endpoints in v1.
- **A-010**: Platform-level cold-start optimisations (e.g., provisioned concurrency, min-instances) are disabled or not configured for the benchmark deployments, to measure baseline cold-start behaviour.
- **A-011**: The exact v1 runtime/toolchain baseline is ASP.NET Core runtime `10.0.5` with .NET SDK `10.0.201`, pinned in repository configuration and mirrored in deployment manifests.
