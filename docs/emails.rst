Emails
======

Emails send handling. It is built around standard class ``MailMessage``. In simple case only ``IEmailSender`` interface will be used with only one method.

    .. class:: IEmailSender

        Email sender interface.

        .. function:: Task SendAsync(MailMessage message);

            Sends the specified message.

    .. class:: EmailSender

        .. function:: Task SendAsync(MailMessage message);

             Sends the specified message.

Example of registration with Autofac:

    .. code-block:: c#

        // emails
        var emailSender = new Saritasa.Tools.Emails.SmtpClientEmailSender();
        builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();

.. note:: For frameworks that do not support MailMessage API (.NET Standard < 2.0) there is lightweight implementation for ``MailAddress``, ``MailAddressCollection`` and ``MailMessage``.

Email Senders
-------------

Right now only ``SmtpClientEmailSender`` is available that uses ``SmtpClient`` to send emails. In future we will probably add providers for SendGrid and AWS.

Interceptors
------------

You can set additional behavior before and after emails sending by using interceptors. Interceptors are handlers that are executed before or after email sending. They can even cancel it. You can use them with ``EmailSender`` class. Here is a simple example how it can be used to filter users out by email address (what sometimes needed for development environment):

    .. code-block:: c#

        var emailSender = new SmtpClientEmailSender();
        var filterInterceptor = new FilterEmailInterceptor("*@saritasa.com; *@saritasa-hosting.com");
        emailSender.AddInterceptor(filterInterceptor);

Now only emails to saritasa.com and saritasa-hosting.com domains will be accepted. The following interceptors are available:

    .. class:: FilterEmailInterceptor

        Filters users to whom send an email. You can ``*`` and ``?`` special characters to apply mask. The symbols ``,``, ``;`` and space can be used as separator.

    .. class:: SaveToFileEmailInterceptor

        Save email to disk as .eml file before or after sending.

    .. class:: CountEmailsInterceptor

        Contains counters of sending and sent emails. Thread safe.
