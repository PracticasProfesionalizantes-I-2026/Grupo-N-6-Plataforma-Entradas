using EntradApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class ReservaTemporalConfiguration : IEntityTypeConfiguration<ReservaTemporal>
{
    public void Configure(EntityTypeBuilder<ReservaTemporal> builder)
    {
        builder.ToTable("ReservasTemporales");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        
        builder.Property(r => r.CompraId).IsRequired();
        builder.HasIndex(r => r.CompraId).IsUnique();
        
        builder.Property(r => r.SectorId).IsRequired();
        builder.HasIndex(r => r.SectorId);
        
        builder.Property(r => r.Cantidad).IsRequired();
        builder.Property(r => r.FechaCreacion).IsRequired();
        builder.Property(r => r.ExpiraEn).IsRequired();
        builder.Property(r => r.Expirada).IsRequired().HasDefaultValue(false);
        
        builder.HasOne(r => r.Compra)
            .WithMany(c => c.ReservasTemporales)
            .HasForeignKey(r => r.CompraId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(r => r.Sector)
            .WithMany()
            .HasForeignKey(r => r.SectorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(r => r.ExpiraEn);
    }
}