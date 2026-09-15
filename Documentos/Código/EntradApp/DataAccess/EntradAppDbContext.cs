using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess;

public class EntradAppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Sector> Sectores => Set<Sector>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<Entrada> Entradas => Set<Entrada>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<EventoEstadoHistorial> EventosEstadosHistorial => Set<EventoEstadoHistorial>();
    public DbSet<ReservaTemporal> ReservasTemporales => Set<ReservaTemporal>();

    public EntradAppDbContext(DbContextOptions<EntradAppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new EventoConfiguration());
        modelBuilder.ApplyConfiguration(new SectorConfiguration());
        modelBuilder.ApplyConfiguration(new CompraConfiguration());
        modelBuilder.ApplyConfiguration(new EntradaConfiguration());
        modelBuilder.ApplyConfiguration(new DevolucionConfiguration());
        modelBuilder.ApplyConfiguration(new EventoEstadoHistorialConfiguration());
        modelBuilder.ApplyConfiguration(new ReservaTemporalConfiguration());

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Super Admin por defecto
        var superAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"); // Cambiar en producción

        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = superAdminId,
            Email = "admin@entradapp.com",
            PasswordHash = passwordHash,
            Nombre = "Super",
            Apellido = "Admin",
            Dni = "00000000",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Rol = EntradApp.Shared.Enums.RolUsuario.SuperAdmin,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            EsBorrado = false
        });

        // Usuario de prueba
        var usuarioPruebaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var passwordHashUsuario = BCrypt.Net.BCrypt.HashPassword("Usuario123!");

        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = usuarioPruebaId,
            Email = "usuario@test.com",
            PasswordHash = passwordHashUsuario,
            Nombre = "Usuario",
            Apellido = "Prueba",
            Dni = "12345678",
            FechaNacimiento = new DateTime(1995, 5, 15),
            Rol = EntradApp.Shared.Enums.RolUsuario.Usuario,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            EsBorrado = false
        });
    }
}