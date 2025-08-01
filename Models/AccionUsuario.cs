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
        public string? TipoAccion { get; set; } // Ej: "login_fail", "registro", "agregar_contacto", "eliminar_contacto", "editar_contacto"
        public DateTime Fecha { get; set; }
    }
}
