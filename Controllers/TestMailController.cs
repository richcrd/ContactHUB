using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ContactHUB.Controllers
{
    public class TestMailController : Controller
    {
        private readonly IConfiguration _config;
        public TestMailController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Enviar(string destino)
        {
            if (string.IsNullOrWhiteSpace(destino))
            {
                TempData["Error"] = "Debes ingresar un correo de destino.";
                return View("Index");
            }
            try
            {
                var smtp = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]));
                smtp.Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Pass"]);
                smtp.EnableSsl = true;
                smtp.Send(_config["Smtp:From"], destino, "Prueba de correo ContactHUB", "¡Este es un test de correo para recuperar tu contraseña, tu codigo es: 123456test");
                TempData["Success"] = "Correo enviado correctamente a " + destino;
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Error al enviar: " + ex.Message;
            }
            return View("Index");
        }
    }
}
