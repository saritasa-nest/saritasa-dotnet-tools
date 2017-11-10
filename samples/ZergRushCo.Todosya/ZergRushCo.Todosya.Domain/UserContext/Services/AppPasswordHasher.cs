using Microsoft.AspNet.Identity;
using Saritasa.Tools.Common.Utils;

namespace ZergRushCo.Todosya.Domain.UserContext.Services
{
    /// <summary>
    /// Custom password hasher.
    /// </summary>
    public class AppPasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Hash password using SHA256 algorithm.
        /// </summary>
        /// <param name="password">Plain user password.</param>
        /// <returns>Hashed user password.</returns>
        public string HashPassword(string password)
        {
            return SecurityUtils.Hash(password, SecurityUtils.HashMethod.Sha256);
        }

        /// <inheritdoc />
        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            // Should not be there.
            return SecurityUtils.CheckHash(providedPassword, hashedPassword) ?
                PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
