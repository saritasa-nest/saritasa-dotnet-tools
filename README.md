Saritasa Tools
==============

Infrastructure components and development tools for company projects. Read the latest documentation on [Wiki](https://github.com/Saritasa/SaritasaTools/wiki).

Overview
--------

1. [Saritasa.Tools.Common](https://www.nuget.org/packages/Saritasa.Tools.Common) - _various utilities (validation, flow, security, atomic), extensions (dict, datetime, string), pagination;_
2. [Saritasa.Tools.Domain](https://www.nuget.org/packages/Saritasa.Tools.Domain) - _general interfaces: repository, unit of work, domain events; exceptions;_
3. [Saritasa.Tools.Emails](https://www.nuget.org/packages/Saritasa.Tools.Emails) - _intefaces for system emails;_
4. [Saritasa.Tools.Misc](https://www.nuget.org/packages/Saritasa.Tools.Misc) - _miscellaneous: password generation;_
5. [Saritasa.Tools.EF6](https://www.nuget.org/packages/Saritasa.Tools.EF6) - _unit of work and repository implementation for Entity Framework 6;_
6. [Saritasa.Tools.EFCore3](https://www.nuget.org/packages/Saritasa.Tools.EFCore3) - _unit of work and repository implementation for Entity Framework Core 3;_
7. [Saritasa.Tools.EFCore5](https://www.nuget.org/packages/Saritasa.Tools.EFCore5) - _unit of work and repository implementation for Entity Framework Core 5;_
8. [Saritasa.Tools.PropertyChangedGenerator](https://www.nuget.org/packages/Saritasa.Tools.PropertyChangedGenerator) - _Source code generator for `PropertyChanged` and `PropertyChanging` events;_

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

Package Preparation
------------

**1.** Create `nuspec` template file. Add it into library folder.
> See an example file [here](https://github.com/Saritasa/SaritasaTools/blob/master/src/Saritasa.Tools.Common/Saritasa.Tools.Common.nuspec.template).

**2.** Create `CHANGELOG` & `VERSION` files. Add it into library folder.
> See an example files: [CHANGELOG](https://github.com/Saritasa/SaritasaTools/blob/master/src/Saritasa.Tools.Common/CHANGELOG.txt) and [VERSION](https://github.com/Saritasa/SaritasaTools/blob/master/src/Saritasa.Tools.Common/VERSION.txt).

**3.** Configure `Cake` build task.
> Go [here](https://github.com/Saritasa/SaritasaTools/blob/master/scripts/Modules/Saritasa.Cake/Tasks/PackTask.cs). Add your library folder into build task.

**4.** Create & Configure `AssemblyInfo.cs` file for your library.

NuGet package upload
--------

**1.** Build the library, test it, and prepare NuGet packages. 

    ./build.ps1
    
> Read an output logs if something went wrong.

**2.** Watch an output `nupkg` files in the executable directory.

**3.** Request [Saritasa](https://www.nuget.org/profiles/Saritasa) profile access.

**4.** [Upload](https://www.nuget.org/packages/manage/upload) NuGet package co authored with [Saritasa](https://www.nuget.org/profiles/Saritasa).

Contributors
------------

* Saritasa http://www.saritasa.com

License
-------

The project is licensed under the terms of the BSD license. Refer to LICENSE.txt for more information.
