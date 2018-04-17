// Copyright (c) 2015-2018, Saritasa. All rights reserved.
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

        private class Product
        {
            [Required]
            [MinLength(2)]
            public string Name { get; set; }
        }

        [Fact]
        public void Domain_validation_exception_should_take_into_account_IValidatableObject()
        {
            // Arrange
            var obj = new User();

            // Act
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
            Assert.Single(validationException.SummaryErrors);
            Assert.Equal(2, validationException.Errors.Count);
        }

        [Fact]
        public void Domain_validation_exception_should_not_throw_on_valid_object()
        {
            // Arrange
            var obj = new Product();
            obj.Name = "Coffee";

            // Act & Assert
            Saritasa.Tools.Domain.Exceptions.ValidationException.ThrowFromObjectValidation(obj);
        }

        [Fact]
        public void Domain_validation_exception_should_throw_on_not_valid_object()
        {
            // Arrange
            var obj = new Product();

            // Act
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
            Assert.Empty(validationException.SummaryErrors);
            Assert.Single(validationException.Errors);
        }

#if NET452

        [MetadataType(typeof(JobMetadata))]
        private class Job
        {
            public string Text { get; set; }

            public int DurationMins { get; set; }
        }

        private class JobMetadata
        {
            [Required]
            [MaxLength(100)]
            public string Text { get; set; }

            [Range(1, 1500)]
            public int DurationMins { get; set; }
        }

        [Fact]
        public void Domain_validation_should_work_on_MetadataType_attribute()
        {
            // Arrange
            var obj = new Job();

            // Act
            Exceptions.ValidationException.UseMetadataType = true;
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
            Assert.Empty(validationException.SummaryErrors);
            Assert.Equal(2, validationException.Errors.Count);
        }

#endif

        [Fact]
        public void Domain_validation_should_keep_message_from_ctor()
        {
            // Arrange & act.
            var ex = new Exceptions.ValidationException("The custom message");

            // Assert.
            Assert.Equal("The custom message", ex.Message);
            Assert.Empty(ex.Errors);
        }
    }
}
