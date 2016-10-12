######
Events
######

********
Overview
********

Events are actions to something that happens to system. They are executed separately independ of main action. For example after user creation welcome email should be sent to him. User creation is a command and email sending is an event.

    ::

        User Create ---> (events pipeline) ---> Email Send

***********
Middlewares
***********

    .. class:: DomainEventLocatorMiddleware

    .. class:: EventExecutorMiddleware

    .. class:: EventHandlerLocatorMiddleware
