using ContactHUB.Models;
using System;

namespace ContactHUB.Helpers
{
    public static class ContactosValidationHelper
    {
        public static bool ValidarUsuario(string? usuarioNombre, out string error)
        {
            if (string.IsNullOrEmpty(usuarioNombre))
            {
                error = "No autorizado.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public static bool ValidarContacto(Contacto contacto, out string error)
        {
            if (contacto == null)
            {
                error = "Contacto no válido.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(contacto.Telefono) && string.IsNullOrWhiteSpace(contacto.Correo))
            {
                error = "Debe proporcionar teléfono o correo.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        public static bool ValidarEtiquetas(int[] etiquetas, out string error)
        {
            if (etiquetas == null)
            {
                error = "Etiquetas no válidas.";
                return false;
            }
            error = string.Empty;
            return true;
        }
    }
}
