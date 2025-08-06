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
                var zonaNicaragua = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                var acciones = _context.AccionUsuarios
                    .OrderByDescending(a => a.Fecha)
                    .Take(200)
                    .ToList()
                    .Select(a => {
                        a.Fecha = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(a.Fecha, DateTimeKind.Utc), zonaNicaragua);
                        return a;
                    })
                    .ToList();
                return View(acciones);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "No se pudo conectar a la base de datos. Por favor, intente más tarde.";
                return View(new List<object>());
            }
        }
    }
}
