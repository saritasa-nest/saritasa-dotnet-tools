// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET452
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Commands.PipelineMiddlewares
{
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
        public virtual void Handle(IMessage message)
        {
            var commandMessage = message as CommandMessage;
            if (commandMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(CommandMessage)));
            }

            var context = new ValidationContext(commandMessage.Content);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(commandMessage.Content, context, results);
            if (!isValid)
            {
                commandMessage.Status = ProcessingStatus.Rejected;
                if (ThrowException)
                {
                    throw new CommandValidationException(results);
                }
            }
        }
    }
}
#endif
