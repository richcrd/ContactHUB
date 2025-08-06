using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactHUB.Services
{
    public class AuthService
    {
        private readonly ContactDbContext _context;
        public AuthService(ContactDbContext context)
        {
            _context = context;
        }

        public bool SuperaIntentosFallidos(string ip, DateTime hoy, out string error)
        {
            error = string.Empty;
            var ultimosFallos = _context.AccionUsuarios
                .Where(a => a.TipoAccion == "login_fail" && a.Fecha >= hoy && (a.IP == ip || a.IdUsuario == null))
                .OrderByDescending(a => a.Fecha)
                .Take(5)
                .ToList();
            if (ultimosFallos.Count == 5)
            {
                var minutosRestantes = 3 - (DateTime.Now - ultimosFallos.First().Fecha).TotalMinutes;
                if (minutosRestantes > 0)
                {
                    error = $"Has superado el límite de intentos fallidos. Espera {Math.Ceiling(minutosRestantes)} minutos para volver a intentar.";
                    return true;
                }
            }
            return false;
        }

        public bool SuperaLimiteRegistrosPorIp(string ip, DateTime hoy, out string error)
        {
            error = string.Empty;
            var registrosHoy = _context.AccionUsuarios.Count(a => a.IP == ip && a.TipoAccion == "registro" && a.Fecha >= hoy);
            if (registrosHoy >= 3)
            {
                error = "Has alcanzado el límite de registros por IP para hoy.";
                return true;
            }
            return false;
        }

        public bool SuperaLimiteUsuarios(out string error)
        {
            error = string.Empty;
            var totalUsuarios = _context.Usuarios.Count();
            if (totalUsuarios >= 10)
            {
                error = "Se ha alcanzado el límite máximo de usuarios registrados.";
                return true;
            }
            return false;
        }

        public Usuario? GetActiveUser(string usuario)
        {
            return _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.UsuarioNombre == usuario && u.Estado.Nombre == "Activo");
        }

        public bool UserExists(string usuario)
        {
            return _context.Usuarios.Any(u => u.UsuarioNombre == usuario);
        }

        public void RegisterUser(Usuario user)
        {
            _context.Usuarios.Add(user);
            _context.SaveChanges();
        }

        public void RegisterAction(AccionUsuario accion)
        {
            _context.AccionUsuarios.Add(accion);
            _context.SaveChanges();
        }

        public int CountUsers() => _context.Usuarios.Count();

        public int CountRegisterActionsByIp(string ip, DateTime hoy) =>
            _context.AccionUsuarios.Count(a => a.IP == ip && a.TipoAccion == "registro" && a.Fecha >= hoy);

        public int CountLoginFails(string ip, DateTime hoy) =>
            _context.AccionUsuarios.Count(a => a.TipoAccion == "login_fail" && a.Fecha >= hoy && (a.IP == ip || a.IdUsuario == null));
    }
}