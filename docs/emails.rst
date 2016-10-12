######
Emails
######

Emails send handling.

.. class:: IEmailSender

    Email sender interface.

    .. function:: Task SendAsync(TMessage message);

    Sends the specified message.

.. class:: EmailSender

    .. function:: Task SendAsync(TMessage message);

Example of registration with Autofac:

    .. code-block:: c#

        // emails
        var emailSender = new Saritasa.Tools.Emails.SystemMail.SmtpClientEmailSender();
        builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();
