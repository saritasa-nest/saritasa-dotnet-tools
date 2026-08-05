C# code analyzers
==============

The tool contains C# code analyzers to prevent specific issues that might occur during development.

## How to setup:

Add a package as a reference. See the [latest version on NuGet](https://www.nuget.org/packages/Saritasa.Tools.CodeAnalyzers).

```xml
<PackageReference Include="Saritasa.Tools.CodeAnalyzers" Version="0.2.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## Analyzers

| ID | Title | Severity | Category |
|----|-------|----------|----------|
| [STAN1000](#stan1000-request-handler-return-type) | MediatR request handler must have a return type | Warning | Design |
| [STAN1001](#stan1001-line-length) | Line exceeds maximum length | Warning | Style |
| [STAN1002](#stan1002-exception-message-dot) | Exception message should end with a dot | Warning | Spelling |
| [STAN1003](#stan1003-singular-type-name) | Type names should use singular nouns | Warning | Naming |
| [STAN1004](#stan1004-spelling) | Word '{0}' has a typo | Warning | Spelling |
| [STAN1005](#stan1005-early-exit) | Use early return instead of else after return | Warning | Style |

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

The default limit is **130 characters**, following the [Saritasa C# style guide](https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#code-lines).

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

Triggered when an exception is created with a `message` argument that does not end with a dot (`.`). This enforces consistent punctuation in exception messages, following the [Saritasa C# style guide](https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#english-spelling).

#### Code causing a warning

```csharp
throw new InvalidOperationException("Something went wrong");
```

#### Code causing no warning

```csharp
throw new InvalidOperationException("Something went wrong.");
```

---

### STAN1003: Singular type name

Triggered when a class or interface name used as a `Controller` or `Service` contains a plural noun (e.g. `UsersController`). Type names should use singular forms, following the [Saritasa C# style guide](https://wiki.saritasa.rocks/dotnet/development/c-sharp-style-guide/#naming).

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
