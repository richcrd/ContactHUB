using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;

namespace ContactHUB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EtiquetasController : Controller
    {
        private readonly ContactDbContext _context;
        public EtiquetasController(ContactDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var etiquetas = _context.Etiquetas.ToList();
            return View(etiquetas);
        }

        public IActionResult Crear()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EtiquetaPartial", new Etiqueta { Id_Estado = 1 });
            return View(new Etiqueta { Id_Estado = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Etiqueta etiqueta)
        {
            if (ModelState.IsValid)
            {
                etiqueta.Id_Estado = 1; // Asignar estado por defecto
                _context.Etiquetas.Add(etiqueta);
                _context.SaveChanges();
                TempData["Success"] = "Etiqueta creada correctamente.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Content("{\"success\":true}", "application/json");
                return RedirectToAction(nameof(Index));
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Devolver errores de ModelState para depuración
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var html = RenderPartialViewToString("_EtiquetaPartial", etiqueta);
                return Content($"{{\"success\":false,\"html\":{System.Text.Json.JsonSerializer.Serialize(html)},\"errors\":{System.Text.Json.JsonSerializer.Serialize(errors)}}}", "application/json");
            }
            return View(etiqueta);
        }

        public IActionResult Editar(int id)
        {
            var etiqueta = _context.Etiquetas.Find(id);
            if (etiqueta == null) return NotFound();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EtiquetaPartial", etiqueta);
            return View(etiqueta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Etiqueta etiqueta)
        {
            if (ModelState.IsValid)
            {
                _context.Etiquetas.Update(etiqueta);
                _context.SaveChanges();
                TempData["Success"] = "Etiqueta actualizada correctamente.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Content("{\"success\":true}", "application/json");
                return RedirectToAction(nameof(Index));
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var html = RenderPartialViewToString("_EtiquetaPartial", etiqueta);
                return Content($"{{\"success\":false,\"html\":{System.Text.Json.JsonSerializer.Serialize(html)}}}", "application/json");
            }
            return View(etiqueta);
        }

        // Helper para renderizar partial a string
        private string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new System.IO.StringWriter())
            {
                var serviceProvider = this.ControllerContext.HttpContext.RequestServices;
                var engine = (Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine?)serviceProvider.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine));
                if (engine == null)
                    throw new System.Exception("No se pudo obtener el servicio ICompositeViewEngine");
                var viewResult = engine.FindView(this.ControllerContext, viewName, false);
                if (!viewResult.Success || viewResult.View == null)
                    throw new System.Exception($"No se pudo encontrar la vista parcial '{viewName}'");
                var viewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext(
                    this.ControllerContext,
                    viewResult.View,
                    this.ViewData,
                    this.TempData,
                    sw,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
                );
                viewResult.View.RenderAsync(viewContext).Wait();
                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var etiqueta = _context.Etiquetas.Find(id);
            if (etiqueta == null) return NotFound();
            _context.Etiquetas.Remove(etiqueta);
            _context.SaveChanges();
            TempData["Success"] = "Etiqueta eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
