# Data Model: Cross-Cloud .NET Cold-Start Performance Benchmark

## Overview

The v1 model centers on one versioned workload, one shared payload catalog, four fixed provider deployments, and one normalized run envelope that stores raw step results, parity exceptions, and summary metrics.

## Entities

### 1. ProviderDeployment

Represents one benchmark target deployment for one provider in one region.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `providerId` | string | yes | Fixed enum: `gcp-cloud-run`, `aws-lambda`, `azure-container-apps`, `scaleway-serverless-containers` |
| `displayName` | string | yes | Human-readable provider name |
| `region` | string | yes | Single documented region for v1 (`europe-west1`, `eu-west-1`, `westeurope`, or `fr-par`) |
| `baseUrl` | string (uri) | yes | Provider-specific base URL targeted by the runner |
| `runtimeVersion` | string | yes | Must match ASP.NET Core runtime `10.0.5` across all four providers |
| `startupPath` | string | yes | Must equal `/api/startup` |
| `computePath` | string | yes | Must equal `/api/compute/matrix` |
| `idleWindowMinutes` | integer | yes | Fixed at `15` in v1 |
| `supportsDeterministicScaleToZeroConfirmation` | boolean | yes | `false` for AWS Lambda in v1 |
| `scaleToZeroEvidenceStrategy` | string | yes | E.g. monitoring metric, platform logs, inferred-only |
| `parityNotes` | array[string] | no | Known caveats documented before runs |

**Validation rules**
- `providerId` must be one of the fixed four v1 providers.
- `runtimeVersion` must equal `10.0.5` across all deployments.
- `region` must match the canonical map for the selected `providerId`.
- `idleWindowMinutes` must equal `15`.
- `startupPath` and `computePath` must be identical for all providers.

### 2. PayloadDefinition

Represents a fixed named matrix payload in the shared catalog.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `payloadId` | string | yes | Stable catalog key, e.g. `matrix-100x100` |
| `dimension` | integer | yes | Allowed values: `100`, `200` |
| `leftMatrix` | array[array[number]] | yes | Square matrix sized to `dimension` |
| `rightMatrix` | array[array[number]] | yes | Square matrix sized to `dimension` |
| `contentHash` | string | yes | Deterministic hash for reproducibility |

**Validation rules**
- Only two payloads are allowed in v1: 100x100 and 200x200.
- Matrix dimensions must match `dimension`.
- `contentHash` must be computed from the serialized payload content.

### 3. WorkloadDefinition

Represents the versioned ordered workload file used by the runner.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `workloadVersion` | string | yes | Semantic or date-based version identifier |
| `description` | string | yes | Human-readable description |
| `payloadCatalog` | array[PayloadDefinition] | yes | Fixed catalog shared across all steps and containing exactly `matrix-100x100` and `matrix-200x200` in v1 |
| `steps` | array[WorkloadStep] | yes | Ordered execution list |

**Validation rules**
- `steps` must contain at least one cold step and one warm step for meaningful comparison.
- Step order is authoritative and must not be altered by the runner.
- Payload definitions referenced by steps must exist in `payloadCatalog`.
- The runner computes the workload file hash from the serialized workload artifact and records it in `BenchmarkRun.workloadFileHash`; it is not authored inside the workload file itself.
- JSON Schema enforces payload shape; custom validation also enforces the exact payload ID set, unique `stepId` values, and contiguous `sequence` numbering.

### 4. WorkloadStep

Represents one ordered request instruction in the workload.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `stepId` | string | yes | Unique within the workload |
| `sequence` | integer | yes | Positive integer defining exact order |
| `intent` | string | yes | Enum: `cold`, `warm` |
| `endpoint` | string | yes | Enum: `startup`, `compute` |
| `method` | string | yes | `GET` for startup, `POST` for compute |
| `payloadRef` | string | conditional | Required for compute steps; omitted for startup |
| `description` | string | no | Human-readable step note |
| `expectedStatus` | integer | yes | Normally `200` |

**Validation rules**
- `sequence` values must be contiguous and unique.
- `startup` steps cannot include `payloadRef`.
- `compute` steps must include a valid `payloadRef`.
- `intent: cold` steps must target `endpoint: startup`.
- `intent: warm` steps must target `endpoint: compute`.
- `intent: cold` steps must be preceded by idle enforcement in execution, not in file structure alone.

### 5. BenchmarkRun

Represents one session execution of the workload against one or more providers.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `runId` | string | yes | Unique run identifier |
| `startedAtUtc` | string (date-time) | yes | Run start timestamp |
| `completedAtUtc` | string (date-time) | no | Run completion timestamp |
| `workloadVersion` | string | yes | Copied from workload file |
| `workloadFileHash` | string | yes | Used to prove identical workload usage |
| `apiContractVersion` | string | yes | Version of the benchmark app OpenAPI contract used for the run |
| `resultSchemaVersion` | string | yes | Version identifier for the normalized results schema |
| `providers` | array[string] | yes | Subset or full set of fixed providers |
| `runnerVersion` | string | yes | Version of the benchmark runner |
| `networkLocationLabel` | string | no | Stable runner location identifier |
| `records` | array[ResultRecord] | yes | Raw results |
| `parityExceptions` | array[ParityException] | yes | May be empty |
| `summaryMetrics` | array[SummaryMetric] | yes | Aggregated output |
| `status` | string | yes | Enum: `running`, `completed`, `completed-with-errors`, `failed` |

### 6. ResultRecord

Represents one measured outcome for one step against one provider.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `runId` | string | yes | Parent run reference |
| `providerId` | string | yes | ProviderDeployment reference |
| `region` | string | yes | Denormalized for easier analysis |
| `stepId` | string | yes | WorkloadStep reference |
| `sequence` | integer | yes | Captured order for replay/debugging |
| `intent` | string | yes | `cold` or `warm` |
| `endpoint` | string | yes | `startup` or `compute` |
| `startedAtUtc` | string (date-time) | yes | Request send timestamp |
| `completedAtUtc` | string (date-time) | yes | Response receipt timestamp |
| `latencyMs` | number | yes | End-to-end wall-clock latency at runner |
| `httpStatus` | integer | yes | HTTP response status |
| `correctness` | string | yes | Enum: `not-applicable`, `passed`, `failed` |
| `responseBody` | object | conditional | Required for `endpoint: compute`; contains the normalized matrix result payload |
| `errorType` | string | no | Timeout, HTTP error, schema mismatch, etc. |
| `errorDetail` | string | no | Human-readable failure detail |
| `scaleToZeroConfirmed` | boolean | conditional | Required for `intent: cold`; omitted for `intent: warm` |
| `annotationRefs` | array[string] | no | Links to parity exception IDs |

**Validation rules**
- `latencyMs` must be non-negative.
- Startup records should use `correctness: not-applicable`.
- Compute records must set `correctness` based on matrix result validation and include the actual matrix output in `responseBody`.
- `scaleToZeroConfirmed` is required for cold steps and may be `false` when paired with a parity exception.
- Warm-step records omit `scaleToZeroConfirmed`.
- A cold-step record with `scaleToZeroConfirmed: false` must link to at least one parity exception through `annotationRefs`.

### 7. ParityException

Represents a known or observed deviation from the idealized cold-start protocol.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `parityExceptionId` | string | yes | Stable identifier |
| `providerId` | string | yes | Provider reference |
| `scope` | string | yes | `provider`, `step`, or `run` |
| `type` | string | yes | E.g. `no-zero-confirmation`, `scale-to-zero-timeout`, `platform-limitation` |
| `description` | string | yes | What differed from the standard protocol |
| `impact` | string | yes | Interpretation impact on comparability |
| `relatedStepId` | string | no | Included when scoped to a step |
| `recordedAtUtc` | string (date-time) | yes | When the exception was added |

### 8. SummaryMetric

Aggregated output for published benchmark summaries.

| Field | Type | Required | Notes |
|------|------|----------|------|
| `providerId` | string | yes | Provider reference |
| `intent` | string | yes | `cold` or `warm` |
| `sampleCount` | integer | yes | Number of included records |
| `minLatencyMs` | number | yes | Required by FR-024 |
| `p50LatencyMs` | number | yes | Required by FR-024 |
| `p95LatencyMs` | number | yes | Required by FR-024 |
| `p99LatencyMs` | number | yes | Required by FR-024 |
| `maxLatencyMs` | number | yes | Required by FR-024 |
| `errorCount` | integer | yes | Number of failed records in the slice |
| `parityExceptionCount` | integer | yes | Count of linked parity exceptions |

## Relationships

- One `WorkloadDefinition` contains many `WorkloadStep` records.
- One `WorkloadDefinition` contains exactly two `PayloadDefinition` records in v1.
- One `BenchmarkRun` references one `WorkloadDefinition` version and many `ResultRecord` entries.
- Each `ResultRecord` maps to one `ProviderDeployment` and one `WorkloadStep`.
- `ParityException` records may relate to a provider, run, or specific step/result.
- `SummaryMetric` aggregates many `ResultRecord` entries for one `(providerId, intent)` pair.

## State Transitions

### BenchmarkRun

`running` → `completed`  
`running` → `completed-with-errors`  
`running` → `failed`

Transition rules:
- Move to `completed` when all planned provider-step executions finish and no record has fatal run-level failure.
- Move to `completed-with-errors` when the run finishes but contains step failures, timeouts, or parity exceptions.
- Move to `failed` only when the runner cannot continue or cannot produce a trustworthy result envelope.

### WorkloadStep Execution

`pending` → `idle-window-enforced` → `scale-check-complete` → `request-sent` → `response-recorded`

Alternative branch for cold steps:

`pending` → `idle-window-enforced` → `parity-exception-recorded` → `request-sent` → `response-recorded`

### Result Correctness

`not-applicable` for startup steps  
`pending-validation` → `passed`  
`pending-validation` → `failed`
