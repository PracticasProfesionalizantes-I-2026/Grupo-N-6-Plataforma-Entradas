using EntradApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> builder)
    {
        builder.ToTable("Devoluciones");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        
        builder.Property(d => d.EntradaId).IsRequired();
        builder.HasIndex(d => d.EntradaId).IsUnique();
        
        builder.Property(d => d.MontoReembolsado).IsRequired().HasPrecision(18, 2);
        builder.Property(d => d.FechaSolicitud).IsRequired();
        builder.Property(d => d.Estado).IsRequired().HasMaxLength(50);
        builder.Property(d => d.EsBorrado).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(d => d.Entrada)
            .WithMany(e => e.Devoluciones)
            .HasForeignKey(d => d.EntradaId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(d => !d.EsBorrado);
    }
}