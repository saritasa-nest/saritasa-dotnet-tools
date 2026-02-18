---
description: Execute the implementation planning workflow using the plan template to generate design artifacts.
handoffs:
  - label: Create Tasks
    agent: speckit.tasks
    prompt: Break the plan into tasks
    send: true
---

## User Input

```text
$ARGUMENTS
```

## Outline

1. **Setup**: First, read shell configuration from `src/.speckit/memory/environment.local.md` if it exists, otherwise from `src/.speckit/memory/environment.md`. Check the `shell` variable value (prefer values from environment.local.md). If shell is `PowerShell`, run `powershell -File src/.speckit/scripts/powershell/setup-plan.ps1` from repo root. If shell is `sh`, run `bash src/.speckit/scripts/sh/setup-plan.sh` from repo root. Parse JSON output for SpecPath, PlanPath, CurrentBranch.

2. **Load context**: Read SpecPath, `src/.speckit/memory/constitution.md`.

3. **Execute plan workflow**: Follow the structure in plan template to:
   - Fill Technical Context (mark unknowns as "NEEDS CLARIFICATION")
   - Fill Constitution Check section from `constitution.md`
   - Evaluate gates (ERROR if violations unjustified)
   - Phase 0: Add `Research` section (resolve all NEEDS CLARIFICATION)
   - Phase 1: Add `Data Model` and `Contracts` sections
   - Plan Quality Validation
   - Re-evaluate Constitution Check post-design

4. **Stop and report**: Command ends after Phase 2 planning. Report CurrentBranch, PlanPath path, and generated artifacts.

## Phases

### Phase 0: Outline & Research

1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION research task
   - For each dependency best practices task
   - For each integration patterns task

2. **Generate and dispatch research agents**:

   ```text
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Analyze** `src/.speckit/memory/project-index.md` to find useful references.
   - This file contains references to files which have useful information about the project
   - References have keywords to find information which might be useful in current context without loading all files
   - Use them when planning

4. **Consolidate findings** in `Research` section of PlanPath file:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: `Research` section with all NEEDS CLARIFICATION resolved

### Phase 1: Design & Contracts

**Prerequisites:** `Research` section complete

This section is designed to **NEVER write implementation** or pseudocode in this section. Only contracts and data models allowed.

1. **Generate** `Contracts` from functional requirements:
   - For each use case
   - For WEB API list controllers and list of methods
   - For Desktop enumerate list of ViewModels and required changes
   - For Blazor/Razor Pages enumerate list of ViewModels and required changes

**Output**: `Data Model` and `Contracts` sections.

### Plan Quality Validation

Validate that plan follows requirements:

- No implementation code (excluding contracts and models).

## Key rules

- **No implementation code** (excluding contracts and models). Only technical solutions that describe and justify the proposed solution should be included. The plan is an important part of the process, and the user must familiarize themselves with this file. If they focus on the code, this goal will not be achieved. Therefore, the plan **MUST** be stripped of unnecessary implementation details.
- Use absolute paths
- ERROR on gate failures or unresolved clarifications
