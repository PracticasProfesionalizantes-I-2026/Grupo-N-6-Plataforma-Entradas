using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class EntradaConfiguration : IEntityTypeConfiguration<Entrada>
{
    public void Configure(EntityTypeBuilder<Entrada> builder)
    {
        builder.ToTable("Entradas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        
        builder.Property(e => e.CompraId).IsRequired();
        builder.HasIndex(e => e.CompraId);
        
        builder.Property(e => e.SectorId).IsRequired();
        builder.HasIndex(e => e.SectorId);
        
        builder.Property(e => e.Dni).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => new { e.Dni, e.SectorId }).IsUnique().HasFilter("[EsBorrado] = 0");
        
        builder.Property(e => e.CodigoUnico).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.CodigoUnico).IsUnique().HasFilter("[EsBorrado] = 0");
        
        builder.Property(e => e.Estado).IsRequired().HasConversion<int>().HasDefaultValue(EstadoEntrada.Activa);
        builder.HasIndex(e => e.Estado);
        
        builder.Property(e => e.FechaCreacion).IsRequired();
        builder.Property(e => e.EsBorrado).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(e => e.Compra)
            .WithMany(c => c.Entradas)
            .HasForeignKey(e => e.CompraId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Sector)
            .WithMany(s => s.Entradas)
            .HasForeignKey(e => e.SectorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Devoluciones)
            .WithOne(d => d.Entrada)
            .HasForeignKey(d => d.EntradaId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(e => !e.EsBorrado);
    }
}