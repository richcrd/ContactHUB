using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactHUB.Data;
using ContactHUB.Helpers;
using ContactHUB.ViewModels;
using ContactHUB.Models;
using System.Linq;

namespace ContactHUB.Controllers
{
    [Authorize]
    public class ContactosController : Controller
    {
        private readonly ContactDbContext _context;
        private readonly ContactosQueryHelper _queryHelper;
        private readonly ContactosCrudHelper _crudHelper;
        private readonly ContactosExportHelper _exportHelper;
        private readonly ContactosStatsHelper _statsHelper;
        public ContactosController(ContactDbContext context)
        {
            _context = context;
            _queryHelper = new ContactosQueryHelper(_context);
            _crudHelper = new ContactosCrudHelper(_context);
            _exportHelper = new ContactosExportHelper(_context);
            _statsHelper = new ContactosStatsHelper(_context);
        }

        public IActionResult Index(string search, int? departamentoId, int? etiquetaId, string orden = "recientes", int page = 1)
        {
            var usuarioNombre = User.Identity?.Name;
            if (!ContactosValidationHelper.ValidarUsuario(usuarioNombre, out var error))
                return RedirectToAction("Login", "Auth");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            var contactosQuery = _queryHelper.GetContactosQuery(usuario.IdUsuario, search, departamentoId, etiquetaId, orden);
            int pageSize = 6;
            int totalContactos = contactosQuery.Count();
            var contactos = contactosQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new ContactosPaginadosViewModel
            {
                Contactos = contactos,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling((double)totalContactos / pageSize),
                TotalContactos = totalContactos,
                PageSize = pageSize
            };

            var usuarioId = usuario.IdUsuario;
            ViewBag.TotalContactos = _statsHelper.GetTotalContactos(usuarioId);
            ViewBag.TotalActivos = _statsHelper.GetTotalActivos(usuarioId);
            ViewBag.AgregadosHoy = _statsHelper.GetAgregadosHoy(usuarioId);
            ViewBag.EliminadosHoy = _statsHelper.GetEliminadosHoy(usuarioId);

            ViewBag.Departamentos = _queryHelper.GetDepartamentos();
            ViewBag.Etiquetas = _queryHelper.GetEtiquetas();
            ViewBag.Search = search;
            ViewBag.DepartamentoId = departamentoId;
            ViewBag.EtiquetaId = etiquetaId;
            ViewBag.Orden = orden;

            return View(viewModel);
        }


        public IActionResult Create()
        {
            ViewBag.Departamentos = _context.Departamentos.ToList();
            ViewBag.Etiquetas = _context.Etiquetas.ToList();
            ViewBag.SelectedEtiquetas = new int[0];
            return PartialView("_ContactoFormPartial", new Contacto());
        }

        [HttpPost]
        public IActionResult Create(Contacto contacto, int[] etiquetas)
        {
            var usuarioNombre = User.Identity?.Name;
            var result = ContactosActionHelper.ValidarCreate(_crudHelper, usuarioNombre, contacto, etiquetas);
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return Json(new { success = false, html = result.Error });
            }
            var usuario = (Usuario)result.Data;
            contacto.Id_Usuario = usuario.IdUsuario;
            contacto.Id_Estado = 1;
            _crudHelper.AgregarContacto(contacto, etiquetas);
            _crudHelper.RegistrarAccion(usuario.IdUsuario, "agregar_contacto");
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
        public IActionResult Edit([Bind("IdContacto,Nombre,Apellido,Telefono,Correo,Direccion,Id_Departamento")] Contacto contacto, int[] etiquetas)
        {
            var usuarioNombre = User.Identity?.Name;
            var result = ContactosActionHelper.ValidarEdit(_crudHelper, usuarioNombre, contacto, etiquetas);
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return Json(new { success = false, html = result.Error });
            }
            var usuario = (Usuario)((dynamic)result.Data).usuario;
            var contactoDb = (Contacto)((dynamic)result.Data).contactoDb;
            _crudHelper.EditarContacto(contactoDb, contacto, etiquetas);
            _crudHelper.RegistrarAccion(usuario.IdUsuario, "editar_contacto");
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
            var result = ContactosActionHelper.ValidarDelete(_crudHelper, usuarioNombre, id);
            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                if (result.Error == "No autorizado.")
                    return Unauthorized();
                if (result.Error == "Has alcanzado el límite de eliminaciones de contactos por hoy.")
                    return RedirectToAction("Index");
                return NotFound();
            }
            var usuario = (Usuario)((dynamic)result.Data).usuario;
            var contacto = (Contacto)((dynamic)result.Data).contacto;
            _crudHelper.EliminarContacto(id);
            _crudHelper.RegistrarAccion(usuario.IdUsuario, "eliminar_contacto");
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
            var contactos = _exportHelper.GetContactosForExport(usuario.IdUsuario, search, departamentoId, etiquetaId, orden, excelOrder: true);
            var bytes = _exportHelper.ExportToExcel(contactos);
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
            var contactos = _exportHelper.GetContactosForExport(usuario.IdUsuario, search, departamentoId, etiquetaId, orden);
            var bytes = _exportHelper.ExportToJson(contactos);
            return File(bytes, "application/json", "contactos.json");
        }
    }
}
