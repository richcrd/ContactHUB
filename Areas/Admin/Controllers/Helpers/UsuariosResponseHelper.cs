using Microsoft.AspNetCore.Mvc;

namespace ContactHUB.Areas.Admin.Controllers.Helpers
{
    public static class UsuariosResponseHelper
    {
        public static IActionResult Error(Controller controller, string mensaje)
        {
            controller.TempData["Error"] = mensaje;
            return controller.RedirectToAction("Index");
        }

        public static IActionResult Success(Controller controller, string mensaje)
        {
            controller.TempData["Success"] = mensaje;
            return controller.RedirectToAction("Index");
        }
    }
}
