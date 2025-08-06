using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Controllers.Helpers;

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
            var usuario = PerfilHelper.GetUsuario(_context, usuarioNombre);
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
            var usuario = PerfilHelper.GetUsuario(_context, usuarioNombre);
            if (usuario == null)
                return PerfilResponseHelper.Error(this, "Usuario no encontrado.");
            if (string.IsNullOrWhiteSpace(nuevoNombre) || nuevoNombre.Length < 4 || nuevoNombre.Length > 50)
                return PerfilResponseHelper.Error(this, "El nombre debe tener entre 4 y 50 caracteres.");
            if (nuevoNombre == usuario.Nombre)
                return PerfilResponseHelper.Error(this, "El nuevo nombre no puede ser igual al anterior.");
            if (!PerfilHelper.NombreDisponible(_context, nuevoNombre, usuario.IdUsuario))
                return PerfilResponseHelper.Error(this, "Ya existe un usuario con ese nombre.");
            usuario.Nombre = nuevoNombre;
            _context.SaveChanges();
            return PerfilResponseHelper.Success(this, "Nombre actualizado correctamente.");
        }

        [HttpPost]
        public IActionResult CambiarContrasena(string actual, string nueva, string repetir)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = PerfilHelper.GetUsuario(_context, usuarioNombre);
            if (usuario == null)
                return PerfilResponseHelper.Error(this, "Usuario no encontrado.");
            if (!ProfilePasswordHelper.IsValidLength(actual) || !ProfilePasswordHelper.IsValidLength(nueva) || !ProfilePasswordHelper.IsValidLength(repetir))
                return PerfilResponseHelper.Error(this, "Todos los campos son obligatorios y deben tener entre 8 y 30 caracteres.");
            if (nueva != repetir)
                return PerfilResponseHelper.Error(this, "La nueva contraseña y la repetición no coinciden.");
            if (!ProfilePasswordHelper.VerifyCurrent(actual, usuario))
                return PerfilResponseHelper.Error(this, "La contraseña actual es incorrecta.");
            if (ProfilePasswordHelper.IsSameAsOld(nueva, usuario))
                return PerfilResponseHelper.Error(this, "La nueva contraseña no puede ser igual a la anterior.");
            if (ProfilePasswordHelper.IsSameAsUser(nueva, usuario))
                return PerfilResponseHelper.Error(this, "La contraseña no puede ser igual a tu nombre o usuario.");
            if (ProfilePasswordHelper.IsCommonPassword(nueva))
                return PerfilResponseHelper.Error(this, "La contraseña es demasiado común. Elige una más segura.");
            usuario.Clave = ProfilePasswordHelper.Hash(nueva, usuario);
            _context.SaveChanges();
            return PerfilResponseHelper.Success(this, "Contraseña actualizada correctamente. Por seguridad, considera cerrar sesión y volver a ingresar.");
        }
    }
}
