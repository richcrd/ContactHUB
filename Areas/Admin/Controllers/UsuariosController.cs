using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContactHUB.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ContactHUB.Areas.Admin.Controllers.Helpers;

namespace ContactHUB.Controllers.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly ContactDbContext _context;
        private readonly ILogger<UsuariosController> _logger;
        public UsuariosController(ContactDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var usuarioActual = UsuariosHelper.GetUsuarioActual(_context, User.Identity?.Name);
            if (usuarioActual == null || usuarioActual.Rol?.Nombre != "Admin")
                return Forbid();
            var usuarios = UsuariosHelper.GetUsuarios(_context);
            return View(usuarios);
        }

        [HttpPost]
        public IActionResult CambiarRol(int id, string rol)
        {
            var usuarioActual = UsuariosHelper.GetUsuarioActual(_context, User.Identity?.Name);
            if (usuarioActual == null || usuarioActual.Rol?.Nombre != "Admin")
                return Forbid();
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == id);
            if (usuario == null)
                return UsuariosResponseHelper.Error(this, "Usuario no encontrado.");
            var rolObj = UsuariosHelper.GetRol(_context, rol);
            if (rolObj == null)
                return UsuariosResponseHelper.Error(this, "Rol no válido.");
            usuario.IdRol = rolObj.IdRol;
            _context.SaveChanges();
            return UsuariosResponseHelper.Success(this, $"Rol actualizado para {usuario.UsuarioNombre}.");
        }
    }
}
