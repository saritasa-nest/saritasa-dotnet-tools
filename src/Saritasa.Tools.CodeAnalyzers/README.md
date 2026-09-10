C# code analyzers
==============

The tool contains C# code analyzers to prevent specific issues that might occur during development.

## How to setup:

Add a package as a reference. See the [latest version on NuGet](https://www.nuget.org/packages/Saritasa.Tools.CodeAnalyzers).

```xml
<PackageReference Include="Saritasa.Tools.CodeAnalyzers" Version="0.2.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

If your project uses a [global package reference](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#global-package-references) (`GlobalPackageReference` in `Directory.Packages.props`) and you need to use attributes such as `[TrackIncludeRequired]`, `[IncludeRequired]`, or `[Includes]` from this package in your code, you still need to add a regular `PackageReference` to the project. Global package references are designed for analyzers and don't include compile-time assets (the `lib` assembly), so the attributes won't be available for direct use in code without an explicit `PackageReference`.

## Analyzers

| ID | Title | Severity | Category |
|----|-------|----------|----------|
| [STAN1000](#stan1000-request-handler-return-type) | MediatR request handler must have a return type | Warning | Design |
| [STAN1001](#stan1001-line-length) | Line exceeds maximum length | Warning | Style |
| [STAN1002](#stan1002-exception-message-dot) | Exception message should end with a dot | Warning | Spelling |
| [STAN1003](#stan1003-singular-type-name) | Type names should use singular nouns | Warning | Naming |
| [STAN1004](#stan1004-spelling) | Word '{0}' has a typo | Warning | Spelling |
| [STAN1005](#stan1005-early-exit) | Use early return instead of else after return | Warning | Style |
| [INCL001](#incl001-navigation-property-not-checked) | Method parameter should require a navigation property | Warning | Usage |
| [INCL002](#incl002-local-variable-missing-include) | Local variable does not set the required navigation property | Warning | Usage |
| [INCL003](#incl003-includes-promise-not-fulfilled) | Method declares [Includes] but return value does not load the required navigation property | Warning | Usage |

---

### STAN1000: Request handler return type

Triggered when a MediatR `IRequestHandler<TRequest>` implementation does not declare a return type. Handlers should always return a value to maintain a consistent and testable API.

#### Code causing a warning

```csharp
public class TestRequestHandler : IRequestHandler<TestRequest>
{
    public Task Handle(TestRequest request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
```

#### Code causing no warning

```csharp
public class TestRequestHandler : IRequestHandler<TestRequest, int>
{
    public Task<int> Handle(TestRequest request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
```

---

### STAN1001: Line length

Triggered when a source line exceeds the configured maximum length. Keeping lines short improves readability and avoids horizontal scrolling.

The default limit is **130 characters**.

#### Code causing a warning

```csharp
// Line below is 145 characters — exceeds the default limit of 130
public void SomeMethod(string firstArgument, string secondArgument, string thirdArgument, string fourthArgument) { }
```

#### Code causing no warning

```csharp
public void SomeMethod(
    string firstArgument,
    string secondArgument)
{ }
```

---

### STAN1002: Exception message dot

Triggered when an exception is created with a `message` argument that does not end with a dot (`.`). This enforces consistent punctuation in exception messages.

#### Code causing a warning

```csharp
throw new InvalidOperationException("Something went wrong");
```

#### Code causing no warning

```csharp
throw new InvalidOperationException("Something went wrong.");
```

#### Code fix

A code fix is available: applying it appends a dot to the exception message. You can use this for fixing all warnings at once.

---

### STAN1003: Singular type name

Triggered when a class or interface name used as a `Controller` or `Service` contains a plural noun (e.g. `UsersController`). Type names should use singular forms.

Certain words are allowed to remain plural by default: `News`, `Settings`, `Options`, `Analytics`, `Physics`, `Mathematics`, `Statics`, `Dynamics`, `Glass`, `Class`, `Gas`, `Bus`, `Cors`, `Status`.

Additional exceptions can be configured per project:

#### Code causing a warning

```csharp
public class UsersController : ControllerBase { }

public interface IOrdersService { }
```

#### Code causing no warning

```csharp
public class UserController : ControllerBase { }

public interface IOrderService { }

// Allowed plural word — no warning
public class SettingsController : ControllerBase { }
```

---

### STAN1004: Spelling

Triggered when a word in an **identifier**, **string literal**, or **comment** is not found in the built-in English dictionary or the configured exclusions list. Uses [Hunspell](https://hunspell.github.io/) under the hood.

The analyzer checks:
- Identifiers (class names, method names, variable names, etc.)
- String literals (regular, interpolated, raw)
- Single-line and multi-line comments
- XML documentation comments (`///`)

The following are automatically ignored: GUIDs, URLs, hex values, file paths, and format strings (e.g. `{0}`).

#### Excluding words

Words that are valid for your project can be excluded by adding them (one per line) to an exclusions file.

By default the analyzer looks for a file named `spell-checker-exclusions.txt` registered as an `AdditionalFiles` entry. Add the following to `Directory.Build.props`:

```xml
<ItemGroup>
  <AdditionalFiles Include="$(MSBuildThisFileDirectory)dictionaries/spell-checker-exclusions.txt" />
</ItemGroup>
```

You can choose a different file path and tell the analyzer about it via `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.STAN1004.exclusions_file = dictionaries/spell-checker-exclusions.txt
```

A **code fix** is available: applying it appends the flagged word to the exclusions file automatically.

#### Code causing a warning

```csharp
// "typoo" is not a valid English word

var typoo = "It's string literal with a typoo."; // identifier and string both warned

// It's single line comment with a typoo.

/// <summary>
/// Method with a typoo.
/// </summary>
public void MethodWithTypoo() { }
```

#### Code causing no warning

```csharp
// Correct spelling

var typo = "It's string literal without a typo.";

// It's single line comment without a typo.

/// <summary>
/// Correct method name.
/// </summary>
public void MethodWithCorrectName() { }

// Words in 'spell-checker-exclusions.txt' are also allowed
var linq = "linq"; // "linq" is in the built-in general exclusions list
```

---

### STAN1005: Early exit

Triggered when an `else` block follows an `if` branch that already exits (via `return` or `throw`). The `else` is unnecessary in that case and should be removed to flatten the control flow.

#### Code causing a warning

```csharp
if (flag)
{
    return "true";
}
else              // <-- warning here
{
    return "false";
}
```

```csharp
if (flag)
{
    throw new InvalidOperationException("Error.");
}
else              // <-- warning here
{
    return "ok";
}
```

#### Code causing no warning

```csharp
if (flag)
{
    return "true";
}
return "false";
```

```csharp
if (flag)
{
    // does not exit
    Console.WriteLine();
}
else
{
    return "false";
}
return "other";
```

---

### Navigation Include attributes

Three attributes control which navigation properties are tracked and how inclusion requirements are communicated between methods.

| Attribute | Target | Purpose |
|-----------|--------|---------|
| `[TrackIncludeRequired]` | Property | Marks a navigation property as requiring explicit loading. Only properties with this attribute are checked by INCL rules. |
| `[IncludeRequired("param", "Property")]` | Method | Declares that the named parameter must have the named property loaded before the method is called. Repeatable. |
| `[Includes("Property")]` | Method | Promises that the method's return value has the named property loaded. Repeatable. |

Example model used in the sections below:

```csharp
class User
{
    public Organization Organization { get; set; }

    [TrackIncludeRequired]  // Only Profile is tracked, Organization produces no diagnostics.
    public UserProfile Profile { get; set; }
}
```

---

### INCL001: Navigation property not checked

Triggered when a method directly accesses a `[TrackIncludeRequired]` property on a parameter, or passes a parameter to a method that requires it via `[IncludeRequired]`, without declaring a matching `[IncludeRequired]` on the current method.

#### Code causing a warning

```csharp
void SetTimezone(User user, string timezone)
{
    // INCL001: Profile has [TrackIncludeRequired] but no [IncludeRequired(nameof(user), nameof(User.Profile))] on this method
    user.Profile.Timezone = timezone;
}
```

#### Or

```csharp
 // Uncommenting this line will fix the INCL001 warning.
// [IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User user, SaveUserDto dto)
{
    // Does not produce INCL001 because Organization has no [TrackIncludeRequired] attribute.
    user.Organization = dto.Organization;
    // INCL001: the called method captures the `user` argument and has [IncludeRequired] attribute.
    // Use IncludeRequiredAttribute on the current method as well.
    SetTimezone(user, dto.Timezone);
}

[IncludeRequired(nameof(user), nameof(User.Profile))]
void SetTimezone(User user, string timezone)
{
    // Does not produce INCL001 because the method has [IncludeRequired] attribute.
    user.Profile.Timezone = timezone;
}
```

#### Fixed

```csharp
[IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User entity, SaveUserDto dto)
{
    user.Organization = dto.Organization;
    SetTimezone(entity, dto.Timezone);
}

[IncludeRequired(nameof(user), nameof(User.Profile))]
void SetTimezone(User user, string timezone)
{
    user.Profile.Timezone = timezone;
}
```

---

### INCL002: Local variable missing include

Triggered when a local variable is passed to a method that requires a navigation property via `[IncludeRequired]`, but the variable was neither loaded with `.Include()`, set in an object initializer, nor returned from a method annotated with `[Includes]`.

#### Code causing a warning

```csharp
[IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User entity, SaveUserDto dto)
{
    user.Organization = dto.Organization;
    SetTimezone(entity, dto.Timezone);
}

async Task Handle(SaveUserDto dto)
{
    var user = await _dbContext.Users
        //.Include(u => u.Profile) // Uncommenting this line would fix the INCL002 warning.
        .FirstOrDefaultAsync(u => u.Id == dto.Id);
    // INCL002: the called method requires User.Profile, but it is not Included in the query.
    UpdateUserProfile(user, dto);
}
```

#### Fixed

```csharp
[IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User entity, SaveUserDto dto)
{
    user.Organization = dto.Organization;
    SetTimezone(entity, dto.Timezone);
}

async Task Handle(SaveUserDto dto)
{
    var user = await _dbContext.Users
        .Include(u => u.Profile) // Load the property directly.
        .FirstOrDefaultAsync(u => u.Id == dto.Id);

    UpdateUserProfile(user, dto);
}
```

#### Code causing a warning

```csharp
[IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User entity, SaveUserDto dto)
{
    user.Organization = dto.Organization;
    SetTimezone(entity, dto.Timezone);
}

async Task Handle2(SaveUserDto dto)
{
    var user = new User
    {
        Id = dto.Id,
        Organization = dto.Organization,
        //Profile = new UserProfile { Timezone = dto.Timezone } // Uncommenting this line would fix the INCL002 warning.
    };
    // INCL002: the called method requires User.Profile, but it is not set.
    UpdateUserProfile(user, dto);
}

```

#### Fixed

```csharp
[IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User entity, SaveUserDto dto)
{
    user.Organization = dto.Organization;
    SetTimezone(entity, dto.Timezone);
}

async Task Handle2(SaveUserDto dto)
{
    var user = new User
    {
        Id = dto.Id,
        Organization = dto.Organization,
        Profile = new UserProfile { Timezone = dto.Timezone } // Load the property directly.
    };

    UpdateUserProfile(user, dto);
}
```

#### Code causing a warning

```csharp
[IncludeRequired(nameof(user), nameof(User.Profile))]
void UpdateUserProfile(User entity, SaveUserDto dto)
{
    user.Organization = dto.Organization;
    SetTimezone(entity, dto.Timezone);
}

async Task Handle3(SaveUserDto dto)
{
    var user = await GetUser(dto.Id);
    // INCL002: the called method requires User.Profile, but it is not checked.
    UpdateUserProfile(user, dto);
}
// [Includes(nameof(User.Profile))] // Uncommenting this line would fix the INCL002 warning.
async Task<User> GetUser(int id)
{
    return _dbContext.Users
        .Include(u => u.Profile)
        .FirstOrDefault(u => u.Id == id);
}
```

#### Fixed

```csharp
// Fix 2: annotate the source method with [Includes]
[Includes(nameof(User.Profile))]
async Task<User> GetUser(int id)
{
    return _dbContext.Users
        .Include(u => u.Profile)
        .FirstOrDefault(u => u.Id == id);
}

async Task Handle3(SaveUserDto dto)
{
    var user = await GetUser(dto.Id); // [Includes(nameof(User.Profile))] satisfies the requirement.
    UpdateUserProfile(user, dto);
}
```

---

### INCL003: \[Includes\] promise not fulfilled

Triggered when a method annotated with `[Includes("Property")]` returns a value that does not load the named property via `.Include()`, an object initializer, or a source method annotated with `[Includes]`.

#### Code causing a warning

```csharp
[Includes(nameof(User.Profile))]
async Task<User> GetUser(int id)
{
   // INCL003: the method has [Includes] attribute for User.Profile, but Profile is not Included in the query.
    return _dbContext.Users
        //.Include(u => u.Profile) // Uncommenting this line would fix the INCL003 warning.
        .FirstOrDefault(u => u.Id == id);
}
```

#### Fixed

```csharp
[Includes(nameof(User.Profile))]
async Task<User> GetUser(int id)
{
    return _dbContext.Users
        .Include(u => u.Profile) // Load the property directly.
        .FirstOrDefault(u => u.Id == id);
}
```

#### Code causing a warning

```csharp
[Includes(nameof(User.Profile))]
async Task<User> CreateUser(SaveUserDto dto)
{
    // INCL003: the method has [Includes] attribute for User.Profile, but Profile is not set.
    var user = new User
    {
        Id = dto.Id,
        Organization = dto.Organization,
        //Profile = new UserProfile { Timezone = dto.Timezone } // Uncommenting this line would fix the INCL003 warning.
    };
    return user;
}
```

#### Fixed

```csharp
[Includes(nameof(User.Profile))]
Task<User> CreateUser(SaveUserDto dto)
{
    var user = new User
    {
        Id = dto.Id,
        Profile = new UserProfile { Timezone = dto.Timezone }, // Load the property directly.
    };
    return Task.FromResult(user);
}
```
