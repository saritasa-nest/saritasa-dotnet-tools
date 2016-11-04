Domain
======

Exceptions
----------

``DomainException`` - Exception indicates that something wrong happaned to domain part of application. Usually the message can be shown to user.

``CommandValidationException`` - Inherits from ``DomainException``. Contains result of command validation against data annotation attributes.

``NotFoundException`` - Inherits from ``DomainException``. Usually means that object you try to find by specified key is not in collection.

``SecurityException`` - Inherits from ``DomainException``. Means that user cannot access to any resource or do any operation because he has no permissions.

Domain Events
-------------

In application a common requirement is an action should be triggered as a consequence of some other action occurring. This is where you can use Domain Events.

    ::

        raise event ---> dispatch event ---> trigger listeners
