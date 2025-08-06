using ContactHUB.Models;

namespace ContactHUB.Helpers
{
    public class ContactosActionResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public static ContactosActionResult Ok(string? message = null, object? data = null)
            => new ContactosActionResult { Success = true, Message = message, Data = data };
        public static ContactosActionResult Fail(string error)
            => new ContactosActionResult { Success = false, Error = error };
    }

    public static class ContactosActionHelper
    {
        public static ContactosActionResult ValidarCreate(ContactosCrudHelper crudHelper, string? usuarioNombre, Contacto contacto, int[] etiquetas)
        {
            if (!ContactosValidationHelper.ValidarUsuario(usuarioNombre, out var error))
                return ContactosActionResult.Fail(error);
            if (!crudHelper.UsuarioValido(usuarioNombre, out var usuario))
                return ContactosActionResult.Fail("No autorizado.");
            if (!ContactosValidationHelper.ValidarContacto(contacto, out error))
                return ContactosActionResult.Fail(error);
            if (!ContactosValidationHelper.ValidarEtiquetas(etiquetas, out error))
                return ContactosActionResult.Fail(error);
            if (crudHelper.LimiteContactos(usuario.IdUsuario))
                return ContactosActionResult.Fail("Has alcanzado el límite total de contactos permitidos (20).");
            if (crudHelper.DuplicadoContacto(usuario.IdUsuario, contacto.Telefono, contacto.Correo))
                return ContactosActionResult.Fail("Ya existe un contacto con ese teléfono o correo.");
            return ContactosActionResult.Ok(data: usuario);
        }

        public static ContactosActionResult ValidarEdit(ContactosCrudHelper crudHelper, string? usuarioNombre, Contacto contacto, int[] etiquetas)
        {
            if (!ContactosValidationHelper.ValidarUsuario(usuarioNombre, out var error))
                return ContactosActionResult.Fail(error);
            if (!crudHelper.UsuarioValido(usuarioNombre, out var usuario))
                return ContactosActionResult.Fail("No autorizado.");
            if (!ContactosValidationHelper.ValidarContacto(contacto, out error))
                return ContactosActionResult.Fail(error);
            if (!ContactosValidationHelper.ValidarEtiquetas(etiquetas, out error))
                return ContactosActionResult.Fail(error);
            if (crudHelper.LimiteEdicionesHoy(usuario.IdUsuario))
                return ContactosActionResult.Fail("Has alcanzado el límite de ediciones de contactos por hoy.");
            if (crudHelper.DuplicadoContacto(usuario.IdUsuario, contacto.Telefono, contacto.Correo, contacto.IdContacto))
                return ContactosActionResult.Fail("Ya existe otro contacto con ese teléfono o correo.");
            var contactoDb = crudHelper.ObtenerContacto(contacto.IdContacto);
            if (contactoDb == null)
                return ContactosActionResult.Fail("Contacto no encontrado.");
            return ContactosActionResult.Ok(data: new { usuario, contactoDb });
        }

        public static ContactosActionResult ValidarDelete(ContactosCrudHelper crudHelper, string? usuarioNombre, int id)
        {
            if (!ContactosValidationHelper.ValidarUsuario(usuarioNombre, out var error))
                return ContactosActionResult.Fail(error);
            if (!crudHelper.UsuarioValido(usuarioNombre, out var usuario))
                return ContactosActionResult.Fail("No autorizado.");
            if (crudHelper.LimiteEliminacionesHoy(usuario.IdUsuario))
                return ContactosActionResult.Fail("Has alcanzado el límite de eliminaciones de contactos por hoy.");
            var contacto = crudHelper.ObtenerContacto(id);
            if (!ContactosValidationHelper.ValidarContacto(contacto, out error))
                return ContactosActionResult.Fail(error);
            return ContactosActionResult.Ok(data: new { usuario, contacto });
        }
    }
}
