Saritasa Tools
==============

An infrastructure components and development tools for company projects. Read the documentation on [Read the Docs](http://saritasa-tools.readthedocs.io/en/latest/index.html).

Overview
--------

1. Saritasa.Tools.Common - _various utilities (validation, flow, security, atomic), extensions (dict, datetime, string), pagination;_
1. Saritasa.Tools.Domain - _general interfaces: repository, unit of work, domian events; exceptions;_
1. Saritasa.Tools.Emails - _intefaces for system emails;_
1. Saritasa.Tools.Messages - _commands, queries, events: pipeline to process and log system queries/actions;_
1. Saritasa.Tools.Misc - _miscellaneous: password generation;_
1. Saritasa.Tools.Ef6 - _unit of work and repository implementation for Entity Framework 6;_
1. Saritasa.Tools.EfCore1 - _unit of work and repository implementation for Entity Framework Core;_
1. Saritasa.Tools.Mvc5 - _utilities for ASP.NET MVC 5;_
1. Saritasa.Tools.NLog4 - _Microsoft common logger abstractions implementation for NLog 4;_

Goals
-----

1. Provide common infrastructure for our projects. Make the same or similar vision of current patterns like Repository, Unit of Work, etc.

1. Provide flexible and extensible infrastructure to manage business requirements.

1. Provide common logging infrastructure to easily track application activity and system bottlenecks.

1. Arrange best practices and common functionality among our projects.

Installation
------------

```
PM> Install-Package Saritasa.Tools
```

Project Setup
-------------

Here are steps you need to do to setup environment to be able to develop. You need following software installed:

- Visual Studio 2015 (https://www.visualstudio.com/downloads/download-visual-studio-vs.aspx)
- psake (https://github.com/psake/psake)
- PowerShell 5
- GitVersion (`choco install gitversion.portable`)

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

Then you need to install our codestyle ruleset. To do that just run the following cmd command under administrator:

```
cmd:
@powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/src/Saritasa.Tools/SaritasaRulesetInstall.ps1'))" && SET PATH=%PATH%;

PowerShell.exe (Ensure Get-ExecutionPolicy is at least RemoteSigned):
iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/src/Saritasa.Tools/SaritasaRulesetInstall.ps1'))

PowerShell v3+ (Ensure Get-ExecutionPolicy is at least RemoteSigned):
iwr https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/src/Saritasa.Tools/SaritasaRulesetInstall.ps1 -UseBasicParsing | iex
```

After that the "Saritasa Code Rules" will be available in "Code Analysis" in project properties.

Commands
--------

* Build the library, test it and prepare nuget packages

```psake pack```

* Generate documentation

```psake docs```

* Clean project

```psake clean```
