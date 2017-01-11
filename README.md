Saritasa Tools
==============

An infrastructure components and development tools for company projects. Read the latest documentation on [Read the Docs](http://saritasa-tools.readthedocs.io/en/latest/index.html).

Overview
--------

1. [Saritasa.Tools.Common](https://www.nuget.org/packages/Saritasa.Tools.Common) - _various utilities (validation, flow, security, atomic), extensions (dict, datetime, string), pagination;_
1. [Saritasa.Tools.Domain](https://www.nuget.org/packages/Saritasa.Tools.Domain) - _general interfaces: repository, unit of work, domain events; exceptions;_
1. [Saritasa.Tools.Emails](https://www.nuget.org/packages/Saritasa.Tools.Emails) - _intefaces for system emails;_
1. [Saritasa.Tools.Messages](https://www.nuget.org/packages/Saritasa.Tools.Messages) - _commands, queries, events: pipeline to process and log system queries/actions;_
1. [Saritasa.Tools.Messages.Abstractions](https://www.nuget.org/packages/Saritasa.Tools.Messages.Abstractions) - _contains interfaces and base classes for package above with minimum dependencies;_
1. [Saritasa.Tools.Misc](https://www.nuget.org/packages/Saritasa.Tools.Misc) - _miscellaneous: password generation;_
1. [Saritasa.Tools.Ef6](https://www.nuget.org/packages/Saritasa.Tools.Ef6) - _unit of work and repository implementation for Entity Framework 6;_
1. [Saritasa.Tools.EfCore1](https://www.nuget.org/packages/Saritasa.Tools.EfCore1) - _unit of work and repository implementation for Entity Framework Core;_
1. [Saritasa.Tools.Mvc5](https://www.nuget.org/packages/Saritasa.Tools.Mvc5) - _utilities for ASP.NET MVC 5;_
1. [Saritasa.Tools.NLog4](https://www.nuget.org/packages/Saritasa.Tools.NLog4) - _Microsoft common logger abstractions implementation for NLog 4;_

Goals
-----

1. Provide common infrastructure for our projects. Make the same or similar vision of current patterns like Repository, Unit of Work, etc.

2. Provide flexible and extensible infrastructure to manage business requirements.

3. Provide common logging infrastructure to easily track application activity and system bottlenecks.

4. Arrange best practices and common functionality among our projects.

Installation
------------

```
PM> Install-Package Saritasa.Tools.<PackageName>
```

Commands
--------

* Build the library, test it and prepare nuget packages.

```psake pack```

* Generate documentation.

```psake docs```

* Clean project.

```psake clean```

Contributors
------------

* Saritasa http://www.saritasa.com
