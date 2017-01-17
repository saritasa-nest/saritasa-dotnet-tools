Domain
======

Exceptions
----------

``DomainException`` - Exception indicates that something wrong happaned to domain part of application. Usually the message can be shown to user.

``CommandValidationException`` - Inherits from ``DomainException``. Contains result of command validation against data annotation attributes.

``NotFoundException`` - Inherits from ``DomainException``. Usually means that object you try to find by specified key is not in collection.

``ForbiddenException`` - Inherits from ``DomainException``. Means that user cannot access to any resource or do any operation because he has no permissions.

``UnauthorizedException`` - Inherits from ``DomainException``.

``ConflictException`` - Inherits from ``DomainException``. Indicates that the request could not be processed because of conflict in the request, such as an edit conflict between multiple simultaneous updates.

``ValidationException`` - Inherits from ``DomainException``. Contains errors dictionary.

Domain Events
-------------

In application a common requirement is an action should be triggered as a consequence of some other action occurring. This is where you can use Domain Events.

    ::

        raise event ---> dispatch event ---> trigger listeners
