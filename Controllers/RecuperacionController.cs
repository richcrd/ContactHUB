using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ContactHUB.Controllers
{
    public class RecuperacionController : Controller
    {
        private readonly ContactDbContext _context;
        private readonly IConfiguration _config;
        public RecuperacionController(ContactDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult Solicitar() => View();

        [HttpPost]
        public IActionResult Solicitar(string usuarioOEmail)
        {
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconocida";
            if (string.IsNullOrWhiteSpace(usuarioOEmail))
            {
                _context.RecuperacionIntentos.Add(new Models.RecuperacionIntento {
                    UsuarioNombre = usuarioOEmail ?? "",
                    Fecha = DateTime.UtcNow,
                    Ip = ip,
                    Exitoso = false,
                    Motivo = "Campo vacío"
                });
                _context.SaveChanges();
                TempData["Error"] = "Debes ingresar tu usuario o email.";
                return View();
            }
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioOEmail || u.Nombre == usuarioOEmail);
            if (usuario == null)
            {
                _context.RecuperacionIntentos.Add(new Models.RecuperacionIntento {
                    UsuarioNombre = usuarioOEmail,
                    Fecha = DateTime.UtcNow,
                    Ip = ip,
                    Exitoso = false,
                    Motivo = "Usuario no encontrado"
                });
                _context.SaveChanges();
                TempData["Error"] = "Usuario no encontrado.";
                return View();
            }
            // Rate limit: solo permite solicitar cada 5 minutos
            if (usuario.RecuperacionTokenExpira.HasValue && usuario.RecuperacionTokenExpira.Value > DateTime.UtcNow && usuario.RecuperacionToken != null)
            {
                var minutosRestantes = (usuario.RecuperacionTokenExpira.Value - DateTime.UtcNow).TotalMinutes;
                if (minutosRestantes > 55) // Si el token fue generado hace menos de 5 minutos
                {
                    _context.RecuperacionIntentos.Add(new Models.RecuperacionIntento {
                        UsuarioNombre = usuario.UsuarioNombre,
                        Fecha = DateTime.UtcNow,
                        Ip = ip,
                        Exitoso = false,
                        Motivo = "Rate limit"
                    });
                    _context.SaveChanges();
                    TempData["Error"] = $"Ya solicitaste un enlace recientemente. Por favor espera unos minutos antes de volver a intentarlo.";
                    return View();
                }
            }
            // Generar token
            usuario.RecuperacionToken = Guid.NewGuid().ToString("N");
            usuario.RecuperacionTokenExpira = DateTime.UtcNow.AddHours(1);
            _context.SaveChanges();

            // Enviar email (simulado, puedes adaptar a tu SMTP real)
            string url = Url.Action("Restablecer", "Recuperacion", new { token = usuario.RecuperacionToken ?? string.Empty }, Request.Scheme) ?? string.Empty;
            string mensaje = $"Hola {usuario.Nombre},\n\nHaz clic en el siguiente enlace para restablecer tu contraseña: {url}\n\nEste enlace expirará en 1 hora.";
            bool exito = true;
            string motivo = "";
            try
            {
                var smtpHost = _config["Smtp:Host"] ?? string.Empty;
                var smtpPortStr = _config["Smtp:Port"] ?? "587";
                var smtpUser = _config["Smtp:User"] ?? string.Empty;
                var smtpPass = _config["Smtp:Pass"] ?? string.Empty;
                var smtpFrom = _config["Smtp:From"] ?? string.Empty;
                var smtp = new SmtpClient(smtpHost, int.TryParse(smtpPortStr, out var port) ? port : 587);
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = true;
                smtp.Send(smtpFrom, usuario.UsuarioNombre ?? string.Empty, "Recupera tu contraseña", mensaje);
            }
            catch (Exception ex)
            {
                exito = false;
                motivo = ex.Message;
                TempData["Info"] = $"Enlace de recuperación: {url}";
            }
            _context.RecuperacionIntentos.Add(new Models.RecuperacionIntento {
                UsuarioNombre = usuario.UsuarioNombre,
                Fecha = DateTime.UtcNow,
                Ip = ip,
                Exitoso = exito,
                Motivo = exito ? "Correo enviado" : motivo
            });
            _context.SaveChanges();
            TempData["Success"] = "Si el usuario existe, se ha enviado un enlace de recuperación.";
            return RedirectToAction("Solicitar");
        }

        [HttpGet]
        public IActionResult Restablecer(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token inválido.";
                return RedirectToAction("Solicitar");
            }
            var usuario = _context.Usuarios.FirstOrDefault(u => u.RecuperacionToken == token && u.RecuperacionTokenExpira > DateTime.UtcNow);
            if (usuario == null)
            {
                TempData["Error"] = "El enlace es inválido o ha expirado.";
                return RedirectToAction("Solicitar");
            }
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult Restablecer(string token, string nueva, string repetir)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token inválido.";
                return RedirectToAction("Solicitar");
            }
            var usuario = _context.Usuarios.FirstOrDefault(u => u.RecuperacionToken == token && u.RecuperacionTokenExpira > DateTime.UtcNow);
            if (usuario == null)
            {
                TempData["Error"] = "El enlace es inválido o ha expirado.";
                return RedirectToAction("Solicitar");
            }
            if (string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(repetir))
            {
                TempData["Error"] = "Todos los campos son obligatorios.";
                ViewBag.Token = token;
                return View();
            }
            if (nueva != repetir)
            {
                TempData["Error"] = "Las contraseñas no coinciden.";
                ViewBag.Token = token;
                return View();
            }
            if (nueva.Length < 8 || nueva.Length > 30)
            {
                TempData["Error"] = "La contraseña debe tener entre 8 y 30 caracteres.";
                ViewBag.Token = token;
                return View();
            }
            var hasher = new PasswordHasher<Usuario>();
            usuario.Clave = hasher.HashPassword(usuario, nueva);
            usuario.RecuperacionToken = null;
            usuario.RecuperacionTokenExpira = null;
            _context.SaveChanges();
            TempData["Success"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login", "Auth");
        }
    }
}
