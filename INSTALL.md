Project Setup
=============

Here are steps you need to do to setup environment to be able to develop. You need following software installed:

- Visual Studio 2017 (https://www.visualstudio.com/downloads/download-visual-studio-vs.aspx) or JetBrains Rider (https://www.jetbrains.com/rider/)
- psake (https://github.com/psake/psake)
- PowerShell 5
- .NET Core (https://www.microsoft.com/net/core)

For documentation:

- python 3 (`choco install python`)
- sphinx (`pip install sphinx`)
- read the docs theme (`pip install sphinx_rtd_theme`)

Code Style Setup
----------------

We are using StyleCop.Analyzers project for extended code style check. It should be installed for every project in solution:

```
Install-Package StyleCop.Analyzers
```

There is `scripts\Saritasa.RulesetInstall.ps1` script that allows to setup ruleset for any project within current directory. It should be run within project repository root. If `/tools/Saritasa.ruleset` is found the ruleset file will be used. Example:

```
PS W:\crm> W:\SaritasaTools\scripts\Saritasa.RulesetInstall.ps1 -exclude OldOrmProject.*
```

To install StyleCop analyzers for all projects within solution run following command in package manager console:

```
Get-Project -All | Install-Package StyleCop.Analyzers
```
