using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ContactHUB.Models;

[Table("Usuario")]
public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }
    [Column("Usuario")]
    public string UsuarioNombre { get; set; } = null!; // nombre de usuario
    public string Nombre { get; set; } = null!; // nombre real
    public string Clave { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    public int Id_Estado { get; set; }
    public Estado Estado { get; set; } = null!;
    public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();

    // Relación con Rol
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    // Recuperación de contraseña
    public string? RecuperacionToken { get; set; }
    public DateTime? RecuperacionTokenExpira { get; set; }
}
