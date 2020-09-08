Project Setup
=============

Here are steps you need to do to set up environment to be able to develop. You need the following software installed:

- Visual Studio 2019 (https://www.visualstudio.com/downloads/download-visual-studio-vs.aspx) or JetBrains Rider (https://www.jetbrains.com/rider/)
- psake (https://github.com/psake/psake)
- PowerShell 5
- .NET Core (https://www.microsoft.com/net/core)

Code Style Setup
----------------

We are using StyleCop.Analyzers project for an extended code style check. It should be installed for every project in solution:

```
Install-Package StyleCop.Analyzers
```

There is `scripts\Saritasa.RulesetInstall.ps1` script that allows to setup ruleset for any project within the current directory. It should be run within the project repository root. If `/tools/Saritasa.ruleset` is found the ruleset file will be used. Example:

```
PS W:\crm> W:\SaritasaTools\scripts\Saritasa.RulesetInstall.ps1 -exclude OldOrmProject.*
```

To install StyleCop analyzers for all projects within the solution run the following command in the package manager console:

```
Get-Project -All | Install-Package StyleCop.Analyzers
```
