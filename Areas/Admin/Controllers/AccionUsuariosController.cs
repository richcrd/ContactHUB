using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using System.Linq;

namespace ContactHUB.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccionUsuariosController : Controller
    {
        private readonly ContactDbContext _context;
        public AccionUsuariosController(ContactDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var acciones = _context.AccionUsuarios
                    .OrderByDescending(a => a.Fecha)
                    .Take(200)
                    .ToList();
                return View(acciones);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "No se pudo conectar a la base de datos. Por favor, intente más tarde.";
                // Puedes loguear el error real si lo deseas: ex.Message
                return View(new List<object>()); // Devuelve una lista vacía para evitar errores en la vista
            }
        }
    }
}
