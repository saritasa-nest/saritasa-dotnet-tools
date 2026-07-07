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
2. Add or update `Directory.Build.props` with content (path to file can be changed, `$(MSBuildThisFileDirectory)` is the directory path of the `Directory.Build.props`):
```
<ItemGroup>
  <AdditionalFiles Include="$(MSBuildThisFileDirectory)dictionaries\spell-checker-exclusions.txt"/>
</ItemGroup>
```
2.1 This file will contain word per line that should not be spell checked.
2.2 Note that the spellchecker accepts only one `AdditionalFiles` statement with `spell-checker-exclusions.txt` name.
You might have other `AdditionalFiles` statements in `Directory.Build.props`,
so please ensure that other analyzers do not use files with the same name.
3. Create `exclusions.txt` by selected path to have ability to use code fix for adding project related words to whitelist
4. Fix all appeared warnings
