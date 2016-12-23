using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// Abstract message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Unique message id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Message type.
        /// </summary>
        byte Type { get; set; }

        /// <summary>
        /// Message name.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Message content. May be command object, or event object.
        /// </summary>
        object Content { get; set; }

        /// <summary>
        /// Custom data.
        /// </summary>
        IDictionary<string, string> Data { get; set; }

        /// <summary>
        /// Contains exception if any error occured during message processing.
        /// </summary>
        Exception Error { get; set; }

        /// <summary>
        /// Error text message.
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Error type.
        /// </summary>
        string ErrorType { get; set; }

        /// <summary>
        /// When message has been created.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Message execution duration, in ms.
        /// </summary>
        int ExecutionDuration { get; set; }

        /// <summary>
        /// Processing status.
        /// </summary>
        ProcessingStatus Status { get; set; }
    }
}
