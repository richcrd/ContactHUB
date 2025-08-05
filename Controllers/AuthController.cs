using Microsoft.AspNetCore.Mvc;
using ContactHUB.Models;
using Microsoft.AspNetCore.Authentication;
using ContactHUB.Services;
using ContactHUB.ViewModels;
using ContactHUB.Helpers;

namespace ContactHUB.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!UserValidationService.ValidarUsuarioLogin(model.Usuario, out string errorUsuario))
            {
                ModelState.AddModelError("Usuario", errorUsuario);
                return View(model);
            }
            if (!UserValidationService.ValidarClave(model.Clave, out string errorClave))
            {
                ModelState.AddModelError("Clave", errorClave);
                return View(model);
            }
            
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var hoy = DateTime.Today;
            if (_authService.SuperaIntentosFallidos(ip, hoy, out string errorIntentos))
            {
                TempData["Error"] = errorIntentos;
                return RedirectToAction("Login");
            }
            try
            {
                var user = _authService.GetActiveUser(model.Usuario);
                if (user != null && Helpers.PasswordHelper.VerifyPassword(user, user.Clave, model.Clave))
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
                    TempData["Success"] = $"¡Bienvenido, {user.Nombre}! Has iniciado sesión correctamente.";
                    return RedirectToAction("Index", "Home");
                }

                _authService.RegisterAction(new AccionUsuario
                {
                    IdUsuario = user?.IdUsuario,
                    IP = ip,
                    TipoAccion = "login_fail",
                    Fecha = DateTime.Now
                });
                TempData["Error"] = "Usuario o clave incorrectos";
                return View(model);
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
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var hoy = DateTime.Today;
            if (_authService.SuperaLimiteRegistrosPorIp(ip, hoy, out string errorIp))
            {
                ModelState.AddModelError(string.Empty, errorIp);
                return View(model);
            }
            if (_authService.SuperaLimiteUsuarios(out string errorUsuarios))
            {
                ModelState.AddModelError(string.Empty, errorUsuarios);
                return View(model);
            }
            if (!UserValidationService.ValidarUsuarioLogin(model.Usuario, out string errorUsuario))
            {
                ModelState.AddModelError("Usuario", errorUsuario);
                return View(model);
            }
            if (!UserValidationService.ValidarNombre(model.Nombre, out string errorNombre))
            {
                ModelState.AddModelError("Nombre", errorNombre);
                return View(model);
            }
            if (!UserValidationService.ValidarClave(model.Clave, out string errorClave))
            {
                ModelState.AddModelError("Clave", errorClave);
                return View(model);
            }
            try
            {
                if (_authService.UserExists(model.Usuario))
                {
                    TempData["Error"] = "El usuario ya existe.";
                    return RedirectToAction("Register");
                }
                var user = new Usuario
                {
                    UsuarioNombre = model.Usuario,
                    Nombre = model.Nombre,
                    Id_Estado = 1,
                    IdRol = 2
                };
                user.Clave = PasswordHelper.HashPassword(user, model.Clave);
                _authService.RegisterUser(user);
                _authService.RegisterAction(new AccionUsuario
                {
                    IP = ip,
                    TipoAccion = "registro",
                    Fecha = DateTime.Now
                });

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
