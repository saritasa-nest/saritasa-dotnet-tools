C# code analyzers
==============

The tool contains C# code analyzers to prevent specific issues that might occur during development.

## How to setup:

Add a package as a reference.

```xml
<PackageReference Include="Saritasa.Tools.CodeAnalyzers" Version="0.1.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```
## Examples:

### Request handlers analyzer

The warning `STAN1000` will be triggered every time the request handler from the MeditR package does not have a return type.

### Code causing a warning

```csharp
public class TestRequestHandler : IRequestHandler<TestRequest>
{
    public Task Handle(TestRequest request CancellationToken cancellationToken)
            => throw new NotImplementedException();
}
```

### Code causing no warning

```csharp
public class TestRequestHandler : IRequestHandler<TestRequest, int>
{
    public Task<int> Handle(TestRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
}
```
