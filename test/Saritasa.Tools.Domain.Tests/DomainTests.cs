// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.Domain.Tests
{
    /// <summary>
    /// Domain tests.
    /// </summary>
    public class DomainTests
    {
        [Fact]
        public void BinaryFormatterSerialize_DomainExceptionWithMessageAndCode_PersistAfterDeserialize()
        {
            // Arrange
            var domainException = new DomainException("Test", 10);
            var formatter = new BinaryFormatter();
            DomainException deserializedDomainException = null;

            // Act
            using (var memoryStream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                formatter.Serialize(memoryStream, domainException);
                memoryStream.Seek(0, SeekOrigin.Begin);
                deserializedDomainException = (DomainException)formatter.Deserialize(memoryStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }

            // Assert
            Assert.Equal(domainException.Message, deserializedDomainException.Message);
            Assert.Equal(domainException.Code, deserializedDomainException.Code);
        }

        [Fact]
        public void BinaryFormatterSerialize_ValidationExceptionWithErrors_PersistAfterDeserialize()
        {
            // Arrange
            var validationException = new Saritasa.Tools.Domain.Exceptions.ValidationException(new Dictionary<string, ICollection<string>>()
            {
                ["Name"] = new List<string> { "Required." },
                ["Dob"] = new List<string> { "Out of range." },
                ["SSN"] = new List<string> { "Incorrect length.", "Should not contain characters." }
            });
            var formatter = new BinaryFormatter();
            Saritasa.Tools.Domain.Exceptions.ValidationException deserializedValidationException = null;

            // Act
            using (var memoryStream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                formatter.Serialize(memoryStream, validationException);
                memoryStream.Seek(0, SeekOrigin.Begin);
                deserializedValidationException = (Saritasa.Tools.Domain.Exceptions.ValidationException)formatter.Deserialize(memoryStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }

            // Assert
            Assert.Equal(validationException.Errors.Count, deserializedValidationException.Errors.Count);
            Assert.Equal(validationException.Errors["Dob"].ElementAt(0), deserializedValidationException.Errors["Dob"].ElementAt(0));
            Assert.Equal(validationException.Errors["SSN"].Count(), deserializedValidationException.Errors["SSN"].Count());
        }

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
        public void ThrowFromObjectValidation_ValidationExceptionThrow_CorrectValidationException()
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
            Assert.Single(validationException.Errors.SummaryErrors);
            Assert.Equal(2, validationException.Errors.Count);
        }

        [Fact]
        public void ThrowFromObjectValidation_ValidProduct_NoException()
        {
            // Arrange
            var obj = new Product();
            obj.Name = "Coffee";

            // Act & Assert
            Saritasa.Tools.Domain.Exceptions.ValidationException.ThrowFromObjectValidation(obj);
        }

        [Fact]
        public void ThrowFromObjectValidation_InvalidProduct_ThrowException()
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
            Assert.Empty(validationException.Errors.SummaryErrors);
            Assert.Single(validationException.Errors);
        }

#if NET48
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
        public void ThrowFromObjectValidation_JobAndUseMetadataType_MetadataTypeUsedInValidation()
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
        public void ValidationException_WithMessage_MessageRemains()
        {
            // Arrange & act
            var ex = new Exceptions.ValidationException("The custom message");

            // Assert
            Assert.Equal("The custom message", ex.Message);
            Assert.Empty(ex.Errors);
        }
    }
}
