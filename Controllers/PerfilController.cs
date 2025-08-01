using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;
using System.Threading.Tasks;

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
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(nuevoNombre) || nuevoNombre.Length < 4 || nuevoNombre.Length > 50)
            {
                TempData["Error"] = "El nombre debe tener entre 4 y 50 caracteres.";
                return RedirectToAction("Index");
            }
            if (nuevoNombre == usuario.Nombre)
            {
                TempData["Error"] = "El nuevo nombre no puede ser igual al anterior.";
                return RedirectToAction("Index");
            }
            if (_context.Usuarios.Any(u => u.Nombre == nuevoNombre && u.IdUsuario != usuario.IdUsuario))
            {
                TempData["Error"] = "Ya existe un usuario con ese nombre.";
                return RedirectToAction("Index");
            }
            usuario.Nombre = nuevoNombre;
            _context.SaveChanges();
            TempData["Success"] = "Nombre actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CambiarContrasena(string actual, string nueva, string repetir)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(repetir))
            {
                TempData["Error"] = "Todos los campos son obligatorios.";
                return RedirectToAction("Index");
            }
            if (nueva != repetir)
            {
                TempData["Error"] = "La nueva contraseña y la repetición no coinciden.";
                return RedirectToAction("Index");
            }
            if (nueva.Length < 8 || nueva.Length > 30)
            {
                TempData["Error"] = "La nueva contraseña debe tener entre 8 y 30 caracteres.";
                return RedirectToAction("Index");
            }
            // Validar que la nueva contraseña no sea igual a la anterior
            var hasher = new PasswordHasher<Usuario>();
            var result = hasher.VerifyHashedPassword(usuario, usuario.Clave, actual);
            if (result != PasswordVerificationResult.Success)
            {
                TempData["Error"] = "La contraseña actual es incorrecta.";
                return RedirectToAction("Index");
            }
            if (hasher.VerifyHashedPassword(usuario, usuario.Clave, nueva) == PasswordVerificationResult.Success)
            {
                TempData["Error"] = "La nueva contraseña no puede ser igual a la anterior.";
                return RedirectToAction("Index");
            }
            // Validar que la contraseña no sea igual al nombre de usuario
            if (nueva == usuario.UsuarioNombre || nueva == usuario.Nombre)
            {
                TempData["Error"] = "La contraseña no puede ser igual a tu nombre o usuario.";
                return RedirectToAction("Index");
            }
            // Validar contraseñas comunes (puedes ampliar la lista)
            string[] comunes = { "12345678", "password", "contraseña", "qwerty", "123456789", "123456", "11111111", "123123123" };
            if (comunes.Contains(nueva))
            {
                TempData["Error"] = "La contraseña es demasiado común. Elige una más segura.";
                return RedirectToAction("Index");
            }
            usuario.Clave = hasher.HashPassword(usuario, nueva);
            _context.SaveChanges();
            TempData["Success"] = "Contraseña actualizada correctamente. Por seguridad, considera cerrar sesión y volver a ingresar.";
            return RedirectToAction("Index");
        }
    }
}
