using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace ContactHUB.Areas.Admin.Controllers.Helpers
{
    public static class UsuariosHelper
    {
        public static Usuario GetUsuarioActual(ContactDbContext context, string usuarioNombre)
        {
            return context.Usuarios.Include(u => u.Rol).FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
        }

        public static List<Usuario> GetUsuarios(ContactDbContext context)
        {
            return context.Usuarios.ToList();
        }

        public static Rol GetRol(ContactDbContext context, string rol)
        {
            return context.Roles.FirstOrDefault(r => r.Nombre == rol);
        }
    }
}
