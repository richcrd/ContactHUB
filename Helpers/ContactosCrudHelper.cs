using ContactHUB.Data;
using ContactHUB.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ContactHUB.Helpers
{
    public class ContactosCrudHelper
    {
        private readonly ContactDbContext _context;
        public ContactosCrudHelper(ContactDbContext context)
        {
            _context = context;
        }

        public bool UsuarioValido(string usuarioNombre, out Usuario usuario)
        {
            usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == usuarioNombre);
            return usuario != null;
        }

        public bool LimiteContactos(int usuarioId)
        {
            return _context.Contactos.Count(c => c.Id_Usuario == usuarioId && c.Id_Estado == 1) >= 20;
        }

        public bool DuplicadoContacto(int usuarioId, string telefono, string correo, int? idIgnorar = null)
        {
            return _context.Contactos.Any(c => c.Id_Usuario == usuarioId && (idIgnorar == null || c.IdContacto != idIgnorar) && (c.Telefono == telefono || c.Correo == correo));
        }

        public void AgregarContacto(Contacto contacto, int[] etiquetas)
        {
            _context.Contactos.Add(contacto);
            _context.SaveChanges();
            foreach (var idEtiqueta in etiquetas)
            {
                _context.ContactoEtiquetas.Add(new ContactoEtiqueta
                {
                    IdContacto = contacto.IdContacto,
                    IdEtiqueta = idEtiqueta
                });
            }
            _context.SaveChanges();
        }

        public void RegistrarAccion(int usuarioId, string tipoAccion)
        {
            _context.AccionUsuarios.Add(new AccionUsuario
            {
                IdUsuario = usuarioId,
                TipoAccion = tipoAccion,
                Fecha = DateTime.Now
            });
            _context.SaveChanges();
        }

        public Contacto? ObtenerContacto(int id)
        {
            return _context.Contactos.Include(c => c.ContactoEtiquetas).FirstOrDefault(c => c.IdContacto == id);
        }

        public void EditarContacto(Contacto contactoDb, Contacto contactoEdit, int[] etiquetas)
        {
            contactoDb.Nombre = contactoEdit.Nombre;
            contactoDb.Apellido = contactoEdit.Apellido;
            contactoDb.Telefono = contactoEdit.Telefono;
            contactoDb.Correo = contactoEdit.Correo;
            contactoDb.Direccion = contactoEdit.Direccion;
            contactoDb.Id_Departamento = contactoEdit.Id_Departamento;
            contactoDb.ContactoEtiquetas.Clear();
            foreach (var idEtiqueta in etiquetas)
            {
                contactoDb.ContactoEtiquetas.Add(new ContactoEtiqueta
                {
                    IdContacto = contactoDb.IdContacto,
                    IdEtiqueta = idEtiqueta
                });
            }
            _context.SaveChanges();
        }

        public bool LimiteEdicionesHoy(int usuarioId)
        {
            var hoy = DateTime.Today;
            return _context.AccionUsuarios.Count(a => a.IdUsuario == usuarioId && a.TipoAccion == "editar_contacto" && a.Fecha >= hoy) >= 100;
        }

        public bool LimiteEliminacionesHoy(int usuarioId)
        {
            var hoy = DateTime.Today;
            return _context.AccionUsuarios.Count(a => a.IdUsuario == usuarioId && a.TipoAccion == "eliminar_contacto" && a.Fecha >= hoy) >= 5;
        }

        public void EliminarContacto(int id)
        {
            var contacto = _context.Contactos.FirstOrDefault(c => c.IdContacto == id);
            if (contacto != null)
            {
                contacto.Id_Estado = 2;
                _context.SaveChanges();
            }
        }
    }
}
