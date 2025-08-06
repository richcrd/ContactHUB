using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContactHUB.Helpers
{
    public class ContactosQueryHelper
    {
        private readonly ContactDbContext _context;
        public ContactosQueryHelper(ContactDbContext context)
        {
            _context = context;
        }

        public IQueryable<Contacto> GetContactosQuery(int usuarioId, string? search, int? departamentoId, int? etiquetaId, string orden = "recientes")
        {
            var query = _context.Contactos
                .Include(c => c.Departamento)
                .Include(c => c.Estado)
                .Include(c => c.ContactoEtiquetas).ThenInclude(ce => ce.Etiqueta)
                .Where(c => c.Id_Usuario == usuarioId && c.Id_Estado == 1);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Nombre.Contains(search) || c.Apellido.Contains(search) || c.Telefono.Contains(search));
            if (departamentoId.HasValue)
                query = query.Where(c => c.Id_Departamento == departamentoId);
            if (etiquetaId.HasValue)
                query = query.Where(c => c.ContactoEtiquetas.Any(ce => ce.IdEtiqueta == etiquetaId));

            if (orden == "recientes")
                query = query.OrderByDescending(c => c.IdContacto);
            else if (orden == "antiguos")
                query = query.OrderBy(c => c.IdContacto);

            return query;
        }

        public List<Departamento> GetDepartamentos() => _context.Departamentos.ToList();
        public List<Etiqueta> GetEtiquetas() => _context.Etiquetas.ToList();
    }
}
