# Member assignments through aliases (draft for discussion)

## Context
Right now a member assignment is a backward step. `WritesWalker` reports `Write.MemberWritten(value,
assignedCount, statement)` when the required path starts with the assigned property chain, of any length.
`IncludeSearcher.ReadMemberWrite` then joins two searches:
1. the part of the path in front of the assigned property, searched above the assignment;
2. the rest of the path, searched in the assigned value.

The user confirmed these semantics, and all 333 tests pass.

The walker only recognizes an assignment whose target chain is rooted at **the searched variable itself**. Any
other name for the same object is missed. The one exception is the compiler capture `#1`, which
`ReadThroughCapture` resolves by starting a new backward walk for every capture it meets. The user wants
aliases handled in general, through one alias map built before the search, instead of special handling for
captures.

Aliases that occur in practice:

| Code | Alias |
|---|---|
| `app.Admin = new User { … }` → `#1 = app.Admin; …; #1 = #2` | `#1` ≡ `app.Admin` (compiler capture of the target) |
| `new User { Settings = s }` → `#1 = new User(); #1.Settings = s` | none: `#1` holds a fresh object, not another variable's chain |
| `var s = user.Settings; s.Profile = x;` | `s` ≡ `user.Settings` |
| `var u2 = user; u2.Settings = x;` | `u2` ≡ `user` |
| `var p = s.Profile;` after the line above | `p` ≡ `user.Settings.Profile` (chained, resolved transitively) |

## Core idea: canonical chains
An **alias** maps a variable to `(root variable, property chain)`, for example `s → (user, [Settings])`.
Resolving is transitive, and a variable that is not an alias maps to itself with an empty chain.

Both sides of the walker's check become canonical:
- **Searched:** variable `v` with path `P` becomes `(r, Cv + P)`, where `v → (r, Cv)`.
- **Assignment:** target `X.c1…ck = value`, where `X → (r', Cx)`, becomes `(r', Cx + c1…ck)`.

The assignment is a `MemberWritten` for the search when:
- `r == r'`;
- the canonical assigned chain starts with `Cv`, and is **longer** than `Cv`. A write at `user.Settings` while
  searching `s` replaces `user.Settings` but does not change the object `s` still holds;
- the canonical searched path starts with the canonical assigned chain.

The `assignedCount` relative to the searched variable is `|Cx + c1…ck| − |Cv|`. `ReadMemberWrite` stays as it
is: the part in front is still searched on the searched variable `v`, and existing code handles `v`'s own
writes and captures.

## Soundness: when an alias may be trusted
An alias produces **fewer** warnings, so a wrong alias hides a real problem. The map therefore only keeps an
alias when nothing can make it stale:
1. **The alias variable is written exactly once** in the body. This rules out `s = other.Settings` later, a
   variable assigned in branches, and a capture written in both arms of `?:`. Captures are usually written
   once, and a `var` with an initializer is written once.
2. **The root variable is written at most once.** A parameter that is never assigned counts as written once.
   This rules out `var s = user.Settings; user = other; s.Profile = x`.
3. **No assignment anywhere in the body writes a prefix of the alias chain through a property.** This rules
   out `var s = user.Settings; user.Settings = other.Settings; s.Profile = x`. A write of the alias itself
   (`#1 = #2` on a target capture) is not a prefix write.
4. **Only variables of this body count, both alias and root.** These are locals declared in it, its
   parameters, and its captures. Writes are counted over the body's whole operation tree, including nested
   lambdas and local functions. A variable from an enclosing body may be written where this body cannot see,
   so it never takes part.

Writes that are counted: simple assignment, variable initializer, `out`/`ref` argument, compound assignment,
increment, deconstruction, and the `foreach` variable. An assignment to a property of the variable is not a
write of the variable.

## Where the map is built (preflight)
The user suggested collecting the map while scanning for the places that need includes. It cannot be filled
during that scan, because a search can start at a statement before an aliasing write further down has been
seen. A loop back edge makes this happen. So it is built as a preflight, lazily, the first time a body's
`FlowGraph` is asked for it:
- **New** `Search/Aliases.cs`: `internal sealed class Aliases`, built from a `FlowGraph`. It does one pass over
  `Graph.OriginalOperation.DescendantsAndSelf()` for locals and parameters (counting writes and collecting
  prefix writes), and one pass over `Graph.Blocks[*].Operations` for `IFlowCaptureOperation`s. Its API:
  - `(object Root, ImmutableArray<string> Chain) Resolve(object variable)`
  - `(object Root, ImmutableArray<string> Chain)? ResolveTarget(IOperation target)`: walks the
    `IPropertyReferenceOperation.Instance` chain (syntax only) down to a variable, then calls `Resolve`.
- `Search/FlowGraph.cs`: a lazy `Aliases` property. Each lambda and local-function `FlowGraph` builds its own
  map, which is right under rule 4.

## Changes
- `Search/WritesWalker.cs`
  - Delete `ReadThroughCapture`. `GetWriteOrDefault` becomes `static` again.
  - Replace `ReadAssignedCount` with a canonical comparison through `statement.FlowGraph.Aliases`, as in Core
    idea above.
  - The `#1.Settings = x` initializer case needs no guard any more. `#1` holds `new User()`, which is not a
    chain, so it is not an alias and maps to itself.
- `Search/IncludeSearcher.cs`: no change. `ReadMemberWrite` already joins the part in front and the value.
- `Requirements/NavigationPath.cs`: may need `Prepend(IEnumerable<string>)` or a `StartsWith` overload that
  takes `ImmutableArray`. Both are small.
- `README.md`: a new "Aliases" subsection with the table above, the four soundness rules, and what stays
  unsupported.

## Still not handled (warns, the safe side)
- Roots that are not variables of the body: fields (`this.user.Settings = x`), calls
  (`GetUser().Settings = x`), and outer variables inside lambdas.
- Aliases through bridges or collections: `var first = users[0]; first.Profile = x` while searching `users`
  for `Profile`. The element and the collection are not the same object.
- Mutating methods: `user.SetProfile(x)`.

## Tests (LocalVariableTests)
Existing tests stay green. New ones:
- **No warning:**
  - `var s = user.Settings; s.Profile = new UserProfile(); Update(user)`, with `Include(u => u.Settings)`,
    requiring `Settings.Profile`;
  - `var u2 = user; u2.Profile = new UserProfile(); Update(user)`;
  - `user.Profile = new UserProfile(); Update(u2)` where `var u2 = user` (aliasing in the other direction).
- **Warning** (each breaks one soundness rule):
  - `s` reassigned;
  - `user` reassigned after `var s = user.Settings`;
  - `user.Settings = other.Settings` between the alias and the write;
  - an alias assigned in two branches.
- **No change for the write above the alias:** `var s = user.Settings; user.Settings = new(); Update(s)`
  requiring `Profile` → INCL002, because `s` still holds the old Settings.

## Verification
Run `dotnet test test/Saritasa.Tools.CodeAnalyzers.Tests`. All tests should be green, including the user's
original `app.Admin = new User { … }; return app;` case, which now goes through the alias map.

## Open points for discussion
1. Is rule 3 too strict? It is flow-insensitive, so a prefix write *before* the alias also disables it.
2. Should fields be roots? They can change outside the method, so this draft says no.
3. Should chained aliases go through bridges (`var first = users.First()`)? This draft says no.
