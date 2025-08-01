using Microsoft.EntityFrameworkCore;
using ContactHUB.Models;
namespace ContactHUB.Data;

public class ContactDbContext : DbContext
{
    public ContactDbContext(DbContextOptions<ContactDbContext> options) : base(options) { }

    public DbSet<Estado> Estados { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Etiqueta> Etiquetas { get; set; }
    public DbSet<Contacto> Contactos { get; set; }
    public DbSet<ContactoEtiqueta> ContactoEtiquetas { get; set; }
    public DbSet<AccionUsuario> AccionUsuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Estado
        modelBuilder.Entity<Estado>()
            .HasMany(e => e.Departamentos)
            .WithOne(d => d.Estado)
            .HasForeignKey(d => d.Id_Estado);

        modelBuilder.Entity<Estado>()
            .HasMany(e => e.Usuarios)
            .WithOne(u => u.Estado)
            .HasForeignKey(u => u.Id_Estado);

        modelBuilder.Entity<Estado>()
            .HasMany(e => e.Etiquetas)
            .WithOne(et => et.Estado)
            .HasForeignKey(et => et.Id_Estado);

        modelBuilder.Entity<Estado>()
            .HasMany(e => e.Contactos)
            .WithOne(c => c.Estado)
            .HasForeignKey(c => c.Id_Estado)
            .IsRequired(false);

        // Departamento
        modelBuilder.Entity<Departamento>()
            .HasMany(d => d.Contactos)
            .WithOne(c => c.Departamento)
            .HasForeignKey(c => c.Id_Departamento)
            .IsRequired(false);

        // Usuario
        modelBuilder.Entity<Usuario>()
            .HasMany(u => u.Contactos)
            .WithOne(c => c.Usuario)
            .HasForeignKey(c => c.Id_Usuario);

        // Etiqueta
        modelBuilder.Entity<Etiqueta>()
            .HasMany(e => e.ContactoEtiquetas)
            .WithOne(ce => ce.Etiqueta)
            .HasForeignKey(ce => ce.IdEtiqueta);

        // Contacto
        modelBuilder.Entity<Contacto>()
            .HasMany(c => c.ContactoEtiquetas)
            .WithOne(ce => ce.Contacto)
            .HasForeignKey(ce => ce.IdContacto);

        // ContactoEtiqueta (Many-to-Many)
        modelBuilder.Entity<ContactoEtiqueta>()
            .HasKey(ce => new { ce.IdContacto, ce.IdEtiqueta });
        modelBuilder.Entity<ContactoEtiqueta>()
            .HasOne(ce => ce.Contacto)
            .WithMany(c => c.ContactoEtiquetas)
            .HasForeignKey(ce => ce.IdContacto)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ContactoEtiqueta>()
            .HasOne(ce => ce.Etiqueta)
            .WithMany(e => e.ContactoEtiquetas)
            .HasForeignKey(ce => ce.IdEtiqueta)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}