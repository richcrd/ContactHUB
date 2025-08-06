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
        private readonly ContactHUB.Helpers.RecuperacionHelper _helper;
        public RecuperacionController(ContactDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _helper = new ContactHUB.Helpers.RecuperacionHelper(_context, _config);
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
            if (!_helper.ValidarCorreo(correo))
            {
                TempData["Error"] = "Debes ingresar tu correo.";
                return View();
            }
            var codigo = _helper.GenerarOtp();
            _helper.GuardarOtp(correo, codigo);
            bool enviado = _helper.EnviarOtpPorCorreo(correo, codigo, info => TempData["Info"] = info);
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
            if (!_helper.ValidarOtpCodigo(correo, codigo))
            {
                TempData["Error"] = "Código incorrecto o expirado.";
                TempData["Correo"] = correo;
                return RedirectToAction("ValidarOtp");
            }
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
            var error = _helper.ValidarRestablecer(nueva, repetir);
            if (!string.IsNullOrEmpty(error))
            {
                TempData["Error"] = error;
                TempData["Correo"] = correo;
                TempData["OtpValidado"] = true;
                return RedirectToAction("Restablecer");
            }
            if (!_helper.RestablecerClave(correo, nueva))
            {
                TempData["Error"] = "No existe usuario con ese correo.";
                return RedirectToAction("Solicitar");
            }
            TempData["Success"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login", "Auth", new { area = "" });
        }
    }
}
