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

Validation Exception
--------------------

This class is designed to be raised when any validation domain exception occurs and contains list of errors with related members (fields).

    .. attribute:: MessageFormatter

        Validation message formatter that can be access by ``Message`` property. Should contain logic how to get summary string from errors dictionary. There are following formatters available in ``ValidationExceptionDelegates`` class:

            - ``SummaryOrDefaultMessageFormatter``. Returns summary message if specific key exists or default one.
            - ``FirstErrorOrDefaultMessageFormatter``. Returns first available validation error. If no errors exist just return default message.
            - ``GroupErrorsOrDefaultMessageFormatter``. Group messages by fields.
            - ``ListErrorsOrDefaultMessageFormatter``. List error messages on separate lines.

    .. attribute:: Message

        Inherits from ``Exception``. Message the describes current erros. Intended for user.

    .. attribute:: SummaryErrors

        Summary errors string. The error that is not refer to any field. General error. Described by empty string key in errors dictionary.

    .. attribute:: Errors

        The dictionary of errors where key is a member (field) name and value is a list of errors. Example if present as JSON:

            .. code-block:: json

                {
                    "": "Validation errors."
                    "Name": ["Field is required"],
                    "Birthday": ["Field is required", "Birthday cannot reach 150 years."],
                    "Password": ["Password should contain at least one digit."]
                }

    .. attribute:: UseMetadataType

        If true there will be additional check for ``MetadataTypeAttribute`` and metadata type registration. False by default. Only available since .NET 4.0 .

    .. function:: void AddError(string member, string error)

        Add error to errors list for specific member.

    .. function:: void AddError(string error)

        Add summary error. It has empty string key.

    .. function:: IDictionary<string, IEnumerable<string>> GetErrorMembersDictionary()

        Returns opposite dictionary where key is error message and value is an enumerable with member names related to the error. Example as JSON:

            .. code-block:: json

                {
                    "Validation errors.": [""],
                    "Field is required": ["FirstName", "LastName"],
                    "Password should contain at least one digit.": ["Password"]
                }

    .. function:: IDictionary<string, string> GetOneErrorDictionary()

        Returns dictionary that contains only one first error message per member name.

    .. function:: void ThrowFromObjectValidation(object obj, IDictionary<object, object> items)

        If object contains validation errors throws instance of ``ValidationException``.

Domain Events
-------------

In application a common requirement is an action should be triggered as a consequence of some other action occurring. This is where you can use Domain Events.

    ::

        raise event ---> dispatch event ---> trigger listeners
