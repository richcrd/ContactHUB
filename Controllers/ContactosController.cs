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
            var hoy = DateTime.Today;
            ViewBag.AgregadosHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuarioId && a.TipoAccion == "agregar_contacto" && a.Fecha >= hoy);
            ViewBag.EliminadosHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuarioId && a.TipoAccion == "eliminar_contacto" && a.Fecha >= hoy);

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
            {
                TempData["Error"] = "No autorizado.";
                return Json(new { success = false, html = "No autorizado" });
            }

            // Limite de 20 contactos en total por usuario
            var totalContactos = _context.Contactos.Count(c => c.Id_Usuario == usuario.IdUsuario && c.Id_Estado == 1);
            if (totalContactos >= 20)
            {
                TempData["Error"] = "Has alcanzado el límite total de contactos permitidos (20).";
                return Json(new { success = false, html = "Has alcanzado el límite total de contactos permitidos (20)." });
            }

            // Validar duplicados de teléfono o correo para este usuario
            if (_context.Contactos.Any(c => c.Id_Usuario == usuario.IdUsuario && (c.Telefono == contacto.Telefono || c.Correo == contacto.Correo)))
            {
                TempData["Error"] = "Ya existe un contacto con ese teléfono o correo.";
                return Json(new { success = false, html = "Ya existe un contacto con ese teléfono o correo." });
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

            TempData["Success"] = "Contacto creado exitosamente.";
            return Json(new { success = true, message = "Contacto creado exitosamente." });
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
            {
                TempData["Error"] = "No autorizado.";
                return Json(new { success = false, html = "No autorizado" });
            }

            // Limite de 100 ediciones por usuario por día
            var hoy = DateTime.Today;
            var edicionesHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuario.IdUsuario && a.TipoAccion == "editar_contacto" && a.Fecha >= hoy);
            if (edicionesHoy >= 100)
            {
                TempData["Error"] = "Has alcanzado el límite de ediciones de contactos por hoy.";
                return Json(new { success = false, html = "Has alcanzado el límite de ediciones de contactos por hoy." });
            }

            // Validar duplicados de teléfono o correo para este usuario (ignorando el contacto actual)
            if (_context.Contactos.Any(c => c.Id_Usuario == usuario.IdUsuario && c.IdContacto != contacto.IdContacto && (c.Telefono == contacto.Telefono || c.Correo == contacto.Correo)))
            {
                TempData["Error"] = "Ya existe otro contacto con ese teléfono o correo.";
                return Json(new { success = false, html = "Ya existe otro contacto con ese teléfono o correo." });
            }

            var contactoDb = _context.Contactos.Include(c => c.ContactoEtiquetas).FirstOrDefault(c => c.IdContacto == contacto.IdContacto);
            if (contactoDb == null)
            {
                TempData["Error"] = "Contacto no encontrado.";
                return Json(new { success = false, html = "No encontrado" });
            }

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

            TempData["Success"] = "Contacto editado exitosamente.";
            return Json(new { success = true, message = "Contacto editado exitosamente." });
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
            if (usuario == null)
            {
                TempData["Error"] = "No autorizado.";
                return Unauthorized();
            }

            // Limite de 5 eliminaciones por usuario por día
            var hoy = DateTime.Today;
            var eliminacionesHoy = _context.AccionUsuarios.Count(a => a.IdUsuario == usuario.IdUsuario && a.TipoAccion == "eliminar_contacto" && a.Fecha >= hoy);
            if (eliminacionesHoy >= 5)
            {
                TempData["Error"] = "Has alcanzado el límite de eliminaciones de contactos por hoy.";
                return RedirectToAction("Index");
            }

            var contacto = _context.Contactos.FirstOrDefault(c => c.IdContacto == id);
            if (contacto == null)
            {
                TempData["Error"] = "Contacto no encontrado.";
                return NotFound();
            }
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

            TempData["Success"] = "Contacto eliminado exitosamente.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult ExportExcel(string search, int? departamentoId, int? etiquetaId, string orden = "recientes")
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
                .Where(c => c.Id_Usuario == usuario.IdUsuario && c.Id_Estado == 1);
            if (!string.IsNullOrWhiteSpace(search))
                contactosQuery = contactosQuery.Where(c => c.Nombre.Contains(search) || c.Apellido.Contains(search) || c.Telefono.Contains(search));
            if (departamentoId.HasValue)
                contactosQuery = contactosQuery.Where(c => c.Id_Departamento == departamentoId);
            if (etiquetaId.HasValue)
                contactosQuery = contactosQuery.Where(c => c.ContactoEtiquetas.Any(ce => ce.IdEtiqueta == etiquetaId));

            // Ordenar por Apellido, Nombre (más útil para usuario)
            contactosQuery = contactosQuery.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);
            var contactos = contactosQuery.ToList();

            // Columnas: Nombre, Apellido, Teléfono, Correo, Departamento, Dirección, Etiquetas
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Nombre,Apellido,Telefono,Correo,Departamento,Direccion,Etiquetas");
            foreach (var c in contactos)
            {
                var etiquetas = c.ContactoEtiquetas != null ? string.Join("|", c.ContactoEtiquetas.Select(e => e.Etiqueta?.Nombre).Where(n => !string.IsNullOrEmpty(n))) : "";
                csv.AppendLine($"{EscapeCsv(c.Nombre)},{EscapeCsv(c.Apellido)},{EscapeCsv(c.Telefono)},{EscapeCsv(c.Correo)},{EscapeCsv(c.Departamento?.Nombre)},{EscapeCsv(c.Direccion)},{EscapeCsv(etiquetas)}");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "contactos.csv");
        }

        [HttpGet]
        public IActionResult ExportJson(string search, int? departamentoId, int? etiquetaId, string orden = "recientes")
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
                .Where(c => c.Id_Usuario == usuario.IdUsuario && c.Id_Estado == 1);
            if (!string.IsNullOrWhiteSpace(search))
                contactosQuery = contactosQuery.Where(c => c.Nombre.Contains(search) || c.Apellido.Contains(search) || c.Telefono.Contains(search));
            if (departamentoId.HasValue)
                contactosQuery = contactosQuery.Where(c => c.Id_Departamento == departamentoId);
            if (etiquetaId.HasValue)
                contactosQuery = contactosQuery.Where(c => c.ContactoEtiquetas.Any(ce => ce.IdEtiqueta == etiquetaId));
            if (orden == "recientes")
                contactosQuery = contactosQuery.OrderByDescending(c => c.IdContacto);
            else if (orden == "antiguos")
                contactosQuery = contactosQuery.OrderBy(c => c.IdContacto);
            var contactos = contactosQuery.ToList();

            var json = System.Text.Json.JsonSerializer.Serialize(contactos.Select(c => new {
                c.Nombre,
                c.Apellido,
                c.Telefono,
                c.Correo,
                c.Direccion,
                Departamento = c.Departamento?.Nombre,
                Etiquetas = c.ContactoEtiquetas != null ? c.ContactoEtiquetas.Select(e => e.Etiqueta?.Nombre ?? string.Empty).ToList() : new List<string>()
            }), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "contactos.json");
        }

        private string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            var escaped = value.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }
    }
}
