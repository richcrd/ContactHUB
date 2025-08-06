using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContactHUB.Data;
using ContactHUB.Areas.Admin.Controllers.Helpers;

namespace ContactHUB.Controllers.Admin
{
    [Area("Admin")]
    [Authorize]
    public class RecuperacionAuditoriaController : Controller
    {
        private readonly ContactDbContext _context;
        public RecuperacionAuditoriaController(ContactDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var usuarioNombre = User.Identity?.Name;
            if (!RecuperacionAuditoriaHelper.IsAdmin(_context, usuarioNombre))
                return Forbid();
            var intentos = RecuperacionAuditoriaHelper.GetIntentos(_context);
            return View(intentos);
        }
    }
}
