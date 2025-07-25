using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContactHUB.Models;
namespace ContactHUB.Models;

[Table("Departamento")]
public class Departamento
{
    [Key]
    public int IdDepartamento { get; set; }
    public string Nombre { get; set; } = null!;
    public int Id_Estado { get; set; }
    public Estado Estado { get; set; } = null!;
    public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
}
