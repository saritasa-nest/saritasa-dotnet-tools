using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Events;
using ZergRushCo.Todosya.Domain.Users.Events;

namespace ZergRushCo.Todosya.Domain.Users.Handlers
{
    /// <summary>
    /// User events handlers.
    /// </summary>
    [EventHandlers]
    public class UserEventsHandlers
    {
        /// <summary>
        /// Handle user create event.
        /// </summary>
        /// <param name="userCreatedEvent">Event instance.</param>
        /// <param name="emailSender">Email sender.</param>
        public void HandleSendEmailOnUserCreate(UserCreatedEvent userCreatedEvent,
            Saritasa.Tools.Emails.IEmailSender emailSender)
        {
            var message = new MailMessage()
            {
                To = { new MailAddress(userCreatedEvent.User.Email) },
                Body = $"Thanks for registering to our site!",
            };
            emailSender.SendAsync(message);
        }
    }
}
