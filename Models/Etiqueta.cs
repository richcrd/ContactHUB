using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContactHUB.Models;
namespace ContactHUB.Models;

[Table("Etiqueta")]
public class Etiqueta
{
    [Key]
    public int IdEtiqueta { get; set; }
    public string Nombre { get; set; } = null!;
    public int Id_Estado { get; set; }
    public Estado Estado { get; set; } = null!;
    public ICollection<ContactoEtiqueta> ContactoEtiquetas { get; set; } = new List<ContactoEtiqueta>();
}
