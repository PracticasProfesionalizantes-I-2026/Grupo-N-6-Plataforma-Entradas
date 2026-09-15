using EntradApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        
        builder.Property(s => s.EventoId).IsRequired();
        builder.HasIndex(s => s.EventoId);
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Capacidad).IsRequired();
        builder.Property(s => s.PrecioBase).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.PrecioActual).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.EntradasVendidas).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.EntradasReservadas).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.EsBorrado).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(s => s.Evento)
            .WithMany(e => e.Sectores)
            .HasForeignKey(s => s.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(s => s.Entradas)
            .WithOne(e => e.Sector)
            .HasForeignKey(e => e.SectorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(s => !s.EsBorrado);
    }
}