using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ContactHUB.Models;

[Table("Estado")]
public class Estado
{
    [Key]
    public int IdEstado { get; set; }
    public string Nombre { get; set; } = null!;
    public ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();
    public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
}
