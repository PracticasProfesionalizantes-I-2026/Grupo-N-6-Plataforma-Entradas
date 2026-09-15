using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntradApp.DataAccess.Configurations;

public class EventoEstadoHistorialConfiguration : IEntityTypeConfiguration<EventoEstadoHistorial>
{
    public void Configure(EntityTypeBuilder<EventoEstadoHistorial> builder)
    {
        builder.ToTable("EventosEstadosHistorial");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();
        
        builder.Property(h => h.EventoId).IsRequired();
        builder.HasIndex(h => h.EventoId);
        
        builder.Property(h => h.EstadoAnterior).IsRequired().HasConversion<int>();
        builder.Property(h => h.EstadoNuevo).IsRequired().HasConversion<int>();
        builder.Property(h => h.SuperAdminId).IsRequired();
        
        builder.Property(h => h.Observaciones).HasMaxLength(500);
        builder.Property(h => h.FechaCambio).IsRequired();
        
        builder.HasOne(h => h.Evento)
            .WithMany(e => e.HistorialEstados)
            .HasForeignKey(h => h.EventoId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(h => h.SuperAdmin)
            .WithMany(u => u.HistorialEstados)
            .HasForeignKey(h => h.SuperAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}