using Microsoft.AspNet.Identity;

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
            return Candy.SecurityUtils.Hash(password, Candy.SecurityUtils.HashMethods.Sha256);
        }

        /// <inheritdoc />
        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            // should not be there
            return Candy.SecurityUtils.CheckHash(providedPassword, hashedPassword) ?
                PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
