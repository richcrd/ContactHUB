using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;

namespace ContactHUB.Controllers
{
    public class AuthController : Controller
    {
        private readonly ContactDbContext _context;
        public AuthController(ContactDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string clave)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuario && u.Clave == clave && u.Estado.Nombre == "Activo");
            if (user != null)
            {
                // Aquí puedes agregar lógica de autenticación real (cookies, claims, etc.)
                ViewBag.Message = "Login exitoso";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Usuario o clave incorrectos, o usuario inactivo.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string usuario, string nombre, string clave)
        {
            // Estado activo por defecto (IdEstado = 1)
            var existe = _context.Usuarios.Any(u => u.UsuarioNombre == usuario);
            if (existe)
            {
                ViewBag.Error = "El usuario ya existe.";
                return View();
            }
            var user = new Usuario
            {
                UsuarioNombre = usuario,
                Nombre = nombre,
                Clave = clave,
                Id_Estado = 1 // Activo
            };
            _context.Usuarios.Add(user);
            _context.SaveChanges();
            ViewBag.Message = "Usuario registrado exitosamente.";
            return RedirectToAction("Login");
        }
    }
}
