# Tasks: Cross-Cloud .NET Cold-Start Performance Benchmark

**Input**: Design documents from `/specs/001-cross-cloud-benchmark/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Include contract, integration, unit, and documentation smoke tests because the feature spec explicitly requires independent testing and reproducibility validation.

**Organization**: Tasks are grouped by user story to keep each delivery slice independently testable while preserving the planned repository structure.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Maps the task to a specific user story (`[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Every task below includes exact file paths

## Path Conventions

- Production code lives in `src/`
- Deployment assets live in `deploy/`
- Workload artifacts live in `workloads/`
- Automated tests live in `tests/`

## v1 Guardrails (Keep Visible During Implementation)

- Exactly four providers only: GCP Cloud Run, AWS Lambda, Azure Container Apps, Scaleway Serverless Containers
- One shared `.NET 10` benchmark app plus one thin AWS Lambda host shim
- One shared runner in `src/BenchmarkRunner/`
- Sequential execution only in v1; no parallel request dispatch
- Enforce the same 15-minute idle window before every `intent: cold` step
- Use only `matrix-100x100` and `matrix-200x200`
- Keep shared endpoint paths fixed at `GET /api/startup` and `POST /api/compute/matrix`
- Record parity exceptions instead of failing the run when zero-state confirmation is unavailable
- No authentication or provider-specific result fields in v1

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold the nearly empty repository into the planned .NET solution and test layout.

- [X] T001 Create the root solution and pin .NET SDK `10.0.201` in `cold-start-perf-comparison.sln` and `global.json`
- [X] T002 [P] Scaffold production projects in `src/Benchmark.Contracts/Benchmark.Contracts.csproj`, `src/BenchmarkApp/BenchmarkApp.csproj`, `src/BenchmarkRunner/BenchmarkRunner.csproj`, and `src/BenchmarkApp.AwsLambdaHost/BenchmarkApp.AwsLambdaHost.csproj`
- [X] T003 [P] Scaffold xUnit test projects in `tests/contract/Benchmark.ContractTests/Benchmark.ContractTests.csproj`, `tests/integration/Benchmark.IntegrationTests/Benchmark.IntegrationTests.csproj`, and `tests/unit/Benchmark.UnitTests/Benchmark.UnitTests.csproj`
- [X] T004 [P] Add centralized package and build configuration in `Directory.Build.props` and `Directory.Packages.props`
- [X] T005 [P] Add repository defaults for .NET outputs and benchmark artifacts in `.gitignore` and `.editorconfig`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared contracts and validation building blocks required by every user story.

**⚠️ CRITICAL**: No user story work should start before this phase is complete.

- [X] T006 Create shared benchmark domain models in `src/Benchmark.Contracts/Providers/ProviderDeployment.cs`, `src/Benchmark.Contracts/Workloads/PayloadDefinition.cs`, `src/Benchmark.Contracts/Workloads/WorkloadDefinition.cs`, `src/Benchmark.Contracts/Workloads/WorkloadStep.cs`, `src/Benchmark.Contracts/Results/BenchmarkRun.cs`, `src/Benchmark.Contracts/Results/ResultRecord.cs`, `src/Benchmark.Contracts/Results/ParityException.cs`, and `src/Benchmark.Contracts/Results/SummaryMetric.cs`
- [X] T007 [P] Implement workload and results schema validation helpers in `src/Benchmark.Contracts/Validation/WorkloadSchemaValidator.cs` and `src/Benchmark.Contracts/Validation/ResultsSchemaValidator.cs`, including custom checks for unique `stepId` values, contiguous `sequence` numbering, and the exact two-entry payload catalog
- [X] T008 [P] Add provider target configuration types in `src/BenchmarkRunner/Configuration/ProviderTargetOptions.cs` and `src/BenchmarkRunner/Configuration/ProviderTargetCatalog.cs`
- [X] T009 [P] Add deterministic matrix payload utilities in `src/Benchmark.Contracts/Payloads/MatrixPayloadFactory.cs` and `src/Benchmark.Contracts/Payloads/MatrixHashCalculator.cs`

**Checkpoint**: Shared contracts, payload helpers, and runner configuration primitives are ready for story work.

---

## Phase 3: User Story 1 - Run a Reproducible Cross-Provider Cold-Start Benchmark (Priority: P1) 🎯 MVP

**Goal**: Deliver the shared benchmark app, sequential runner skeleton, and normalized result envelope needed to execute one ordered workload and capture comparable records.

**Independent Test**: Run the benchmark against any single deployed provider using `workloads/v1/cross-cloud-sequential.json` and verify the emitted JSON result envelope includes provider, region, step ID, latency, status, and cold/warm intent for every executed step.

### Tests for User Story 1

- [X] T010 [P] [US1] Add API contract tests for `GET /api/startup` and `POST /api/compute/matrix` against `specs/001-cross-cloud-benchmark/contracts/benchmark-app.openapi.yaml` in `tests/contract/Benchmark.ContractTests/BenchmarkAppOpenApiContractTests.cs`
- [X] T011 [P] [US1] Add workload and results schema contract tests for `workloads/v1/cross-cloud-sequential.json`, `specs/001-cross-cloud-benchmark/contracts/workload.schema.json`, and `specs/001-cross-cloud-benchmark/contracts/results.schema.json` in `tests/contract/Benchmark.ContractTests/BenchmarkSchemasContractTests.cs`, including assertions for at least one cold step and one warm step
- [X] T012 [P] [US1] Add sequential runner integration coverage for ordered step execution and record emission in `tests/integration/Benchmark.IntegrationTests/SequentialWorkloadExecutorIntegrationTests.cs`

### Implementation for User Story 1

- [X] T013 [US1] Implement the shared benchmark host and startup probe route in `src/BenchmarkApp/Program.cs` and `src/BenchmarkApp/Endpoints/StartupEndpoint.cs`
- [X] T014 [P] [US1] Implement run metadata and raw result-envelope assembly in `src/BenchmarkRunner/Program.cs` and `src/BenchmarkRunner/Services/RunEnvelopeBuilder.cs`, including workload version, benchmark app contract version, and result schema version capture
- [X] T015 [P] [US1] Add workload loading and workload hash capture in `src/BenchmarkRunner/Services/WorkloadFileLoader.cs` and `src/BenchmarkRunner/Services/WorkloadHashService.cs`
- [X] T016 [US1] Author the schema-valid sequential v1 workload artifact with the fixed two-entry payload catalog and explicit cold/warm step order in `workloads/v1/cross-cloud-sequential.json`
- [X] T017 [US1] Implement single-request sequential dispatch with shared path targeting in `src/BenchmarkRunner/Services/SequentialWorkloadExecutor.cs`
- [X] T018 [US1] Write normalized result envelopes as structured JSON output in `src/BenchmarkRunner/Services/ResultEnvelopeWriter.cs`

**Checkpoint**: User Story 1 produces reproducible ordered runs and normalized raw result records without introducing concurrency.

---

## Phase 4: User Story 2 - Enforce and Confirm Cold-Start Measurement (Priority: P2)

**Goal**: Guarantee the v1 15-minute cold-step protocol is enforced uniformly and annotate every missing or partial zero-state confirmation as a parity exception.

**Independent Test**: For one provider, verify the runner waits the full 15-minute idle window before a cold step, attempts zero-state confirmation, and records either `scaleToZeroConfirmed: true` or a parity exception before sending the request.

### Tests for User Story 2

- [X] T019 [P] [US2] Add integration tests for the fixed 15-minute idle window and cold-step confirmation flow in `tests/integration/Benchmark.IntegrationTests/ColdStartIdleWindowIntegrationTests.cs`
- [X] T020 [P] [US2] Add unit tests for provider evidence evaluation and parity exception mapping in `tests/unit/Benchmark.UnitTests/ScaleEvidenceServiceImplementationsTests.cs`

### Implementation for User Story 2

- [X] T021 [US2] Implement the fixed v1 cold-step idle coordinator in `src/BenchmarkRunner/Services/ColdStartIdleWindowCoordinator.cs`
- [X] T022 [P] [US2] Implement provider zero-state evidence services in `src/BenchmarkRunner/Services/ScaleEvidence/IProviderScaleEvidenceService.cs`, `src/BenchmarkRunner/Services/ScaleEvidence/CloudRunScaleEvidenceService.cs`, `src/BenchmarkRunner/Services/ScaleEvidence/AzureContainerAppsScaleEvidenceService.cs`, `src/BenchmarkRunner/Services/ScaleEvidence/ScalewayScaleEvidenceService.cs`, and `src/BenchmarkRunner/Services/ScaleEvidence/AwsLambdaScaleEvidenceService.cs`
- [X] T023 [US2] Integrate cold-step confirmation and parity exception recording into `src/BenchmarkRunner/Services/SequentialWorkloadExecutor.cs` and `src/BenchmarkRunner/Services/ParityExceptionRecorder.cs`
- [X] T024 [US2] Capture per-provider cold-start policy metadata, canonical regions, disabled warm-start optimizations, and benchmark resource settings in `deploy/gcp-cloud-run/descriptor.yaml`, `deploy/aws-lambda/descriptor.yaml`, `deploy/azure-container-apps/descriptor.yaml`, and `deploy/scaleway-serverless/descriptor.yaml` using GCP=`europe-west1`, AWS=`eu-west-1`, Azure=`westeurope`, and Scaleway=`fr-par`

**Checkpoint**: User Story 2 makes cold-start intent trustworthy and transparent even when a platform cannot expose deterministic scale-to-zero evidence.

---

## Phase 5: User Story 3 - Execute and Capture Compute-Probe Results (Priority: P3)

**Goal**: Add the fixed matrix compute probe, correctness validation, and summary metrics for warm compute comparisons.

**Independent Test**: Send a fixed payload to `POST /api/compute/matrix` for a single provider and verify the result record captures warm intent, latency, status, correctness, and matrix output without stopping the run on malformed payload errors.

### Tests for User Story 3

- [X] T025 [P] [US3] Add unit tests for matrix multiplication correctness and payload guards in `tests/unit/Benchmark.UnitTests/MatrixComputeServiceTests.cs`
- [X] T026 [P] [US3] Add integration tests for warm compute success, malformed payload handling, and run continuation in `tests/integration/Benchmark.IntegrationTests/ComputeProbeIntegrationTests.cs`

### Implementation for User Story 3

- [X] T027 [US3] Implement the compute endpoint and matrix multiplication service in `src/BenchmarkApp/Endpoints/MatrixComputeEndpoint.cs` and `src/BenchmarkApp/Services/MatrixComputeService.cs`
- [X] T028 [P] [US3] Implement fixed-payload request validation and response DTO mapping in `src/BenchmarkApp/Validation/MatrixComputeRequestValidator.cs` and `src/BenchmarkApp/Models/MatrixComputeContracts.cs`
- [X] T029 [US3] Finalize deterministic matrix contents and content hashes in the existing `workloads/v1/cross-cloud-sequential.json` payload catalog for `matrix-100x100` and `matrix-200x200`, then re-run the contract/schema validation from `T011`
- [X] T030 [US3] Implement warm-step compute execution and correctness verification in `src/BenchmarkRunner/Services/ComputeStepExecutor.cs` and `src/BenchmarkRunner/Services/MatrixResultVerifier.cs`
- [X] T031 [US3] Compute per-provider and per-intent `p50`, `p95`, `p99`, `min`, and `max` summaries in `src/BenchmarkRunner/Services/SummaryMetricCalculator.cs` and `src/BenchmarkRunner/Services/RunEnvelopeBuilder.cs`
- [X] T032 [US3] Capture compute response bodies, correctness state, and non-fatal error continuation rules in `src/BenchmarkRunner/Services/ResultRecordFactory.cs` and `src/BenchmarkRunner/Services/RunFailurePolicy.cs`

**Checkpoint**: User Story 3 adds comparable warm-compute measurements and summary metrics while preserving sequential v1 execution.

---

## Phase 6: User Story 4 - Reproduce Benchmark from Documentation Alone (Priority: P4)

**Goal**: Make the benchmark deployable and runnable from the repository alone, with explicit provider descriptors, AWS packaging, and operator-facing documentation.

**Independent Test**: A new reviewer can follow `README.md` plus `specs/001-cross-cloud-benchmark/quickstart.md` to deploy one provider, run the benchmark, and validate the output structure without asking for undocumented setup details.

### Tests for User Story 4

- [X] T033 [P] [US4] Add documentation smoke tests for single-provider deployment and runner execution steps in `tests/integration/Benchmark.IntegrationTests/QuickstartSmokeTests.cs`
- [X] T034 [P] [US4] Add provider descriptor validation tests for the `deploy/` manifests in `tests/contract/Benchmark.ContractTests/ProviderDescriptorContractTests.cs`

### Implementation for User Story 4

- [X] T035 [US4] Add the thin AWS Lambda host shim over the shared app in `src/BenchmarkApp.AwsLambdaHost/Program.cs` and `src/BenchmarkApp.AwsLambdaHost/LambdaEntryPoint.cs`
- [X] T036 [P] [US4] Create GCP Cloud Run and Azure Container Apps native deployment manifests plus operator notes alongside the benchmark descriptors in `deploy/gcp-cloud-run/service.yaml`, `deploy/gcp-cloud-run/README.md`, `deploy/azure-container-apps/containerapp.yaml`, and `deploy/azure-container-apps/README.md`
- [X] T037 [P] [US4] Create AWS Lambda and Scaleway native deployment manifests plus operator notes alongside the benchmark descriptors in `deploy/aws-lambda/template.yaml`, `deploy/aws-lambda/README.md`, `deploy/scaleway-serverless/container.yaml`, and `deploy/scaleway-serverless/README.md`
- [X] T038 [US4] Document the canonical provider regions, runtime/toolchain pin (`10.0.5` / `10.0.201`), disabled warm-start optimizations, resource settings, deployment steps, and end-to-end run instructions in `README.md` and `specs/001-cross-cloud-benchmark/quickstart.md`

**Checkpoint**: User Story 4 makes the benchmark reproducible from repository documentation and deployment assets alone.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Finalize cross-story configuration, validation wiring, and v1 guardrail documentation.

- [X] T039 [P] Update benchmark configuration examples and output-path guidance in `src/BenchmarkRunner/appsettings.json` and `README.md`
- [X] T040 [P] Add final cross-project test wiring and execution guidance in `tests/contract/Benchmark.ContractTests/Benchmark.ContractTests.csproj`, `tests/integration/Benchmark.IntegrationTests/Benchmark.IntegrationTests.csproj`, `tests/unit/Benchmark.UnitTests/Benchmark.UnitTests.csproj`, and `README.md`
- [X] T041 Validate v1 guardrail, parity-caveat, warm-start-disablement, and resource-setting language in `README.md`, `deploy/gcp-cloud-run/README.md`, `deploy/aws-lambda/README.md`, `deploy/azure-container-apps/README.md`, and `deploy/scaleway-serverless/README.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** → can start immediately
- **Phase 2: Foundational** → depends on Phase 1 and blocks all user stories
- **Phase 3: US1** → depends on Phase 2
- **Phase 4: US2** → depends on US1 runner skeleton being in place
- **Phase 5: US3** → depends on US1 shared app/runner skeleton and can extend it through separate compute collaborators after US2 lands
- **Phase 6: US4** → depends on US1-US3 implementation assets for packaging, docs, and smoke validation
- **Phase 7: Polish** → depends on all selected user stories being complete

### User Story Dependencies

- **US1 (P1)**: First deliverable and MVP slice
- **US2 (P2)**: Extends US1 with cold-start enforcement and parity annotation
- **US3 (P3)**: Extends US1 with compute workload execution and depends on US2’s cold-step runner flow for complete benchmark parity
- **US4 (P4)**: Documents and packages the system after US1-US3 behavior is available
- **Task extension note**: `T031` extends `src/BenchmarkRunner/Services/RunEnvelopeBuilder.cs`, which is first created in `T014`

### Within Each User Story

- Write tests before implementation and confirm they fail for the missing behavior
- Keep sequential v1 rules intact when editing `workloads/v1/cross-cloud-sequential.json` and `src/BenchmarkRunner/Services/SequentialWorkloadExecutor.cs`
- Finish each story’s contract and integration coverage before moving to the next phase

---

## Parallel Opportunities

- **Setup**: `T002`, `T003`, `T004`, and `T005` can run in parallel after `T001`
- **Foundational**: `T007`, `T008`, and `T009` can run in parallel after `T006`
- **US1**: `T010`, `T011`, and `T012` can run in parallel; `T014` and `T015` can run in parallel after `T013`
- **US2**: `T019` and `T020` can run in parallel; `T022` and `T024` can run in parallel after `T021`
- **US3**: `T025` and `T026` can run in parallel; `T028` can proceed in parallel with `T027`; `T031` and `T032` can proceed after `T030`
- **US4**: `T033` and `T034` can run in parallel; `T036` and `T037` can run in parallel after `T035`
- **Polish**: `T039` and `T040` can run in parallel before `T041`

---

## Parallel Example: User Story 1

```bash
Task: "T010 [US1] Add API contract tests in tests/contract/Benchmark.ContractTests/BenchmarkAppOpenApiContractTests.cs"
Task: "T011 [US1] Add workload/results schema tests in tests/contract/Benchmark.ContractTests/BenchmarkSchemasContractTests.cs"
Task: "T012 [US1] Add sequential runner integration tests in tests/integration/Benchmark.IntegrationTests/SequentialWorkloadExecutorIntegrationTests.cs"
```

## Parallel Example: User Story 2

```bash
Task: "T019 [US2] Add idle-window integration tests in tests/integration/Benchmark.IntegrationTests/ColdStartIdleWindowIntegrationTests.cs"
Task: "T020 [US2] Add scale-evidence unit tests in tests/unit/Benchmark.UnitTests/ScaleEvidenceServiceImplementationsTests.cs"
```

## Parallel Example: User Story 3

```bash
Task: "T025 [US3] Add matrix compute unit tests in tests/unit/Benchmark.UnitTests/MatrixComputeServiceTests.cs"
Task: "T026 [US3] Add compute probe integration tests in tests/integration/Benchmark.IntegrationTests/ComputeProbeIntegrationTests.cs"
```

## Parallel Example: User Story 4

```bash
Task: "T036 [US4] Create Cloud Run and Azure deployment descriptors in deploy/gcp-cloud-run/ and deploy/azure-container-apps/"
Task: "T037 [US4] Create AWS Lambda and Scaleway deployment descriptors in deploy/aws-lambda/ and deploy/scaleway-serverless/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate one sequential run against a single provider using `workloads/v1/cross-cloud-sequential.json`
5. Stop and review before adding cold-confirmation and compute complexity

### Incremental Delivery

1. Deliver **US1** for ordered execution and normalized raw results
2. Add **US2** for enforced cold-start fairness and parity exceptions
3. Add **US3** for warm compute execution and summary metrics
4. Add **US4** for provider packaging and documentation-only reproducibility
5. Finish with Phase 7 polish and final validation guidance

### Suggested Team Split

1. One developer owns shared scaffolding and contracts in Phases 1-2
2. One developer extends the runner through US1-US2
3. One developer owns compute endpoint and warm-step work in US3 after US1 lands
4. One developer owns deployment manifests and documentation in US4 once runtime behavior stabilizes

---

## Notes

- The task list intentionally keeps the sequential v1 rules explicit in both workload and runner tasks
- Provider descriptor tasks are separated from native deployment-manifest tasks so cold-start evidence policy and deployment packaging stay traceable
- The repository is currently minimal, so scaffolding tasks are included instead of assuming an existing .NET solution
- Finish implementation with a publishability check confirming the output envelope includes raw records, run metadata, parity annotations, and p50/p95/p99/min/max summaries before sharing benchmark results
