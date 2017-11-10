using System.Net.Mail;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Events;
using ZergRushCo.Todosya.Domain.UserContext.Events;

namespace ZergRushCo.Todosya.Domain.UserContext.Handlers
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
        public async Task HandleSendEmailOnUserCreate(
            UserCreatedEvent userCreatedEvent,
            Saritasa.Tools.Emails.IEmailSender emailSender)
        {
            var message = new MailMessage
            {
                To =
                {
                    new MailAddress(userCreatedEvent.User.Email)
                },
                Body = "Thanks for registering to our site!",
            };
            await emailSender.SendAsync(message);
        }
    }
}
