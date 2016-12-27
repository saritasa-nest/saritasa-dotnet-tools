using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZergRushCo.Todosya.Domain.TaskContext
{
    /// <summary>
    /// Validate color. Expected values are #ffffff, #FFF123, #eee, etc.
    /// </summary>
    public class ColorValidationAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return Regex.IsMatch(value.ToString(), @"^#(?:[0-9a-fA-F]{3}){1,2}$") ?
                null : new ValidationResult("Invalid color");
        }
    }
}
