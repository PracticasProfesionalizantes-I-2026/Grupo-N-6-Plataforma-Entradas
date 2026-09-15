using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess;

public static class DbInitializer
{
    public static async Task InitializeAsync(EntradAppDbContext context)
    {
        await context.Database.MigrateAsync();
        
        // Verificar si ya hay datos
        if (await context.Usuarios.AnyAsync()) return;

        // Super Admin
        var superAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var superAdmin = new Usuario
        {
            Id = superAdminId,
            Email = "admin@entradapp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Nombre = "Super",
            Apellido = "Admin",
            Dni = "00000000",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Rol = RolUsuario.SuperAdmin,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            EsBorrado = false
        };
        context.Usuarios.Add(superAdmin);

        // Usuario de prueba
        var usuarioPruebaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var usuarioPrueba = new Usuario
        {
            Id = usuarioPruebaId,
            Email = "usuario@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Usuario123!"),
            Nombre = "Usuario",
            Apellido = "Prueba",
            Dni = "12345678",
            FechaNacimiento = new DateTime(1995, 5, 15),
            Rol = RolUsuario.Usuario,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            EsBorrado = false
        };
        context.Usuarios.Add(usuarioPrueba);

        await context.SaveChangesAsync();
    }
}