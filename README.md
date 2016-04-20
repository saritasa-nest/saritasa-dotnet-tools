Saritasa Tools
==============

An infrastructure components and development tools for company projects.

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
- PowerShell

For documentation:

- python 3 (`choco install python`)
- sphinx  (`pip install sphinx`)
- read the docs theme (`pip install sphinx_rtd_theme`)

Commands
--------

* Build the solution and prepare nuget package

```psake build```

* Generate documentation

```psake build-doc```
