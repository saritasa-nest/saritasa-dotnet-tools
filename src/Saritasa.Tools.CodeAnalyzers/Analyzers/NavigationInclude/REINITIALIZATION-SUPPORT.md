# Re-initialization support

## Requirements

A **loaded object** is loaded by the query with required paths, e.g. `user` with `Settings.Profile` and `Address.City`.

An assignment makes paths loaded from that point on, and its value is never inspected (`null`, `new`, a local, a method result: all the same):

- `x.A1…An = <anything>` loads `A1…An` and everything under it. Other properties keep the query's loading.
- `x = <anything>` (including `new`, `with`, or a call) loads every required path of `x`.
- An assignment applies only on the paths it reaches. Elsewhere the query's loading still counts.
- Constructor arguments have no effect.

If a required path is neither loaded by the query nor assigned before the use, the analyzer warns (INCL002).

| # | Code (required `Settings.Profile`, `Address.City`) | Expected |
|---|---|---|
| 1 | `user.Settings = null;` | no warning for `Settings.Profile` |
| 2 | `user.Address = new Address();` | no warning for `Address.City`; `Settings.Profile` still from the query |
| 3 | `user.Settings.Profile.Name = "x";` | no effect: writes below the required path |
| 4 | `if (reset) user.Settings = …;` then a use | no warning: the query still loads it where `reset` is false |
| 5 | `user = new User();` | no warning |
| 6 | `user = user with { Settings = … };` | no warning |
| 7 | `user = new User(settings, address);` | constructor arguments have no effect; the whole assignment still loads everything |
| 8 | An assignment after a use | the use is judged by the query; the assignment covers later uses only |

Out of scope:
- `foreach` loops: not supported for now. Elements not loaded by the query give INCL002.
- Calls (`ResetSettings(user)`), lambdas, and local functions.
- Fields on the path (INCL004).
- Tracing values to their origin: aliases, locals, and method results are not followed.

## Implementation

Approach: keep the current backward search (`WritesWalker` + `IncludeSearcher`) and stop reading assigned values.
Each commit builds and keeps the tests green. Tests for the new cases go into a new file,
`test/Saritasa.Tools.CodeAnalyzers.Tests/NavigationIncludeAnalyzerTests/ReInitializationTests.cs`.

### Commit 1: pin current behavior for unchanged cases
- **Concept:** cases that already behave as required (the prefix rule, writes below the path, branch and order rules)
  become tests first, so later commits cannot break them silently.
- **Changes:** tests for P4, P6, P7, W6, O2 in `ReInitializationTests.cs`. No production code.

### Commit 2: member assignment does not inspect the value
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

### Commit 3: whole assignment does not inspect the value
- **Concept:** `x = <anything>` loads every required path of `x`. Object initializers compile to a capture and a
  whole assignment, so they are covered too.
- **Changes:**
  - `IncludeSearcher.ReadWrite`, the `Write.Written` branch: return `Answer.Loaded` instead of reading the value.
  - `WritesWalker.ReadThroughCapture`: no change expected; the capture is followed as before.
  - Update existing tests that expect INCL002 for `user = new User()` or `user = other`.
  - Tests for W1, W2, W3, W5.

### Commit 4: records and constructor arguments
- **Concept:** `user = user with { … }` is a whole assignment (commit 3 covers it). Constructor arguments are never read,
  so they cannot change the result.
- **Changes:** tests for W4, and for the constructor case (`user = new User(settings, address)`). If the `with`
  expression reaches the walker as something other than a whole assignment, fix the classification in
  `WritesWalker.GetWriteOrDefault` here.

### Commit 5: remove dead code
- **Concept:** after commits 2–3 the value-reading path for member and whole writes is no longer used.
- **Changes:** delete helpers that only served it (check each with a usage search first). Keep `Answer.Unknown`
  where the capture or out-argument paths still need it.

### Commit 6: documentation
- **Concept:** the README describes the rules a user relies on.
- **Changes:** a "Re-initialization" subsection in `Analyzers/NavigationInclude/README.md`, with the rules and the
  out-of-scope list from the Requirements section. The old inspection behavior is removed from the text.
