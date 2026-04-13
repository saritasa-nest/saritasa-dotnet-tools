# How to Install Analyzers

## STAN1001 Line Length
1. Add `max_line_length` with `130` value to `.editorconfig`
2. Install the latest version of `Saritasa.Tools.CodeAnalyzers` NuGet
3. Fix all appeared warnings

## STAN1002 Exception Message Dot
1. Install the latest version of `Saritasa.Tools.CodeAnalyzers` NuGet
2. Fix all appeared warnings

## STAN1003 Singular Type Name
1. Install the latest version of `Saritasa.Tools.CodeAnalyzers` NuGet
2. Fix all appeared warnings
3. If you want to allow some plural words in type name you can add them in `.editorconfig` in that way: `dotnet_diagnostic.STAN1003.allowed_plural_words = Accounts, Items`

## STAN1004 Spellcheck
1. Install the latest version of `Saritasa.Tools.CodeAnalyzers` NuGet
2. Add or update `Directory.Build.props` with content (path to file can be changed, `$(MSBuildThisFileDirectory)` is the path to the `Directory.Build.props`):
```
<ItemGroup>
  <AdditionalFiles Include="$(MSBuildThisFileDirectory)dictionaries\exclusions.txt"/>
</ItemGroup>
```
3. Create `exlusions.txt` by selected path to have ability to use code fix for adding project related words to whitelist
4. Fix all appeared warnings
