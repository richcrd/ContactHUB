using System;
using System.ComponentModel.DataAnnotations;

namespace ContactHUB.Models
{
    public class RecuperacionOtp
    {
        [Key]
        public int Id { get; set; }
        public string Correo { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public DateTime Expira { get; set; }
        public bool Usado { get; set; } = false;
    }
}
