<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Modified principles:
  - Template Principle 1 -> I. Cross-Cloud Fairness & Comparability
  - Template Principle 2 -> II. Reproducibility & Explicit Versioning
  - Template Principle 3 -> III. Contract-First Development
  - Template Principle 4 -> IV. Transparency of Parity Exceptions & Caveats
  - Template Principle 5 -> V. Minimal v1 Scope & Simplicity
- Added sections:
  - Required Artifacts & Gates
  - Delivery & Review Workflow
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
- Follow-up TODOs:
  - None
-->
# cold-start-perf-comparison Constitution

## Core Principles

### I. Cross-Cloud Fairness & Comparability
Benchmark behavior that affects measured latency MUST be shared across providers.
The repository MUST keep one shared benchmark app contract, one shared runner,
one ordered workload definition, one measurement point at the runner, one fixed
v1 idle window of 15 minutes before cold probes, and one canonical region per
provider. Provider-specific implementation exists only where the platform
interface requires it, such as the AWS host adapter, and MUST NOT change
endpoint paths, payloads, summary metrics, or result semantics. Rationale:
results are only comparable when the benchmark method is materially the same on
every platform.

### II. Reproducibility & Explicit Versioning
Every benchmark-affecting artifact MUST be explicitly versioned or pinned before
results are published. At minimum this includes the .NET SDK and runtime
baseline, workload file version and hash, contract and schema versions, provider
descriptors, canonical regions, and the result schema version. Any change to
one of these inputs MUST be captured in repository artifacts and called out in
the related spec, plan, tasks, and published results. Rationale: a benchmark
that cannot be rerun from the recorded inputs is not a trustworthy benchmark.

### III. Contract-First Development
Public benchmark behavior MUST be defined in contracts before implementation
work is merged. HTTP endpoints MUST be described in OpenAPI, workload and
result files MUST be described in JSON Schema, and tasks and tests MUST trace
back to those contracts. A pull request that changes endpoint shape, payload
format, result fields, or workload semantics without updating the governing
contract and validation coverage is non-compliant. Rationale: contract-first
development preserves parity across providers and prevents undocumented drift.

### IV. Transparency of Parity Exceptions & Caveats
Any known deviation from perfect parity MUST be recorded, not hidden. If a
provider cannot confirm scale-to-zero, requires a platform-specific packaging
shim, exposes different operational evidence, or otherwise limits
comparability, the repository MUST record that as a parity exception or
benchmark caveat in the relevant spec, plan, documentation, and result
annotations where applicable. Normalized result records MUST remain
provider-neutral; caveats belong in annotations and documentation, not in
silent behavior differences. Rationale: transparent caveats protect the
credibility of published comparisons.

### V. Minimal v1 Scope & Simplicity
Version 1 is intentionally narrow. The repository MUST limit v1 to exactly four
providers, one shared ASP.NET Core benchmark app, one thin AWS Lambda host
shim, one shared sequential runner, two fixed matrix payloads (100x100 and
200x200), unauthenticated benchmark endpoints, and the published cold and warm
summary metrics. New providers, concurrency experiments, cost analysis,
dashboards, auth, or extra services MUST NOT be added to v1 unless the spec
changes and the added complexity is justified in the plan. Rationale: a
smaller benchmark is easier to keep fair, testable, and reproducible.

## Required Artifacts & Gates

- Every feature spec MUST identify the benchmark contract surface, canonical
  environments, runtime and toolchain pins, fairness constraints, and any known
  parity exceptions.
- Every implementation plan MUST include a Constitution Check that explicitly
  verifies compliance with all five core principles before research begins and
  again after design is complete.
- Every task list for benchmark-affecting work MUST include contract
  validation, reproducibility and versioning updates, parity-exception
  documentation when applicable, and documentation or quickstart validation
  when operator workflow changes.
- Complexity that violates Minimal v1 Scope & Simplicity MUST be logged in
  Complexity Tracking with the simpler alternative rejected and the reason it
  was insufficient.
- Benchmark outputs are non-publishable unless they include normalized raw
  records, run metadata, and p50/p95/p99/min/max summaries by provider and
  intent.

## Delivery & Review Workflow

- Work MUST flow spec -> plan -> tasks -> implementation; implementation MUST
  NOT outrun the approved planning artifacts.
- Contract changes MUST land with corresponding tests and documentation in the
  same change set.
- Reviews MUST reject provider-specific behavior that alters benchmark
  semantics without an approved, documented parity exception.
- Reviews MUST reject unpinned runtime or toolchain changes and undocumented
  workload or schema changes.
- Documentation for deployment and execution MUST be sufficient for a new
  reviewer to reproduce at least a single-provider run without private
  instructions.

## Governance

This constitution supersedes conflicting local conventions for this repository.
Amendments MUST update the constitution and every impacted template or guidance
document in the same change. Versioning follows semantic rules: MAJOR for
removing or redefining a principle in a backward-incompatible way, MINOR for
adding a principle or materially expanding governance, and PATCH for
clarifications that do not change repository obligations. Compliance MUST be
checked in every plan, pull request, and benchmark publication review;
unresolved violations block merge or publication until explicitly amended.

**Version**: 1.0.0 | **Ratified**: 2026-04-14 | **Last Amended**: 2026-04-14
