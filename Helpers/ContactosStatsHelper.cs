using ContactHUB.Data;
using System;

namespace ContactHUB.Helpers
{
    public class ContactosStatsHelper
    {
        private readonly ContactDbContext _context;
        public ContactosStatsHelper(ContactDbContext context)
        {
            _context = context;
        }

        public int GetTotalContactos(int usuarioId)
            => _context.Contactos.Count(c => c.Id_Usuario == usuarioId);

        public int GetTotalActivos(int usuarioId)
            => _context.Contactos.Count(c => c.Id_Usuario == usuarioId && c.Id_Estado == 1);

        public int GetAgregadosHoy(int usuarioId)
        {
            var hoy = DateTime.Today;
            return _context.AccionUsuarios.Count(a => a.IdUsuario == usuarioId && a.TipoAccion == "agregar_contacto" && a.Fecha >= hoy);
        }

        public int GetEliminadosHoy(int usuarioId)
        {
            var hoy = DateTime.Today;
            return _context.AccionUsuarios.Count(a => a.IdUsuario == usuarioId && a.TipoAccion == "eliminar_contacto" && a.Fecha >= hoy);
        }
    }
}
