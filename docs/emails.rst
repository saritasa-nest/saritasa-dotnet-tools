Emails
======

Emails send handling.

.. class:: IEmailSender

	Email sender interface.

	.. function:: Task SendAsync(TMessage message);

	Sends the specified message.

.. class:: EmailSender

	.. function:: Task SendAsync(TMessage message);
