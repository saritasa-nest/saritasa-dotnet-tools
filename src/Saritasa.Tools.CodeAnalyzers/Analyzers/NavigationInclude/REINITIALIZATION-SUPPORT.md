# Re-initialization support

## Requirements

A **loaded object** is loaded by the query with required paths, e.g. `user` with `Settings.Profile` and `Address.City`.

An assignment makes paths loaded from that point on, and its value is never inspected (`null`, `new`, a local, a method result: all the same):

- `x.A1…An = <anything>` loads `A1…An` and everything under it. Other properties keep the query's loading.
- `x = new …` and `x = y with { … }` (object creation) load every required path of `x`. Other whole assignments,
  `x = query` or `x = other`, read the value as before, so a query without `Include` still gives INCL002.
- An assignment applies only on the paths it reaches. Elsewhere the query's loading still counts.
- Constructor arguments are never read: `new User(settings)` counts as loaded, like any object creation.

If a required path is neither loaded by the query nor assigned before the use, the analyzer warns (INCL002).

| # | Code (required `Settings.Profile`, `Address.City`) | Expected |
|---|---|---|
| 1 | `user.Settings = null;` | no warning for `Settings.Profile` |
| 2 | `user.Address = new Address();` | no warning for `Address.City`; `Settings.Profile` still from the query |
| 3 | `user.Settings.Profile.Name = "x";` | no effect: writes below the required path |
| 4 | `if (reset) user.Settings = …;` then a use | no warning: the query still loads it where `reset` is false |
| 5 | `user = new User();` | no warning |
| 6 | `user = user with { Settings = … };` | no warning |
| 7 | `user = new User(settings, address);` | constructor arguments are not read; the object creation counts as loaded |
| 8 | An assignment after a use | the use is judged by the query; the assignment covers later uses only |

Out of scope:
- `foreach` loops: not supported for now. Elements not loaded by the query give INCL002.
- Calls (`ResetSettings(user)`), lambdas, and local functions.
- Fields on the path (INCL004).
- Tracing values to their origin: aliases, locals, and method results are not followed.

## Implementation

Approach: keep the current backward search (`WritesWalker` + `IncludeSearcher`) and stop reading assigned values.
Each commit builds and keeps the tests green. Tests go into the existing test classes: assignments to a local in
`LocalVariableTests.cs`, branch merges in `ControlFlowTests.cs`.

### Commit 1 (done): pin current behavior for unchanged cases
- **Concept:** cases that already behave as required (the prefix rule, writes below the path, branch and order rules)
  become tests first, so later commits cannot break them silently.
- **Changes:** pinned tests for unchanged cases in `LocalVariableTests.cs`. No production code. Redundant ones
  were removed after review.

### Commit 2 (done): member assignment does not inspect the value
- **Concept:** `x.A1…An = <anything>` loads the assigned property `A1…An` and everything under it, without reading the
  value. The chain before it is still validated: `x.A1…A(n-1)` must be loaded on `x` before the assignment.
- **Changes:**
  - `IncludeSearcher.ReadMemberWrite`: the `underAssigned` part returns `Answer.Loaded` instead of
    `Search(member.Value, …)`. The `inFront` part is unchanged and keeps validating the chain to the property.
  - Test: `user.Settings.Profile = …` where the query does not load `Settings` → INCL002 for `Settings`, even though
    the assigned property itself is loaded.
  - Update existing tests that expect INCL002 for a `new` value that lacks the nested path; the new expectation is no
    warning.
  - Tests for P1, P2, P3, P5.

### Commit 3 (done): object creation loads every path, query and other values are still read
- **Decision:** `x = <anything>` does not load every path. Only object creation does (`new`, initializers, and
  `with` for records). `x = query` and `x = other` keep reading the value, so a query without `Include` still gives
  INCL002.
- **Concept:** `new` already returned `Loaded`. Records were the gap: `x = x with { ... }` is lowered by the compiler
  to a clone call, and a write through a compiler temporary was not followed.
- **Changes:**
  - `IncludeSearcher.ReadValue`: a value whose syntax is a `with` expression is `Loaded`. This must come before the
    variable arm, because the copy is read as a temporary.
  - `WritesWalker.WritesVariable`: resolves a target that the compiler captured (`#0 = user; ...; #0 = #1`) to the
    variable, using `ReadThroughCapture`.
  - Tests: `Handle_RecordWithExpression_NoIncl2` in `LocalVariableTests.cs`. The record's `with` sets only `Id`, so
    the `Settings.Profile` answer comes from the rule, not from a member write.

### Commit 4 (done): records and constructor arguments
- **Concept:** `user = user with { … }` counts as creation (done in commit 3). Constructor arguments are never read.
- **Changes:** `Handle_RecordWithExpression_NoIncl2` (commit 3) and `Handle_ConstructorArgumentsCountAsLoaded_NoIncl2`,
  both in `LocalVariableTests.cs`. The argument's source has no `Profile`, so a read of it would give INCL002.

### Commit 5 (done): remove dead code
- **Concept:** after commits 2–3 the value-reading path for member writes is no longer used.
- **Changes:** removed `Write.MemberWritten.Value` (nothing read it) and `NavigationPath.SkipFirst` (no caller).
  `Answer.Unknown` and `JoinPaths` are still used by other paths and stay.

### Commit 6 (done): documentation
- **Concept:** the README describes the rules a user relies on.
- **Changes:** `README.md`: a "Re-initialization" section under Special cases, a row for the captured target in
  "What the compiler rewrites", and the "deliberately optimistic" bullets updated for `with` and for assigned values.
