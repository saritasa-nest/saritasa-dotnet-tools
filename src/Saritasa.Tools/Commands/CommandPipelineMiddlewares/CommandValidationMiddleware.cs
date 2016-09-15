// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Commands.CommandPipelineMiddlewares
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    using Messages;

    /// <summary>
    /// Validates command. It uses data annotation attributes.
    /// </summary>
    public class CommandValidationMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "CommandValidation";

        /// <summary>
        /// Throw exception if object is not valid.
        /// </summary>
        public bool ThrowException { get; set; } = true;

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException("Message should be CommandMessage type");
            }

            var context = new ValidationContext(commandMessage.Content);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(commandMessage.Content, context, results);
            if (!isValid)
            {
                commandMessage.Status = Message.ProcessingStatus.Rejected;
                if (ThrowException)
                {
                    throw new CommandValidationException(results);
                }
            }
        }
    }
}
#endif
