Guard
=====

Provides common patterns to validate values. Contains set of methods to write less code to defined method pre-conditions. Example:

    .. code-block:: c#

        void CreateUser(User user, int score)
        {
            Guard.IsNotNull(user, nameof(user));
            Guard.IsNotNegative(score, nameof(score));
        }

There are methods implemented: ``IsNotEmpty``, ``IsNotOutOfLength``, ``IsNotNull``, ``IsNotNegative``, ``IsNotNegativeOrZero``, ``IsNotInPast``, ``IsNotInFuture``, ``IsNotInvalidEmail``.

.. class:: Guard

    Contains common validation regexp patterns.

    .. attribute:: EmailExpression (regexp)

        Email regular expression.

    .. attribute:: WebUrlExpression (regexp)

        Web url regular expression. Support http and https protocols.

    .. attribute:: StripHTMLExpression (regexp)

        Regular expression to remove html tags from string.
