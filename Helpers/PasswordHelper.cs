using Microsoft.AspNetCore.Identity;
using ContactHUB.Models;

namespace ContactHUB.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(Usuario user, string password)
        {
            var hasher = new PasswordHasher<Usuario>();
            return hasher.HashPassword(user, password);
        }

        public static bool VerifyPassword(Usuario user, string hashedPassword, string providedPassword)
        {
            var hasher = new PasswordHasher<Usuario>();
            return hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
        }
    }
}
