using Microsoft.AspNetCore.Identity;
using ContactHUB.Models;
using System.Linq;

namespace ContactHUB.Controllers.Helpers
{
    public static class ProfilePasswordHelper
    {
        private static readonly string[] comunes = { "12345678", "password", "contraseña", "qwerty", "123456789", "123456", "11111111", "123123123" };

        public static bool IsCommonPassword(string password)
        {
            return comunes.Contains(password);
        }

        public static bool IsValidLength(string password, int min = 8, int max = 30)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= min && password.Length <= max;
        }

        public static bool IsSameAsUser(string password, Usuario usuario)
        {
            return password == usuario.UsuarioNombre || password == usuario.Nombre;
        }

        public static bool IsSameAsOld(string password, Usuario usuario)
        {
            var hasher = new PasswordHasher<Usuario>();
            return hasher.VerifyHashedPassword(usuario, usuario.Clave, password) == PasswordVerificationResult.Success;
        }

        public static bool VerifyCurrent(string current, Usuario usuario)
        {
            var hasher = new PasswordHasher<Usuario>();
            return hasher.VerifyHashedPassword(usuario, usuario.Clave, current) == PasswordVerificationResult.Success;
        }

        public static string Hash(string password, Usuario usuario)
        {
            var hasher = new PasswordHasher<Usuario>();
            return hasher.HashPassword(usuario, password);
        }
    }
}
