// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
#if NET452
using System.Runtime.Serialization.Formatters.Binary;
#endif
using Xunit;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.Domain.Tests
{
    /// <summary>
    /// Domain tests.
    /// </summary>
    public class DomainTests
    {
#if NET452
        [Fact]
        public void Domain_exception_should_serialize_deserialize_correctly()
        {
            // Arrange
            var domainException = new DomainException("Test");
            var formatter = new BinaryFormatter();
            DomainException deserializedDomainException = null;

            // Act
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, domainException);
                memoryStream.Seek(0, SeekOrigin.Begin);
                deserializedDomainException = (DomainException)formatter.Deserialize(memoryStream);
            }

            // Assert
            Assert.Equal(domainException.Message, deserializedDomainException.Message);
        }
#endif

        private class User : IValidatableObject
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrEmpty(FirstName))
                {
                    yield return new ValidationResult("Error message with no member");
                    yield return new ValidationResult("Error message with nmember", new[] { nameof(FirstName) });
                }
            }
        }

        [Fact]
        public void Domain_validation_exception_should_take_into_account_IValidatableObject()
        {
            // Arrange
            var obj = new User();

            // Act & Assert
            Exceptions.ValidationException validationException = null;
            try
            {
                Saritasa.Tools.Domain.Exceptions.ValidationException.ThrowFromObjectValidation(obj);
            }
            catch (Exceptions.ValidationException ex)
            {
                validationException = ex;
            }

            // Assert
            Assert.NotNull(validationException);
            Assert.Equal(1, validationException.SummaryErrors.Count());
            Assert.Equal(2, validationException.Errors.Count);
        }
    }
}
