using EntradApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique().HasFilter("[EsBorrado] = 0");
        
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Dni).IsRequired().HasMaxLength(20);
        builder.HasIndex(u => u.Dni).IsUnique().HasFilter("[EsBorrado] = 0");
        
        builder.Property(u => u.FechaNacimiento).IsRequired();
        builder.Property(u => u.Rol).IsRequired().HasConversion<int>();
        builder.Property(u => u.Activo).IsRequired();
        builder.Property(u => u.FechaCreacion).IsRequired();
        builder.Property(u => u.EsBorrado).IsRequired().HasDefaultValue(false);
        
        builder.HasMany(u => u.EventosCreados)
            .WithOne(e => e.UsuarioCreador)
            .HasForeignKey(e => e.UsuarioCreadorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.Compras)
            .WithOne(c => c.Usuario)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.HistorialEstados)
            .WithOne(h => h.SuperAdmin)
            .HasForeignKey(h => h.SuperAdminId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(u => !u.EsBorrado);
    }
}