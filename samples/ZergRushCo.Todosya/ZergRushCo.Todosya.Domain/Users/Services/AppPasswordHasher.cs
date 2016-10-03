using Microsoft.AspNet.Identity;

namespace ZergRushCo.Todosya.Domain.Users.Services
{
    /// <summary>
    /// Custom password hasher.
    /// </summary>
    public class AppPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return Candy.SecurityUtils.Hash(password, Candy.SecurityUtils.HashMethods.Sha256);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            // should not be there
            return Candy.SecurityUtils.CheckHash(providedPassword, hashedPassword) ?
                PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
