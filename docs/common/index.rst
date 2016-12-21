Common
======

From day to day developing using .NET standard library sometimes you get in idea in mind "that little feature would be useful to have in all my projects". It could me just few lines of code you keep repeating around. Usually in that cases we do *Helpers* classes that migrate from project to project.

We tried to collect such little things in one place to be able to use it in all projects. Also we focused to have good documentation that would list all helpers and where these can be used.

.. toctree::
    :maxdepth: 1

    atomic
    extensions
    flow
    guard
    pagination
    security
    string

Overview
--------

Here are some examples to use. Calculate SHA256 hash for password:

    .. code-block:: c#

        SecurityUtils.Sha256("mypassword");

Set of functions to parse or get value or default:

    .. code-block:: c#

        StringUtils.ParseDefault("incorrect", 1); // returns 1
        StringUtils.ParseDefault("incorrect", false) // returns false
        dict.GetValueDefault(5, "default") // default if dict has no 5 key

Format string:

    .. code-block:: c#
    
        "{0} + {1} = {2}".FormatWith(2, 2, 4)  // returns "2 + 2 = 4"

String handy extensions to check for empty/not empty that easier to read:

    .. code-block:: c#

        if (str.IsNotEmpty()) ...
        if (str.IsEmpty()) ...
        dbuser.name = user.name.NullSafe() ...

Chunk select:

    .. code-block:: c#

        foreach (var item in list.ChunkSelect(50)) ...  // returns items from source by 50

Frameworks
----------

* .NET 4.0
* .NET 4.5.2
* .NET 4.6.1
* .NET Core 1.0
* .NET Core 1.1
* .NET Standard 1.2
* .NET Standard 1.6
