// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Commands.PipelineMiddlewares
{
    /// <summary>
    /// Validates command. It uses data annotation attributes.
    /// </summary>
    public class CommandValidationMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Throw exception if object is not valid.
        /// </summary>
        public bool ThrowException { get; set; } = true;

        /// <summary>
        /// .ctor
        /// </summary>
        public CommandValidationMiddleware()
        {
            Id = this.GetType().Name;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            try
            {
                Domain.Exceptions.ValidationException.ThrowFromObjectValidation(messageContext.Content);
            }
            catch (Exception)
            {
                messageContext.Status = ProcessingStatus.Rejected;
                if (ThrowException)
                {
                    throw;
                }
            }
        }
    }
}
