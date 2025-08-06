using System.Text.RegularExpressions;

namespace ContactHUB.Services
{
    public static class UserValidationService
    {
        public static bool ValidarUsuarioLogin(string usuario, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(usuario) || usuario.Length < 4 || usuario.Length > 10 || !Regex.IsMatch(usuario, "^[a-zA-Z0-9_]+$"))
            {
                error = "El usuario debe tener entre 4 y 10 caracteres y solo puede contener letras, números y guion bajo.";
                return false;
            }
            return true;
        }

        public static bool ValidarNombre(string nombre, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 4 || nombre.Length > 50 || !Regex.IsMatch(nombre, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
            {
                error = "El nombre debe tener entre 4 y 50 caracteres y solo puede contener letras y espacios.";
                return false;
            }
            return true;
        }

        public static bool ValidarClave(string clave, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 30)
            {
                error = "La clave debe tener entre 8 y 30 caracteres.";
                return false;
            }
            return true;
        }
    }
}
