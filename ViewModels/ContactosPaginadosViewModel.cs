using ContactHUB.Models;

namespace ContactHUB.ViewModels
{
    public class ContactosPaginadosViewModel
    {
        public IEnumerable<Contacto> Contactos { get; set; } = new List<Contacto>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalContactos { get; set; }
        public int PageSize { get; set; }
    }
}
