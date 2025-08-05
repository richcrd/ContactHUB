using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                // Validaciones del lado del servidor
                if (string.IsNullOrWhiteSpace(usuario) || usuario.Length < 4 || usuario.Length > 10 || !System.Text.RegularExpressions.Regex.IsMatch(usuario, "^[a-zA-Z0-9_]+$"))
                {
                    TempData["Error"] = "El usuario debe tener entre 4 y 10 caracteres y solo puede contener letras, números y guion bajo.";
                    return RedirectToAction("Login");
                }
                if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 30)
                {
                    TempData["Error"] = "La clave debe tener entre 8 y 30 caracteres.";
                    return RedirectToAction("Login");
                }

                // Limite de intentos fallidos de login por usuario/IP
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                var hoy = DateTime.Today;
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
                        TempData["Error"] = $"Has superado el límite de intentos fallidos. Espera {Math.Ceiling(minutosRestantes)} minutos para volver a intentar.";
                        return RedirectToAction("Login");
                    }
                }

                var user = _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefault(u => u.UsuarioNombre == usuario && u.Estado.Nombre == "Activo");
                if (user != null)
                {
                    var hasher = new PasswordHasher<Usuario>();
                    var result = hasher.VerifyHashedPassword(user, user.Clave, clave);
                    if (result == PasswordVerificationResult.Success)
                    {
                        var claims = new List<System.Security.Claims.Claim>
                        {
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.UsuarioNombre),
                            new System.Security.Claims.Claim("Nombre", user.Nombre),
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Rol.Nombre)
                        };
                        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Cookies");
                        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync("Cookies", principal);
                        TempData["Success"] = "¡Bienvenido, " + user.Nombre + "! Has iniciado sesión correctamente.";
                        return RedirectToAction("Index", "Home");
                    }
                }
                // Registrar intento fallido
                _context.AccionUsuarios.Add(new AccionUsuario
                {
                    IdUsuario = user?.IdUsuario,
                    IP = ip,
                    TipoAccion = "login_fail",
                    Fecha = DateTime.Now
                });
                _context.SaveChanges();
                TempData["Error"] = "Usuario o clave incorrectos";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Login: {Message}", ex.Message);
                TempData["Error"] = "No se pudo conectar con la base de datos. Intente más tarde.";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string usuario, string nombre, string clave)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var hoy = DateTime.Today;
            // Limite de registros por IP por día
            var registrosHoy = _context.AccionUsuarios.Count(a => a.IP == ip && a.TipoAccion == "registro" && a.Fecha >= hoy);
            if (registrosHoy >= 3)
            {
                TempData["Error"] = "Has alcanzado el límite de registros por IP para hoy.";
                return RedirectToAction("Register");
            }
            // Limite total de usuarios en el sistema
            var totalUsuarios = _context.Usuarios.Count();
            if (totalUsuarios >= 10)
            {
                TempData["Error"] = "Se ha alcanzado el límite máximo de usuarios registrados.";
                return RedirectToAction("Register");
            }

            if (string.IsNullOrWhiteSpace(usuario) || usuario.Length < 4 || usuario.Length > 10 || !System.Text.RegularExpressions.Regex.IsMatch(usuario, "^[a-zA-Z0-9_]+$"))
            {
                TempData["Error"] = "El usuario debe tener entre 4 y 10 caracteres y solo puede contener letras, números y guion bajo.";
                return RedirectToAction("Register");
            }
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 4 || nombre.Length > 50 || !System.Text.RegularExpressions.Regex.IsMatch(nombre, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
            {
                TempData["Error"] = "El nombre debe tener entre 4 y 50 caracteres y solo puede contener letras y espacios.";
                return RedirectToAction("Register");
            }
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 30)
            {
                TempData["Error"] = "La clave debe tener entre 8 y 30 caracteres.";
                return RedirectToAction("Register");
            }
            try
            {
                // Estado activo por defecto (IdEstado = 1)
                var existe = _context.Usuarios.Any(u => u.UsuarioNombre == usuario);
                if (existe)
                {
                TempData["Error"] = "El usuario ya existe.";
                return RedirectToAction("Register");
                }
                var hasher = new PasswordHasher<Usuario>();
                var user = new Usuario
                {
                    UsuarioNombre = usuario,
                    Nombre = nombre,
                    Id_Estado = 1, // Activo
                    IdRol = 2 // Usuario normal por defecto
                };
                user.Clave = hasher.HashPassword(user, clave);
                _context.Usuarios.Add(user);
                _context.SaveChanges();

                // Registrar acción en AccionUsuario
                _context.AccionUsuarios.Add(new AccionUsuario
                {
                    IP = ip,
                    TipoAccion = "registro",
                    Fecha = DateTime.Now
                });
                _context.SaveChanges();

                TempData["Success"] = "Usuario registrado exitosamente.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Register: {Message}", ex.Message);
                TempData["Error"] = "No se pudo conectar con la base de datos. Intente más tarde.";
                return RedirectToAction("Register");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            TempData["Success"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Login");
        }
    }
}
