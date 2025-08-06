using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;
using System.Collections.Generic;

namespace ContactHUB.Areas.Admin.Controllers.Helpers
{
    public static class RecuperacionAuditoriaHelper
    {
        public static bool IsAdmin(ContactDbContext context, string usuarioNombre)
        {
            var usuario = context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            return usuario != null && usuario.IdRol == 1;
        }

        public static List<RecuperacionIntento> GetIntentos(ContactDbContext context, int max = 200)
        {
            return context.RecuperacionIntentos.OrderByDescending(i => i.Fecha).Take(max).ToList();
        }
    }
}
