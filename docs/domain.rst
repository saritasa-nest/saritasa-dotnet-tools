Domain
======

Exceptions
----------

``DomainException`` - Exception indicates that something wrong happaned to domain part of application. Usually the message can be shown to user.

``CommandValidationException`` - Inherits from ``DomainException``. Contains result of command validation against data annotation attributes.

``NotFoundException`` - Inherits from ``DomainException``. Usually means that object you try to find by specified key is not in collection.
