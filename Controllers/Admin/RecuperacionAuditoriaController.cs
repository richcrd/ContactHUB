using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContactHUB.Data;
using System.Linq;

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
            // Solo admin
            var usuario = User.Identity?.Name != null ? _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == User.Identity.Name) : null;
            if (usuario == null || usuario.IdRol != 1)
            {
                return Forbid();
            }
            var intentos = _context.RecuperacionIntentos.OrderByDescending(i => i.Fecha).Take(200).ToList();
            return View(intentos);
        }
    }
}
