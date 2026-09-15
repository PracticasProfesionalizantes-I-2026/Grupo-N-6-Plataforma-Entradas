using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("Compras");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        
        builder.Property(c => c.UsuarioId).IsRequired();
        builder.HasIndex(c => c.UsuarioId);
        builder.Property(c => c.EventoId).IsRequired();
        builder.HasIndex(c => c.EventoId);
        builder.Property(c => c.SectorId).IsRequired();
        builder.HasIndex(c => c.SectorId);
        
        builder.Property(c => c.Cantidad).IsRequired();
        builder.Property(c => c.Total).IsRequired().HasPrecision(18, 2);
        
        builder.Property(c => c.Estado).IsRequired().HasConversion<int>().HasDefaultValue(EstadoCompra.PendientePago);
        builder.HasIndex(c => c.Estado);
        
        builder.Property(c => c.FechaCreacion).IsRequired();
        builder.Property(c => c.EsBorrado).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Compras)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(c => c.Evento)
            .WithMany(e => e.Compras)
            .HasForeignKey(c => c.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(c => c.Sector)
            .WithMany()
            .HasForeignKey(c => c.SectorId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(c => c.Entradas)
            .WithOne(e => e.Compra)
            .HasForeignKey(e => e.CompraId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(c => c.ReservasTemporales)
            .WithOne(r => r.Compra)
            .HasForeignKey(r => r.CompraId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(c => !c.EsBorrado);
    }
}