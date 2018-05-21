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

    .. class:: SmtpClientEmailSender

        Utilizes``SmtpClient`` to send emails. Unlike standard ``SmtpClient`` you can call send message method without any synchronization. It puts them to queue before sending. Thread safe.

            .. attribute:: MaxQueueSize

                Max queue size. If queue size for some reason is exceeded the ``EmailQueueExceededException`` exception will be thrown. Default value is 10240.

    .. class:: MultiSmtpClientEmailSender

        Handles several smtp clients to send emails using round-robin method. The main parameter is ``smtpClientInstancesCount`` that specifies how many smtp clients to use (default is 2) to send emails simultaneously.

Interceptors
------------

You can set additional behavior before and after emails sending by using interceptors. Interceptors are handlers that are executed before or after email sending. They can even cancel it. You can use them with ``EmailSender`` class. Here is a simple example how it can be used to filter users out by email address (what sometimes needed for development environment):

    .. code-block:: c#

        var emailSender = new SmtpClientEmailSender();
        var filterInterceptor = new FilterEmailInterceptor("*@saritasa.com; *@saritasa-hosting.com");
        emailSender.AddInterceptor(filterInterceptor);

Now only emails to saritasa.com and saritasa-hosting.com domains will be accepted. The following interceptors are available:

    .. class:: FilterEmailInterceptor

        Filters users to whom send an email. You can ``*`` and ``?`` special characters to apply mask. The symbols ``,``, ``;`` and space can be used as separator. For example: ``*@yandex.ru;*+test@saritasa.com``.

    .. class:: SaveToFileEmailInterceptor

        Save email to disk as .eml file before or after sending.

    .. class:: CountEmailsInterceptor

        Contains counters of sending and sent emails. Thread safe.

Using with ASP.NET
------------------

ThreadPool is not a safe place to keep emails processing. See the article `QueueBackgroundWorkItem to reliably schedule and run background processes in ASP.NET <https://blogs.msdn.microsoft.com/webdev/2014/06/04/queuebackgroundworkitem-to-reliably-schedule-and-run-background-processes-in-asp-net/>`_. You should implement you own decarator class to do that:

    .. code-block:: c#

        /// <summary>
        /// The email sender decorates <see cref="IEmailSender" /> class. It adds messages to queue
        /// using <see cref="HostingEnvironment.QueueBackgroundWorkItem(Action{System.Threading.CancellationToken})" />
        /// method.
        /// </summary>
        public class AspNetSmtpEmailSender : IEmailSender
        {
            private readonly IEmailSender actualSender;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="actualSender">Actual sender need to decorate.</param>
            public AspNetSmtpEmailSender(IEmailSender actualSender)
            {
                Guard.IsNotNull(actualSender, nameof(actualSender));
                this.actualSender = actualSender;
            }

            /// <inheritdoc />
            public Task SendAsync(MailMessage message)
            {
                HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                {
                    await actualSender.SendAsync(message).ConfigureAwait(false);
                });
                return Task.FromResult(true);
            }
        }
