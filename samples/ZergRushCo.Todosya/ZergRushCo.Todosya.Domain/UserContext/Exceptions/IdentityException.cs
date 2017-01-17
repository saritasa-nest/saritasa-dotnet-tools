using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Domain.Exceptions;

namespace ZergRushCo.Todosya.Domain.UserContext.Exceptions
{
    /// <summary>
    /// Wraps MS Identity result and throws it as exception if
    /// something went wrong.
    /// </summary>
    public class IdentityException : DomainException
    {
        /// <summary>
        /// Original identity result.
        /// </summary>
        public IdentityResult Result { get; }

        /// <summary>
        /// The list of all error messages separated by new line.
        /// </summary>
        public override string Message => Result.Errors.First();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="result">Identity result.</param>
        public IdentityException(IdentityResult result)
        {
            this.Result = result;
        }
    }
}
