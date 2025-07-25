using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContactHUB.Models;
namespace ContactHUB.Models;

[Table("ContactoEtiqueta")]
public class ContactoEtiqueta
{
    [Key]
    public int IdContacto { get; set; }
    public Contacto Contacto { get; set; } = null!;
    public int IdEtiqueta { get; set; }
    public Etiqueta Etiqueta { get; set; } = null!;
}
