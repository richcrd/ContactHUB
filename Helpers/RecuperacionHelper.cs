using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ContactHUB.Helpers
{
    public class RecuperacionHelper
    {
        private readonly ContactDbContext _context;
        private readonly IConfiguration _config;
        public RecuperacionHelper(ContactDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public bool ValidarCorreo(string correo)
        {
            return !string.IsNullOrWhiteSpace(correo);
        }

        public string GenerarOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public void GuardarOtp(string correo, string codigo)
        {
            _context.RecuperacionOtps.Add(new RecuperacionOtp
            {
                Correo = correo,
                Codigo = codigo,
                Expira = DateTime.UtcNow.AddMinutes(10),
                Usado = false
            });
            _context.SaveChanges();
        }

        public bool EnviarOtpPorCorreo(string correo, string codigo, Action<string> setInfo)
        {
            string mensaje = $"Tu código de recuperación es: {codigo}. Expira en 10 minutos.";
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
                return true;
            }
            catch
            {
                setInfo?.Invoke($"Código de recuperación: {codigo}");
                return false;
            }
        }

        public bool ValidarOtpCodigo(string correo, string codigo)
        {
            var otp = _context.RecuperacionOtps
                .FirstOrDefault(o => o.Correo == correo && o.Codigo == codigo && !o.Usado && o.Expira > DateTime.UtcNow);
            if (otp == null) return false;
            otp.Usado = true;
            _context.SaveChanges();
            return true;
        }

        public string ValidarRestablecer(string nueva, string repetir)
        {
            if (string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(repetir))
                return "Todos los campos son obligatorios.";
            if (nueva != repetir)
                return "Las contraseñas no coinciden.";
            if (nueva.Length < 8 || nueva.Length > 30)
                return "La contraseña debe tener entre 8 y 30 caracteres.";
            return string.Empty;
        }

        public bool RestablecerClave(string correo, string nueva)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == correo);
            if (usuario == null) return false;
            var hasher = new PasswordHasher<Usuario>();
            usuario.Clave = hasher.HashPassword(usuario, nueva);
            _context.SaveChanges();
            return true;
        }
    }
}
