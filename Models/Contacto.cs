using ContactHUB.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactHUB.Models;

[Table("Contacto")]
public class Contacto
{
    [Key]
    public int IdContacto { get; set; }
    public int Id_Usuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public int? Id_Departamento { get; set; }
    public Departamento? Departamento { get; set; }
    public int? Id_Estado { get; set; }
    public Estado? Estado { get; set; }
    public ICollection<ContactoEtiqueta> ContactoEtiquetas { get; set; } = new List<ContactoEtiqueta>();
}
