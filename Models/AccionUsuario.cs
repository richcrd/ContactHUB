using System;

namespace ContactHUB.Models
{
    using System.ComponentModel.DataAnnotations;
    public class AccionUsuario
    {
        [Key]
        public int IdAccion { get; set; }
        public int? IdUsuario { get; set; }
        public string? IP { get; set; }
        public string? TipoAccion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
