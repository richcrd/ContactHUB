using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactHUB.Data;
using System.Linq;

namespace ContactHUB.Controllers
{
    [Authorize]
    public class ContactosController : Controller
    {
        private readonly ContactDbContext _context;
        public ContactosController(ContactDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, int? departamentoId, int? etiquetaId, string orden = "recientes")
        {
            var usuarioNombre = User.Identity?.Name;
            if (string.IsNullOrEmpty(usuarioNombre))
                return RedirectToAction("Login", "Auth");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
                return RedirectToAction("Login", "Auth");


            var contactosQuery = _context.Contactos
                .Include(c => c.Departamento)
                .Include(c => c.Estado)
                .Include(c => c.ContactoEtiquetas).ThenInclude(ce => ce.Etiqueta)
                .Where(c => c.Id_Usuario == usuario.IdUsuario && c.Id_Estado == 1); // Solo activos

            if (!string.IsNullOrWhiteSpace(search))
            {
                contactosQuery = contactosQuery.Where(c => c.Nombre.Contains(search) || c.Apellido.Contains(search) || c.Telefono.Contains(search));
            }
            if (departamentoId.HasValue)
            {
                contactosQuery = contactosQuery.Where(c => c.Id_Departamento == departamentoId);
            }
            if (etiquetaId.HasValue)
            {
                contactosQuery = contactosQuery.Where(c => c.ContactoEtiquetas.Any(ce => ce.IdEtiqueta == etiquetaId));
            }

            // Ordenar por agregados recientes o antiguos
            if (orden == "recientes")
            {
                contactosQuery = contactosQuery.OrderByDescending(c => c.IdContacto); // Suponiendo que IdContacto es autoincremental
            }
            else if (orden == "antiguos")
            {
                contactosQuery = contactosQuery.OrderBy(c => c.IdContacto);
            }

            var contactos = contactosQuery.ToList();

            // Estadísticas
            var usuarioId = usuario.IdUsuario;
            ViewBag.TotalContactos = _context.Contactos.Count(c => c.Id_Usuario == usuarioId);
            ViewBag.TotalActivos = _context.Contactos.Count(c => c.Id_Usuario == usuarioId && c.Id_Estado == 1);
            ViewBag.TotalInactivos = _context.Contactos.Count(c => c.Id_Usuario == usuarioId && c.Id_Estado == 2);

            ViewBag.Departamentos = _context.Departamentos.ToList();
            ViewBag.Etiquetas = _context.Etiquetas.ToList();
            ViewBag.Search = search;
            ViewBag.DepartamentoId = departamentoId;
            ViewBag.EtiquetaId = etiquetaId;
            ViewBag.Orden = orden;

            return View(contactos);
        }


        public IActionResult Create()
        {
            ViewBag.Departamentos = _context.Departamentos.ToList();
            ViewBag.Etiquetas = _context.Etiquetas.ToList();
            ViewBag.SelectedEtiquetas = new int[0];
            return PartialView("_ContactoFormPartial", new ContactHUB.Models.Contacto());
        }

        [HttpPost]
        public IActionResult Create(ContactHUB.Models.Contacto contacto, int[] etiquetas)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
                return Json(new { success = false, html = "No autorizado" });

            // Limite de 50 contactos agregados por usuario por día
            var hoy = DateTime.Today;
            var agregadosHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuario.IdUsuario && a.TipoAccion == "agregar_contacto" && a.Fecha >= hoy);
            if (agregadosHoy >= 50)
            {
                return Json(new { success = false, html = "Has alcanzado el límite de contactos agregados por hoy." });
            }

            contacto.Id_Usuario = usuario.IdUsuario;
            contacto.Id_Estado = 1; // Activo por defecto
            _context.Contactos.Add(contacto);
            _context.SaveChanges();

            foreach (var idEtiqueta in etiquetas)
            {
                _context.ContactoEtiquetas.Add(new ContactHUB.Models.ContactoEtiqueta
                {
                    IdContacto = contacto.IdContacto,
                    IdEtiqueta = idEtiqueta
                });
            }
            _context.SaveChanges();

            // Registrar acción en AccionUsuario
            _context.AccionUsuarios.Add(new ContactHUB.Models.AccionUsuario
            {
                IdUsuario = usuario.IdUsuario,
                TipoAccion = "agregar_contacto",
                Fecha = DateTime.Now
            });
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult Edit(int id)
        {
            var contacto = _context.Contactos
                .Include(c => c.ContactoEtiquetas)
                .FirstOrDefault(c => c.IdContacto == id);
            if (contacto == null) return NotFound();
            ViewBag.Departamentos = _context.Departamentos.ToList();
            ViewBag.Etiquetas = _context.Etiquetas.ToList();
            ViewBag.SelectedEtiquetas = contacto.ContactoEtiquetas.Select(ce => ce.IdEtiqueta).ToArray();
            return PartialView("_ContactoFormPartial", contacto);
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Edit([Bind("IdContacto,Nombre,Apellido,Telefono,Correo,Direccion,Id_Departamento")] ContactHUB.Models.Contacto contacto, int[] etiquetas)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
                return Json(new { success = false, html = "No autorizado" });

            // Limite de 100 ediciones por usuario por día
            var hoy = DateTime.Today;
            var edicionesHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuario.IdUsuario && a.TipoAccion == "editar_contacto" && a.Fecha >= hoy);
            if (edicionesHoy >= 100)
            {
                return Json(new { success = false, html = "Has alcanzado el límite de ediciones de contactos por hoy." });
            }

            var contactoDb = _context.Contactos.Include(c => c.ContactoEtiquetas).FirstOrDefault(c => c.IdContacto == contacto.IdContacto);
            if (contactoDb == null) return Json(new { success = false, html = "No encontrado" });

            contactoDb.Nombre = contacto.Nombre;
            contactoDb.Apellido = contacto.Apellido;
            contactoDb.Telefono = contacto.Telefono;
            contactoDb.Correo = contacto.Correo;
            contactoDb.Direccion = contacto.Direccion;
            contactoDb.Id_Departamento = contacto.Id_Departamento;
            // No modificar el Id_Estado al editar, mantener el original

            contactoDb.ContactoEtiquetas.Clear();
            foreach (var idEtiqueta in etiquetas)
            {
                contactoDb.ContactoEtiquetas.Add(new ContactHUB.Models.ContactoEtiqueta
                {
                    IdContacto = contactoDb.IdContacto,
                    IdEtiqueta = idEtiqueta
                });
            }
            _context.SaveChanges();

            // Registrar acción en AccionUsuario
            _context.AccionUsuarios.Add(new ContactHUB.Models.AccionUsuario
            {
                IdUsuario = usuario.IdUsuario,
                TipoAccion = "editar_contacto",
                Fecha = DateTime.Now
            });
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult Delete(int id)
        {
            var contacto = _context.Contactos.Include(c => c.Departamento).FirstOrDefault(c => c.IdContacto == id);
            if (contacto == null) return NotFound();
            return View(contacto);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var usuarioNombre = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null) return Unauthorized();

            // Limite de 5 eliminaciones por usuario por día
            var hoy = DateTime.Today;
            var eliminacionesHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuario.IdUsuario && a.TipoAccion == "eliminar_contacto" && a.Fecha >= hoy);
            if (eliminacionesHoy >= 5)
            {
                TempData["Error"] = "Has alcanzado el límite de eliminaciones de contactos por hoy.";
                return RedirectToAction("Index");
            }

            var contacto = _context.Contactos.FirstOrDefault(c => c.IdContacto == id);
            if (contacto == null) return NotFound();
            // Cambiar estado a Inactivo (Id_Estado = 2)
            contacto.Id_Estado = 2;
            _context.SaveChanges();

            // Registrar acción en AccionUsuario
            _context.AccionUsuarios.Add(new ContactHUB.Models.AccionUsuario
            {
                IdUsuario = usuario.IdUsuario,
                TipoAccion = "eliminar_contacto",
                Fecha = DateTime.Now
            });
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
