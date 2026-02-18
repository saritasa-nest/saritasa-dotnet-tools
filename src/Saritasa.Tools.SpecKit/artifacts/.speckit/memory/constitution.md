# [PROJECT_NAME] Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)

**Strict layer separation MUST be enforced:**
- **Domain** - Pure domain entities with no external dependencies. Feature-organized (e.g., Store, Users).
- **Domain Services** - Simple helper services for domain operations. Keep logic minimal; complex business logic belongs in UseCases.
- **Infrastructure** - Data access (EF Core, repositories), external integrations. Depends on Domain only.
- **UseCases** - Business logic orchestration. Contains application-specific workflows. Feature-organized to match Domain.
- **Web** - API controllers, middleware, startup configuration. Thin layer - delegates to UseCases.

**Rationale:** Clean Architecture ensures maintainability, testability, and clear separation of concerns. Prevents coupling and makes the codebase comprehensible as it scales.

### II. CQRS via MediatR (NON-NEGOTIABLE)

**All business logic MUST follow CQRS pattern using MediatR:**
- **Commands** - Mutate state, return void or entity ID. Named with action verbs (e.g., `CreateProductCommand`, `UpdateOrderStatusCommand`)
- **Queries** - Read-only operations, return DTOs. Named with descriptive nouns (e.g., `GetProductByIdQuery`, `SearchActiveUsersQuery`)
- **Handlers** - One handler per command/query. Contains business logic orchestration (e.g., `CreateProductCommandHandler`)
- **Controllers** - Thin wrappers that send requests via `IMediator` and return HTTP responses

**MediatR pipeline MUST be used for cross-cutting concerns:**
- Validation (FluentValidation integration recommended)
- Logging and performance monitoring
- Transaction management
- Authorization checks

**Rationale:** CQRS separates read and write concerns, improving scalability and clarity. MediatR provides clean abstraction for request/response patterns, enabling testable and maintainable command/query handlers. Pipeline behaviors eliminate repetitive cross-cutting code.

### III. Zero-Warning Build Policy (NON-NEGOTIABLE)

**All builds MUST complete with zero warnings:**
- Compiler warnings treated as errors in CI/CD pipelines
- Code analysis and security analyzers enabled with appropriate severity levels
- Warnings must be fixed, not suppressed, unless documented justification provided
- XML documentation comments required for public APIs where applicable

**Rationale:** Warnings indicate potential bugs, code smells, or maintainability issues. Zero-warning policy enforces code quality and prevents warning fatigue that masks real problems.

### [PRINCIPLE_IV_NAME]
<!-- Example: IV. Test-First (NON-NEGOTIABLE) -->
[PRINCIPLE_IV_DESCRIPTION]
<!-- Example: TDD mandatory: Tests written → User approved → Tests fail → Then implement; Red-Green-Refactor cycle strictly enforced -->

## Technology Stack Standards

**Mandatory Technologies:**
- **.NET** - Latest LTS version at project start
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for data access
- **MediatR** - CQRS implementation
- **PostgreSQL** - Default database (configurable via connection string)
- **Serilog** - Structured logging with JSON output

**Optional Technologies (per project requirements):**
- **FluentValidation** - Command/query validation
- **AutoMapper** - DTO mapping
- **Hangfire** - Background job processing
- **.NET Aspire** - Cloud-native orchestration

**Testing Requirements:**
- Unit tests for handlers and domain logic
- Integration tests for API endpoints (recommended)
- Test projects follow naming: `[ProjectName].Tests`

## Development Workflow

**Standard Workflow:**
1. Feature branches created from `develop`
2. Code implements feature following architecture principles
3. Unit tests written for business logic
4. Pull request created with description linking to Jira ticket
5. Code review verifies architecture compliance and functionality
6. CI pipeline validates build, tests, code quality (zero warnings required)
7. Merge to target branch after approval

**Required Git Practices:**
- Commit messages follow conventional commits format (e.g., ends with task code)
- Feature branch has a task code
- No direct commits to protected branches
- Squash commits on merge (preferred) or maintain clean history

**Jira Integration:**
- All work tracked in Jira
- PR descriptions reference Jira ticket number
- Follow Saritasa Jira Tips: https://wiki.saritasa.rocks/general/jira-tips

## [SECTION_NAME]
<!-- Example: Additional Constraints, Security Requirements, Performance Standards, etc. -->

## Governance

[GOVERNANCE_RULES]
<!-- Example: All PRs/reviews must verify compliance; Complexity must be justified -->

**Version**: [CONSTITUTION_VERSION] | **Ratified**: [RATIFICATION_DATE] | **Last Amended**: [LAST_AMENDED_DATE]
<!-- Example: Version: 2.1.1 | Ratified: 2025-06-13 | Last Amended: 2025-07-16 -->
