---
description: Execute the implementation plan by processing and executing all tasks defined in tasks.md
handoffs:
  - label: Make Documentation
    agent: speckit.document
    prompt: Summarize the feature specification files and write documentation.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

1. Use the `get_spec_files` MCP tool to retrieve the paths for the current feature. Parse JSON output for Specification, Plan and Tasks. All paths are absolute.

2. Load and analyze the implementation context:
   - **REQUIRED**: Read Tasks for the complete task list and execution plan. Stop execution if doesn't exist
   - **REQUIRED**: Read Plan for tech stack, architecture, and file structure. Stop execution if doesn't exist

3. Parse Tasks structure and extract:
   - **Task phases**: Setup, Tests, Core, Integration, Polish
   - **Task dependencies**: Sequential vs parallel execution rules
   - **Task details**: ID, description, file paths, parallel markers [P]
   - **Execution flow**: Order and dependency requirements

4. Execute implementation following the task plan:
   - **Phase-by-phase execution**: Complete each phase before moving to the next
   - **Respect dependencies**: Run sequential tasks in order, parallel tasks [P] can run together
   - **Follow TDD approach**: Execute test tasks before their corresponding implementation tasks
   - **File-based coordination**: Tasks affecting the same files must run sequentially
   - **Validation checkpoints**: Verify each phase completion before proceeding

5. Implementation execution rules:
   - **Setup first**: Initialize project structure, dependencies, configuration
   - **Tests before code**: If you need to write tests for contracts, entities, and integration scenarios
   - **Core development**: Implement models, services, CLI commands, endpoints
   - **Integration work**: Database connections, middleware, logging, external services
   - **Polish and validation**: Unit tests, performance optimization, documentation

6. Progress tracking and error handling:
   - Report progress after each completed task
   - Halt execution if any non-parallel task fails
   - For parallel tasks [P], continue with successful tasks, report failed ones
   - Provide clear error messages with context for debugging
   - Suggest next steps if implementation cannot proceed
   - **IMPORTANT** For completed tasks, make sure to mark the task off as [X] in the tasks file.

7. Completion validation:
   - Verify all required tasks are completed
   - Check that implemented features match the original specification
   - Validate that tests pass and coverage meets requirements
   - Confirm the implementation follows the technical plan
   - Ensure that generated code didn't produce new warnings and errors
   - Ensure that code hasn't high cognitive complexity
   - Report final status with summary of completed work

Note: This command assumes a complete task breakdown exists in Tasks. If tasks are incomplete or missing, suggest running `/speckit.tasks` first to regenerate the task list.
