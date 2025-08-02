using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContactHUB.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            var usuario = User.Identity?.Name != null ? _context.Usuarios.Include(u => u.Rol).FirstOrDefault(u => u.UsuarioNombre == User.Identity.Name) : null;
            if (usuario == null || usuario.Rol?.Nombre != "Admin")
            {
                return Forbid();
            }
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        [HttpPost]
        public IActionResult CambiarRol(int id, string rol)
        {
            var usuarioActual = User.Identity?.Name != null ? _context.Usuarios.Include(u => u.Rol).FirstOrDefault(u => u.UsuarioNombre == User.Identity.Name) : null;
            if (usuarioActual == null || usuarioActual.Rol?.Nombre != "Admin")
            {
                return Forbid();
            }
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }
            var rolObj = _context.Roles.FirstOrDefault(r => r.Nombre == rol);
            if (rolObj == null)
            {
                TempData["Error"] = "Rol no válido.";
                return RedirectToAction("Index");
            }
            usuario.IdRol = rolObj.IdRol;
            _context.SaveChanges();
            TempData["Success"] = $"Rol actualizado para {usuario.UsuarioNombre}.";
            return RedirectToAction("Index");
        }
    }
}
