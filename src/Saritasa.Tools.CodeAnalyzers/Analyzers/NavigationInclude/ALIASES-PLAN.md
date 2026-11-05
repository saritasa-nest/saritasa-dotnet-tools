# Member assignments through aliases and loops

## Summary
The analyzer accepts `user.Settings.Profile = x` as loading `Settings.Profile` on `user`. This plan makes it
accept the same assignment written through **another name** for the object, and a `foreach` that assigns
every element:

```csharp
var settings = user.Settings;
settings.Profile = new UserProfile();      // assigns user.Settings.Profile
UpdateTimezone(user, dto);                 // requires Settings.Profile: no warning

foreach (var u in users) u.Settings.Profile = new UserProfile();
UpdateTimezones(users, dto);               // requires Settings.Profile on every element: no warning
```

The search stays **backward only**. When it meets an assignment through a name it cannot place yet
(`settings`), it keeps it as a **pending write** and reads on until that name is traced to its root. The
search on a path ends only when every pending write is resolved or dropped.

## What is handled
`user` comes from `.Include(u => u.Settings)`, `users` from `.Include(u => u.Settings).ToListAsync()`. The code
ends with a call that requires `Settings.Profile`.

| # | Code | Result |
|---|---|---|
| 1 | `var s = user.Settings; s.Profile = new UserProfile();` | no warning |
| 2 | `var t = user; t.Settings = new UserSettings { Profile = new UserProfile() };` | no warning |
| 3 | `var t = user; var s = t.Settings; s.Profile = …;` | no warning |
| 4 | `user.Settings ??= new UserSettings { Profile = … };` | INCL002 unless the query loaded `Settings.Profile` |
| 5 | `var s = user.Settings ?? throw …; s.Profile = …;` | no warning |
| 6 | `var s = user.Settings; await strategy.ExecuteAsync(() => { s.Profile = …; UpdateTimezone(user, dto); });` | no warning |
| 7 | `foreach (var u in users) u.Settings.Profile = …;` | no warning |

And these keep warning, because the name no longer holds the same object, or not every element is
assigned:

| # | Code | Why |
|---|---|---|
| 8 | `var s = user.Settings; s = other.Settings; s.Profile = …;` | `s` is the Settings of `other` |
| 9 | `var s = user.Settings; user = other; s.Profile = …;` | `s` belongs to the old `user` |
| 10 | `var s = user.Settings; user.Settings = other.Settings; s.Profile = …;` | `s` is the old Settings object |
| 11 | `var s = reset ? user.Settings : other.Settings; s.Profile = …;` | on the `other` path nothing sets it |
| 12 | `if (reset) { var s = user.Settings; s.Profile = …; }` | on the other branch nothing sets it |
| 13 | `foreach (var u in users) { if (reset) continue; u.Settings.Profile = …; }` | some elements are skipped |

A new object counts as loaded (`user.Settings = new UserSettings()` gives no warning), so the warning cases
store a queried object: `other.Settings`.

## How it works

### Today
`WritesWalker.FindWrites` reads upward from the place of use and stops at the first write of the variable.
`ReadAssignedCount` accepts `user.Settings.Profile = x` as a write when the target's chain starts at the
variable itself. `IncludeSearcher.ReadMemberWrite` then searches the part of the path **in front** of the
assigned property on the variable above the assignment, and the part **under** it in the assigned value.

### Pending writes
Each path of the walk carries a small state:

```csharp
record PathState(
    ImmutableList<PendingWrite> Pending,  // closest to the place of use first
    Write? Candidate);                    // the best write found on this path so far

record PendingWrite(
    object Name,                          // the name the write went through: settings, then user
    ImmutableArray<string> Chain,         // [Profile], then [Settings, Profile]
    CodePosition Assignment,              // where the write happened
    ImmutableHashSet<object> Written,     // names written since the assignment
    ImmutableHashSet<string> Stored);     // properties stored since the assignment
```

Reading a statement upward, searching `user` for `Settings.Profile`:

| Statement | What happens |
|---|---|
| `s.Profile = x`, where `s` is not `user` | a new pending `(s, [Profile])`, if `[Profile]` is a run of segments of the path and there is no candidate yet |
| `s = t.Settings`, where `s` is pending | the pending becomes `(t, [Settings, Profile])` |
| the pending reaches `user` and the path starts with its chain | **resolved**: `MemberWritten` at its assignment becomes the candidate |
| `s = new …`, `s = Call()`, a parameter, the start of the method | **dropped**: the write went into some other object |
| a pending's new root is in its `Written`, or its chain has a property in its `Stored` | **dropped**: the name no longer holds the same object (cases 8–10) |
| a write of `user` itself (`user = …`, `user.Settings = …`) | becomes the candidate if there is none yet |
| every statement | its written names and stored properties are added to every pending |

- **A candidate stops the collecting.** A pending above the candidate cannot win, because the closest write
  to the place of use wins. Pendings below it can still resolve and replace it.
- **The path ends** when there is a candidate and nothing is pending. It returns the candidate, or, at the
  start of the method, what `FindWrites` returns today. `IncludeSearcher` does not change: it still gets one
  `Write` per path.
- **Dropping is always safe.** A dropped pending is not a write, so the search goes on to the query and may
  warn.

### Captures
The compiler writes `user.Settings = new UserSettings { Profile = … }` as:

```
#0 = user.Settings      // the place
#1 = new UserSettings()
#1.Profile = …
#0 = #1                 // the assignment
```

An assignment to a bare capture (`#0 = #1`) becomes a pending with an empty chain, `(#0, [])`. `#0 = user.Settings`
resolves it to `(user, [Settings])`. `?? throw` and `??=` resolve through captures the same way.
`ReadThroughCapture` is deleted.

### Paths and loops
The walker already forks at every block with several ways in, and each fork gets its own copy of the
state. So `?:` (case 11) resolves on the `user` path and drops on the `other` path, with no merge rule.

The key of the visited blocks becomes `(block, variable, path, state)`. A loop that comes back with the same
state returns `NothingNew`, as it does today. A loop that comes back with a different state is read again
with that state. The states are finite, so the walk stops. Example:

```csharp
var s = user.Settings;
while (reset)
{
    s.Profile = new UserProfile();    // (A)
    UpdateTimezone(user, dto);        // (U)
    user.Settings = other.Settings;   // (B)
}
```

From (U): (A) gives the pending `(s, [Profile])`. On the way in from in front of the loop, `s = user.Settings`
resolves it, so (A) loads it. On the way in through the back edge, (B) becomes the candidate and adds
`Settings` to `Stored`. `s = user.Settings` then drops the pending, so the path ends at (B), the Settings of
`other`. **INCL002**, which is right for the second round.

### Lambdas
- **Inside a lambda (case 6),** a pending that is not resolved at the start of the body goes on reading in
  front of the statement that creates the lambda, as a captured variable does today.
- **Writes made by a lambda or local function can happen at any moment,** and the walker does not see when.
  So a pending is dropped when its root, or a property of its chain, is written in any lambda or local
  function of the method other than the body being read. That list is read once per method.

### `foreach`
A pending write is about one object, so it cannot say "every element". The loop is recognized by its
shape instead, in the operation tree:

```csharp
foreach (var u in users)          // users, or a chain on it: user.Applications
{
    u.Settings.Profile = …;       // a top-level statement of the body, rooted at u
}
```

Conditions:
1. **The loop goes over the searched variable itself**, or a chain on it, and the path starts with
   `chain + [Settings, Profile]`.
2. **The collection is an array, or implements `ICollection<T>` or `IReadOnlyCollection<T>`.** Enumerating it
   again gives the same objects; a `.Where(…)` or an `IQueryable` does not qualify.
3. **The assignment is a top-level statement of the body.**
4. **The body has no `break` or `continue`** for this loop.

When the walker reaches the loop's exit block and the loop qualifies, the loop counts as a write of the
searched variable: `MemberWritten` at the assignment in the body. The part in front is searched above the
loop, the part under it in the assigned value.

## Not handled

### Missing include, no warning
The walker does not read the code that does these writes:

```csharp
var s = user.Settings;
ResetSettings(user);            // inside: user.Settings = other.Settings
s.Profile = new UserProfile();
UpdateTimezone(user, dto);      // no warning, but user.Settings is now other's, without Profile
```

```csharp
foreach (var u in users) u.Settings.Profile = new UserProfile();
users.Add(other);               // or FillUsers(users)
UpdateTimezones(users, dto);    // no warning, but other has no Profile
```

Both are known limits of the whole analyzer: called methods are not read, and `users.Add(other)` without any
loop gives no warning today either.

### Warns, although the code is fine

```csharp
var s = user.Settings;
user.Settings.Profile = new UserProfile();
UpdateTimezone2(s, dto);        // INCL002: the alias is on the side of the searched variable
// workaround: UpdateTimezone2(user.Settings, dto);
```

```csharp
var s = GetSettings(user);      // a call result is a root, not user.Settings
s.Profile = new UserProfile();
// workaround: var s = user.Settings;
```

```csharp
users[0].Profile = new UserProfile();
UpdateUserProfile(users[0], dto);   // an element is not a name
// workaround: var first = users[0]; first.Profile = …; UpdateUserProfile(first, dto);
```

```csharp
var t = user;
user.Settings = new UserSettings();
var s = t.Settings;             // Settings was stored above, but after the assignment's chain started
s.Profile = new UserProfile();  // INCL002: Stored covers everything from the assignment up to the root
// workaround: var s = user.Settings;
```

```csharp
foreach (var u in users) { var s = u.Settings; s.Profile = …; }   // not a top-level u.… assignment
for (var i = 0; i < users.Count; i++) users[i].Settings.Profile = …;
users.ForEach(u => u.Settings.Profile = …);
// workaround: foreach (var u in users) u.Settings.Profile = …;
```

Fields are INCL004, as everywhere.

## Implementation
Each commit builds and keeps every test green.

### Commit 1: pending writes through locals
- `Search/WritesWalker.cs`:
  - `PathState` and `PendingWrite` (private nested records, or a new `Search/PathState.cs`).
  - `FindWrites(variable, position, path)` keeps its signature and starts with an empty state. A private
    overload takes the state, and `FindWritesBeforeBlock` and `FindWritesBeforeBody` pass it on.
  - `GetWriteOrDefault` becomes a step: a statement returns either a final `Write` or the new state. The
    rules are the table above, for locals and parameters only.
  - `visitedBlocks` key: `(BasicBlock, object, NavigationPath, PathState)`; `PathState` compares by value.
- Tests (`LocalVariableTests`): cases 1, 3, 8–12, the `while` example.

### Commit 2: captures
- `WritesWalker`: an assignment to a bare capture becomes a pending with an empty chain; `#0 = …` resolves it.
  `ReadThroughCapture` is deleted.
- Tests: cases 2, 4 (with and without `ThenInclude`), 5.

### Commit 3: lambdas
- `WritesWalker.FindWritesBeforeBody`: the state goes on to the creation statement.
- The names written and properties stored in the lambdas and local functions of a method: read once and kept
  on the method's `FlowGraph`, which lambdas and local functions reach through their creator.
- Tests: case 6; `Action reset = () => user.Settings = other.Settings; var s = user.Settings; reset(); s.Profile = …;`
  reports INCL002.

### Commit 4: `foreach`
- `WritesWalker.FindWritesBeforeBlock`: at a loop's exit block, find its `IForEachLoopOperation` and check
  conditions 1–4; when they hold, return the `MemberWritten`.
- Tests: case 7, case 13, and the same loop with `break` and with `.Where(…)`.

### Commit 5: documentation
- `README.md`: an "Assignments through other names and loops" subsection with the tables from
  [What is handled](#what-is-handled) and the examples from [Not handled](#not-handled).

## Verification
`dotnet test test/Saritasa.Tools.CodeAnalyzers.Tests`: all tests green after every commit.
