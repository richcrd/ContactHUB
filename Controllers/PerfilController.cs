using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;

namespace ContactHUB.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly ContactDbContext _context;
        public PerfilController(ContactDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Auth");
            }
            return View(usuario);
        }

        [HttpPost]
        public IActionResult CambiarNombre(string nuevoNombre)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
                return Error("Usuario no encontrado.");
            if (string.IsNullOrWhiteSpace(nuevoNombre) || nuevoNombre.Length < 4 || nuevoNombre.Length > 50)
                return Error("El nombre debe tener entre 4 y 50 caracteres.");
            if (nuevoNombre == usuario.Nombre)
                return Error("El nuevo nombre no puede ser igual al anterior.");
            if (_context.Usuarios.Any(u => u.Nombre == nuevoNombre && u.IdUsuario != usuario.IdUsuario))
                return Error("Ya existe un usuario con ese nombre.");
            usuario.Nombre = nuevoNombre;
            _context.SaveChanges();
            return Success("Nombre actualizado correctamente.");
        }

        [HttpPost]
        public IActionResult CambiarContrasena(string actual, string nueva, string repetir)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
                return Error("Usuario no encontrado.");
            if (!Helpers.ProfilePasswordHelper.IsValidLength(actual) || !Helpers.ProfilePasswordHelper.IsValidLength(nueva) || !Helpers.ProfilePasswordHelper.IsValidLength(repetir))
                return Error("Todos los campos son obligatorios y deben tener entre 8 y 30 caracteres.");
            if (nueva != repetir)
                return Error("La nueva contraseña y la repetición no coinciden.");
            if (!Helpers.ProfilePasswordHelper.VerifyCurrent(actual, usuario))
                return Error("La contraseña actual es incorrecta.");
            if (Helpers.ProfilePasswordHelper.IsSameAsOld(nueva, usuario))
                return Error("La nueva contraseña no puede ser igual a la anterior.");
            if (Helpers.ProfilePasswordHelper.IsSameAsUser(nueva, usuario))
                return Error("La contraseña no puede ser igual a tu nombre o usuario.");
            if (Helpers.ProfilePasswordHelper.IsCommonPassword(nueva))
                return Error("La contraseña es demasiado común. Elige una más segura.");
            usuario.Clave = Helpers.ProfilePasswordHelper.Hash(nueva, usuario);
            _context.SaveChanges();
            return Success("Contraseña actualizada correctamente. Por seguridad, considera cerrar sesión y volver a ingresar.");
        }
        // Métodos auxiliares para simplificar respuestas
        private IActionResult Error(string mensaje)
        {
            TempData["Error"] = mensaje;
            return RedirectToAction("Index");
        }

        private IActionResult Success(string mensaje)
        {
            TempData["Success"] = mensaje;
            return RedirectToAction("Index");
        }
    }
}
