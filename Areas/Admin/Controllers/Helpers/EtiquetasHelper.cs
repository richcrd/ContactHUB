using ContactHUB.Data;
using ContactHUB.Models;
using System.Linq;
using System.Collections.Generic;

namespace ContactHUB.Areas.Admin.Controllers.Helpers
{
    public static class EtiquetasHelper
    {
        public static List<Etiqueta> GetEtiquetas(ContactDbContext context)
        {
            return context.Etiquetas.ToList();
        }

        public static Etiqueta GetEtiqueta(ContactDbContext context, int id)
        {
            return context.Etiquetas.Find(id);
        }

        public static void AddEtiqueta(ContactDbContext context, Etiqueta etiqueta)
        {
            etiqueta.Id_Estado = 1;
            context.Etiquetas.Add(etiqueta);
            context.SaveChanges();
        }

        public static void UpdateEtiqueta(ContactDbContext context, Etiqueta etiqueta)
        {
            context.Etiquetas.Update(etiqueta);
            context.SaveChanges();
        }

        public static void RemoveEtiqueta(ContactDbContext context, Etiqueta etiqueta)
        {
            context.Etiquetas.Remove(etiqueta);
            context.SaveChanges();
        }
    }
}
