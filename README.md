Saritasa Tools
==============

Infrastructure components and development tools for company projects. Read the latest documentation on [Read the Docs](http://saritasa-tools.readthedocs.io/en/latest/index.html).

Overview
--------

1. [Saritasa.Tools.Common](https://www.nuget.org/packages/Saritasa.Tools.Common) - _various utilities (validation, flow, security, atomic), extensions (dict, datetime, string), pagination;_
2. [Saritasa.Tools.Domain](https://www.nuget.org/packages/Saritasa.Tools.Domain) - _general interfaces: repository, unit of work, domain events; exceptions;_
3. [Saritasa.Tools.Emails](https://www.nuget.org/packages/Saritasa.Tools.Emails) - _intefaces for system emails;_
4. [Saritasa.Tools.Misc](https://www.nuget.org/packages/Saritasa.Tools.Misc) - _miscellaneous: password generation;_
5. [Saritasa.Tools.EF6](https://www.nuget.org/packages/Saritasa.Tools.EF6) - _unit of work and repository implementation for Entity Framework 6;_
6. [Saritasa.Tools.EFCore3](https://www.nuget.org/packages/Saritasa.Tools.EFCore3) - _unit of work and repository implementation for Entity Framework Core 3;_

Goals
-----

1. Provide a common infrastructure for our projects. Make the same or similar vision of current patterns like Repository, Unit of Work, etc.

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

* Build the library, test it, and prepare NuGet packages.

    `psake pack`

* Clean project.

    `psake clean`

Contributors
------------

* Saritasa http://www.saritasa.com

License
-------

The project is licensed under the terms of the BSD license. Refer to LICENSE.txt for more information.
