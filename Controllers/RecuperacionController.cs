using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ContactHUB.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public IActionResult Solicitar()
        {
            TempData.Remove("Error");
            return View();
        }

        [HttpPost]
        public IActionResult Solicitar(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                TempData["Error"] = "Debes ingresar tu correo.";
                return View();
            }

            // Generar OTP
            var codigo = new Random().Next(100000, 999999).ToString();
            var expira = DateTime.UtcNow.AddMinutes(10);

            // Guardar OTP
            _context.RecuperacionOtps.Add(new RecuperacionOtp
            {
                Correo = correo,
                Codigo = codigo,
                Expira = expira,
                Usado = false
            });
            _context.SaveChanges();

            // Enviar OTP por correo
            string mensaje = $"Tu código de recuperación es: {codigo}. Expira en 10 minutos.";
            bool enviado = false;
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
                smtp.Send(smtpFrom, correo, "Código de recuperación", mensaje);
                enviado = true;
            }
            catch
            {
                TempData["Info"] = $"Código de recuperación: {codigo}";
            }

            TempData["Correo"] = correo;
            if (enviado)
                TempData["Success"] = "Hemos enviado a tu correo el código OTP de recuperación.";
            else
                TempData["Error"] = "No se pudo enviar el correo, pero aquí tienes tu código OTP.";
            return RedirectToAction("ValidarOtp");
        }

        [HttpGet]
        public IActionResult ValidarOtp()
        {
            ViewBag.Correo = TempData["Correo"] as string;
            return View();
        }

        [HttpPost]
        public IActionResult ValidarOtp(string correo, string codigo)
        {
            var otp = _context.RecuperacionOtps
                .FirstOrDefault(o => o.Correo == correo && o.Codigo == codigo && !o.Usado && o.Expira > DateTime.UtcNow);

            if (otp == null)
            {
                TempData["Error"] = "Código incorrecto o expirado.";
                TempData["Correo"] = correo;
                return RedirectToAction("ValidarOtp");
            }

            otp.Usado = true;
            _context.SaveChanges();

            TempData["Correo"] = correo;
            TempData["OtpValidado"] = true;
            TempData["Success"] = "Código OTP validado correctamente. Ahora puedes restablecer tu contraseña.";
            return RedirectToAction("Restablecer");
        }

        [HttpGet]
        public IActionResult Restablecer()
        {
            if (TempData["OtpValidado"] == null || TempData["Correo"] == null)
            {
                TempData["Error"] = "Debes validar tu código primero.";
                return RedirectToAction("Solicitar");
            }
            ViewBag.Correo = TempData["Correo"] as string;
            return View();
        }

        [HttpPost]
        public IActionResult Restablecer(string correo, string nueva, string repetir)
        {
            if (string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(repetir))
            {
                TempData["Error"] = "Todos los campos son obligatorios.";
                TempData["Correo"] = correo;
                TempData["OtpValidado"] = true;
                return RedirectToAction("Restablecer");
            }
            if (nueva != repetir)
            {
                TempData["Error"] = "Las contraseñas no coinciden.";
                TempData["Correo"] = correo;
                TempData["OtpValidado"] = true;
                return RedirectToAction("Restablecer");
            }
            if (nueva.Length < 8 || nueva.Length > 30)
            {
                TempData["Error"] = "La contraseña debe tener entre 8 y 30 caracteres.";
                TempData["Correo"] = correo;
                TempData["OtpValidado"] = true;
                return RedirectToAction("Restablecer");
            }

            // Buscar usuario por Email
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == correo);
            if (usuario == null)
            {
                TempData["Error"] = "No existe usuario con ese correo.";
                return RedirectToAction("Solicitar");
            }
            var hasher = new PasswordHasher<Usuario>();
            usuario.Clave = hasher.HashPassword(usuario, nueva);
            _context.SaveChanges();

            TempData["Success"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login", "Auth", new { area = "" });
        }
    }
}
