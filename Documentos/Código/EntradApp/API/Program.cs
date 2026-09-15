using EntradApp.API.Filters;
using EntradApp.API.Middleware;
using EntradApp.BusinessLogic.Interfaces;
using EntradApp.BusinessLogic.Services;
using EntradApp.DataAccess;
using EntradApp.DataAccess.Repositories.Implementations;
using EntradApp.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// DbContext
builder.Services.AddDbContext<EntradAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=entradapp.db"));

// Repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<ISectorRepository, SectorRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IEntradaRepository, EntradaRepository>();
builder.Services.AddScoped<IDevolucionRepository, DevolucionRepository>();
builder.Services.AddScoped<IEventoEstadoHistorialRepository, EventoEstadoHistorialRepository>();
builder.Services.AddScoped<IReservaTemporalRepository, ReservaTemporalRepository>();

// Services
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<ISectorService, SectorService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IEntradaService, EntradaService>();
builder.Services.AddScoped<IDevolucionService, DevolucionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPrecioDinamicoService, PrecioDinamicoService>();
builder.Services.AddScoped<IReservaTemporalService, ReservaTemporalService>();
builder.Services.AddScoped<IValidacionCompraService, ValidacionCompraService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDevelopmentOnlyChangeInProduction12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EntradApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EntradAppUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("Usuario", policy => policy.RequireRole("Usuario"));
});

// Background Services
builder.Services.AddHostedService<PrecioDinamicoJob>();
builder.Services.AddHostedService<ReservaTemporalCleanupJob>();
builder.Services.AddHostedService<EventoFinalizadoJob>();

var app = builder.Build();

// Initialize DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntradAppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseScalar();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();