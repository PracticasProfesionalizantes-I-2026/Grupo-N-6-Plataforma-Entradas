using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("Eventos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Descripcion).HasMaxLength(2000);
        builder.Property(e => e.FechaInicio).IsRequired();
        builder.Property(e => e.FechaFin).IsRequired();
        builder.Property(e => e.Lugar).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Direccion).IsRequired().HasMaxLength(300);
        
        builder.Property(e => e.Estado).IsRequired().HasConversion<int>().HasDefaultValue(EstadoEvento.Borrador);
        builder.HasIndex(e => e.Estado);
        
        builder.Property(e => e.UsuarioCreadorId).IsRequired();
        builder.Property(e => e.FechaCreacion).IsRequired();
        builder.Property(e => e.EsBorrado).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(e => e.UsuarioCreador)
            .WithMany(u => u.EventosCreados)
            .HasForeignKey(e => e.UsuarioCreadorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Sectores)
            .WithOne(s => s.Evento)
            .HasForeignKey(s => s.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Compras)
            .WithOne(c => c.Evento)
            .HasForeignKey(c => c.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.HistorialEstados)
            .WithOne(h => h.Evento)
            .HasForeignKey(h => h.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(e => !e.EsBorrado);
    }
}