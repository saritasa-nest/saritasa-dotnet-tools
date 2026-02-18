# Implementation Plan: [FEATURE]

**Feature Branch**: `[feature-name]`
**Created**: [DATE]
**Input**: User input: "$ARGUMENTS"

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

**Language/Version**: [C# <version>, .NET <version>]
**Primary Dependencies**: [DI, logging, EF Core, Automapper]
**Storage**: [PostgreSQL/SQL Server via EF Core or N/A]
**Testing**: [xUnit/NUnit/MSTest, FluentAssertions]
**Target Platform**: [ASP.NET Core API/UI, CLI, Desktop, Workers]
**Performance Goals**: [p95 latency, throughput, memory]
**Constraints**: [SLOs, deployment targets]
**Scale/Scope**: [users, endpoints, jobs]

## Research

[Describe only core choices. Omit description of choices which typical for the solution. If there aren't changes which needs justification just write this and skip this section. Don't write tech choice just to write it]

For each technology choice:
- Decision: [what was chosen]
- Rationale: [why chosen]
- Alternatives considered: [what else evaluated]

## Project Structure

### Changes Overview (repository root)
<!-- ACTION REQUIRED: Show required changes:
  - Display changed/removed/added files
  - Hide other non-changed parts
  - Use template below. Actual structure might be different. Prioritize existing structure -->

# src assemblies example (apply required actions)
```text
src/
├── Company.TestProject.Domain/
├── Company.TestProject.Domain.Tests/
├── Company.TestProject.Infrastructure.Abstractions/
├── Company.TestProject.UseCases/
├── Company.TestProject.UseCases.Tests/
├── Company.TestProject.UseCases.Common/
├── Company.TestProject.Infrastructure/
├── Company.TestProject.Infrastructure.FedEx/
├── Company.TestProject.Infrastructure.DataAccess/
├── Company.TestProject.Cli/
├── Company.TestProject.Desktop/
├── Company.TestProject.Web/
└── Company.TestProject.sln
```

## Data Model

[Data model related to the feature]

## Contracts

[Contracts between layers]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

*STRICTLY use this format. Only name + passes/fails, no extra information*

- [Principle Name] [PASS]/[FAILED]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
