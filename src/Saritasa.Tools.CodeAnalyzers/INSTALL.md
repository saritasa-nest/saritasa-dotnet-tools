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
2. Add `AdditionalFiles` with `Include` containing path to `spell-checker-exclusions.txt` file in `Directory.Build.props`. Like this:
   ```
   <ItemGroup>
     <AdditionalFiles Include="$(MSBuildThisFileDirectory)dictionaries/spell-checker-exclusions.txt"/>
   </ItemGroup>
   ```
   - You can choose any path for `spell-checker-exclusions.txt` file.
   - `$(MSBuildThisFileDirectory)` is the path to the directory that contains the `Directory.Build.props`
   - This file will contain word per line that should not be spell checked.
   - Note that the spellchecker accepts only one `AdditionalFiles` statement with `spell-checker-exclusions.txt` name.
     You might have other `AdditionalFiles` statements in `Directory.Build.props`,
     so please ensure that other analyzers do not use files with the same name.
3. Path to exclusions can be configured via `.editorconfig`: `dotnet_diagnostic.STAN1004.exclusions_file = path/to/exclusions.txt`
   - Please update the path in `Directory.Build.props` if you do this, they must be the same.
   - `dotnet_diagnostic.STAN1004.exclusions_file` value should be the end of the path in `AdditionalFiles`.
     - For example, value is `dictionaries/spell-checker-exclusions.txt` correct for `$(MSBuildThisFileDirectory)dictionaries/spell-checker-exclusions.txt` in `AdditionalFiles`.
4. Create file by the selected path to have ability to use code fix for adding project related words to whitelist
5. Fix all appeared warnings
