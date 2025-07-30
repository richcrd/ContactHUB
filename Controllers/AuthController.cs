using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace ContactHUB.Controllers
{
    public class AuthController : Controller
    {
        private readonly ContactDbContext _context;
        private readonly ILogger<AuthController> _logger;
        public AuthController(ContactDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string clave)
        {
            // Validaciones del lado del servidor
            if (string.IsNullOrWhiteSpace(usuario) || usuario.Length < 4 || usuario.Length > 10 || !System.Text.RegularExpressions.Regex.IsMatch(usuario, "^[a-zA-Z0-9_]+$"))
            {
                ViewBag.Error = "El usuario debe tener entre 4 y 10 caracteres y solo puede contener letras, números y guion bajo.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 30)
            {
                ViewBag.Error = "La clave debe tener entre 8 y 30 caracteres.";
                return View();
            }
            try
            {
                var user = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuario && u.Clave == clave && u.Estado.Nombre == "Activo");
                if (user != null)
                {
                    var claims = new List<System.Security.Claims.Claim>
                    {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.UsuarioNombre),
                        new System.Security.Claims.Claim("Nombre", user.Nombre)
                    };
                    var identity = new System.Security.Claims.ClaimsIdentity(claims, "Cookies");
                    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync("Cookies", principal);
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.Error = "Usuario o clave incorrectos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Login: {Message}", ex.Message);
                ViewBag.Error = "No se pudo conectar con la base de datos. Intente más tarde.";
            }
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
            if (string.IsNullOrWhiteSpace(usuario) || usuario.Length < 4 || usuario.Length > 10 || !System.Text.RegularExpressions.Regex.IsMatch(usuario, "^[a-zA-Z0-9_]+$"))
            {
                ViewBag.Error = "El usuario debe tener entre 4 y 10 caracteres y solo puede contener letras, números y guion bajo.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 4 || nombre.Length > 50 || !System.Text.RegularExpressions.Regex.IsMatch(nombre, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
            {
                ViewBag.Error = "El nombre debe tener entre 4 y 50 caracteres y solo puede contener letras y espacios.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 30)
            {
                ViewBag.Error = "La clave debe tener entre 8 y 30 caracteres.";
                return View();
            }
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Register: {Message}", ex.Message);
                ViewBag.Error = "No se pudo conectar con la base de datos. Intente más tarde.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }
    }
}
