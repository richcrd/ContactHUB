using ContactHUB.Data;
using ContactHUB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;

namespace ContactHUB.Helpers
{
    public class ContactosExportHelper
    {
        private readonly ContactDbContext _context;
        public ContactosExportHelper(ContactDbContext context)
        {
            _context = context;
        }

        public List<Contacto> GetContactosForExport(int usuarioId, string? search, int? departamentoId, int? etiquetaId, string orden, bool excelOrder = false)
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
            if (excelOrder)
                query = query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);
            else if (orden == "recientes")
                query = query.OrderByDescending(c => c.IdContacto);
            else if (orden == "antiguos")
                query = query.OrderBy(c => c.IdContacto);
            return query.ToList();
        }

        public byte[] ExportToExcel(List<Contacto> contactos)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Nombre,Apellido,Telefono,Correo,Departamento,Direccion,Etiquetas");
            foreach (var c in contactos)
            {
                var etiquetas = c.ContactoEtiquetas != null ? string.Join("|", c.ContactoEtiquetas.Select(e => e.Etiqueta?.Nombre).Where(n => !string.IsNullOrEmpty(n))) : "";
                csv.AppendLine($"{EscapeCsv(c.Nombre)},{EscapeCsv(c.Apellido)},{EscapeCsv(c.Telefono)},{EscapeCsv(c.Correo)},{EscapeCsv(c.Departamento?.Nombre)},{EscapeCsv(c.Direccion)},{EscapeCsv(etiquetas)}");
            }
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public byte[] ExportToJson(List<Contacto> contactos)
        {
            var json = JsonSerializer.Serialize(contactos.Select(c => new {
                c.Nombre,
                c.Apellido,
                c.Telefono,
                c.Correo,
                c.Direccion,
                Departamento = c.Departamento?.Nombre,
                Etiquetas = c.ContactoEtiquetas != null ? c.ContactoEtiquetas.Select(e => e.Etiqueta?.Nombre ?? string.Empty).ToList() : new List<string>()
            }), new JsonSerializerOptions { WriteIndented = true });
            return Encoding.UTF8.GetBytes(json);
        }

        public static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            var escaped = value.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }
    }
}
