using System;

namespace ContactHUB.Models
{
    public class RecuperacionIntento
    {
        public int Id { get; set; }
        public string? UsuarioNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string? Ip { get; set; }
        public bool Exitoso { get; set; }
        public string? Motivo { get; set; }
    }
}
