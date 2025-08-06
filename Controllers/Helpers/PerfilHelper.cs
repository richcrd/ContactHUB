using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;

namespace ContactHUB.Controllers.Helpers
{
    public static class PerfilHelper
    {
        public static Usuario? GetUsuario(ContactDbContext context, string usuarioNombre)
        {
            return context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
        }

        public static bool NombreDisponible(ContactDbContext context, string nuevoNombre, int idUsuario)
        {
            return !context.Usuarios.Any(u => u.Nombre == nuevoNombre && u.IdUsuario != idUsuario);
        }
    }
}
