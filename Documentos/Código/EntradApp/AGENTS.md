# AGENTS.md - Contexto Operativo para Agentes IA

## Proposito del Proyecto

**EntradApp** - Plataforma de venta de entradas para eventos (Ticketing System)
- Arquitectura: N-Tier (.NET 10) con Clean Architecture
- Flujo obligatorio: Controller -> Service -> Repository -> DbContext
- Base de datos: SQLite (desarrollo) / SQL Server/PostgreSQL (produccion)
- Autenticacion: JWT Bearer (roles: Usuario, SuperAdmin)

## Comandos CLI

```bash
# Restaurar dependencias
dotnet restore

# Compilar todo
dotnet build EntradApp.sln

# Ejecutar API
dotnet run --project API

# Ejecutar tests unitarios
dotnet test Tests/UnitTests

# Ejecutar tests de integracion
dotnet test Tests/IntegrationTests

# Ejecutar todos los tests
dotnet test EntradApp.sln

# Crear migracion (si hay cambios en entidades)
dotnet ef migrations add NombreMigracion --project DataAccess --startup-project API

# Aplicar migraciones
dotnet ef database update --project DataAccess --startup-project API
```

## Convencion de Capas N-Tier

| Capa | Responsabilidad | No debe |
|------|-----------------|---------|
| **API (Controllers)** | Validacion DTO, ModelState, Auth, HTTP codes | Logica de negocio, EF Core directo |
| **BusinessLogic (Services)** | Reglas de dominio, orquestacion, validaciones | EF Core, SQL, HTTP |
| **DataAccess (Repositories)** | CRUD, queries EF Core, transacciones | Logica de negocio, DTOs |
| **Shared** | DTOs, Exceptions, Enums, Common types | Logica, EF Core |

**Regla de oro**: La logica de negocio vive SOLO en Services.

## Estructura de Archivos

```
EntradApp/
├── Shared/           # DTOs, Exceptions, Enums, Common
├── DataAccess/       # Entities, Repositories, DbContext, Migrations
├── BusinessLogic/    # Interfaces + Services
├── API/              # Controllers, Filters, Middleware, Jobs
├── Tests/
│   ├── UnitTests/    # xUnit + Moq (services aislados)
│   └── IntegrationTests/ # WebApplicationFactory + SQLite in-memory
└── bruno/            # Coleccion Bruno
```

## Reglas de Negocio Implementadas

| Codigo | Regla | Implementacion |
|--------|-------|----------------|
| RF-01 | Crear evento con estado PendienteAprobacion | EventoService.CrearAsync |
| RF-02 | Consultar disponibilidad tiempo real | ValidacionCompraService.ValidarDisponibilidadAsync |
| RF-03 | Max 4 entradas/user/evento (acumulado) | ValidacionCompraService.ValidarLimiteMaximoAsync |
| RF-04 | Rechazar si supera cupo | ValidacionCompraService.ValidarDisponibilidadAsync |
| RF-05 | Reserva temporal 10 min | ReservaTemporalService + Job limpieza 1 min |
| RF-06 | DNI obligatorio por entrada | CompraCreateDTO.Dnis (required) |
| RF-07 | DNI unico por evento | ValidacionCompraService.ValidarDnisDuplicadosAsync |
| RF-08 | Registrar transaccion | CompraService.CrearCompraAsync |
| RF-09 | Devolucion 80% + stock++ | DevolucionService.SolicitarDevolucionAsync |
| RF-10 | Precio +20% si ocupacion > 80% | PrecioDinamicoService.EvaluarYAplicarIncrementoAsync (job 5 min) |
| RF-11 | Codigo unico alfanumerico por entrada | Generado en CompraService |
| RF-12 | Impedir reuso entrada | EntradaService.ValidarEntradaAsync (estado Utilizada) |
| RF-13 | Registrar fecha validacion | Entrada.FechaValidacion |
| RF-14 | Registro transacciones | ReportesController.GetTransacciones |
| RF-15 | Reportes historicos | ReportesController.GetEstadisticas/Ocupacion |
| RF-16 | Admin ver pendientes | AdminController.GetEventosPendientes |
| RF-17 | Admin aprobar | AdminService.AprobarEventoAsync |
| RF-18 | Admin rechazar con motivo | AdminService.RechazarEventoAsync |
| RF-19 | Solo eventos Aprobados visibles | EventosController.GetEventos (filtro Estado=Aprobado) |

## Estados de Evento (Maquina de Estados)

```
Borrador -> PendienteAprobacion -> Aprobado -> Cancelado/Finalizado
                      -> Rechazado (NO vuelve a Pendiente)
Cualquiera -> Borrado (soft delete)
```

## Excepciones Tipadas -> HTTP Codes

| Excepcion | HTTP Code |
|-----------|-----------|
| NotFoundException | 404 |
| ValidationException | 400 |
| ConflictException (y derivadas) | 409 |
| UnauthorizedException | 401 |
| ForbiddenException | 403 |
| ConcurrencyException | 409 |

## DTOs Pattern

Cada entidad tiene:
- `<Entidad>CreateDTO` - Para crear (validaciones [Required], [Range], etc.)
- `<Entidad>UpdateDTO` - Para actualizar (propiedades opcionales)
- `<Entidad>ResponseDTO` - Para respuestas (sin datos sensibles)

Mapeo manual en Services: `private static ResponseDTO MapToResponseDTO(Entity e)`

## Background Jobs (IHostedService)

| Job | Intervalo | Descripcion |
|-----|-----------|-------------|
| PrecioDinamicoJob | 5 min | Evalua incremento 20% si ocupacion > 80% |
| ReservaTemporalCleanupJob | 1 min | Libera reservas expiradas, repone stock |
| EventoFinalizadoJob | 1 hora | Finaliza eventos con FechaFin < ahora |

## Testing

### Unit Tests (xUnit + Moq)
- Ubicacion: `Tests/UnitTests/Services/`
- Prueban Services aislados (mock repositories)
- Un test por flujo feliz + uno por cada excepcion

### Integration Tests (WebApplicationFactory)
- Ubicacion: `Tests/IntegrationTests/Controllers/`
- SQLite en memoria
- Prueban flujo HTTP completo: Controller -> Service -> Repository -> DbContext

## Convenciones de Codigo

- **C# 12 / .NET 10**: records, pattern matching, primary constructors
- **Nullable enabled**: `<Nullable>enable</Nullable>`
- **Async suffix**: Todos los metodos async terminan en `Async`
- **CancellationToken**: Siempre pasado a repositorios
- **Result<T>**: Para operaciones que pueden fallar sin exception
- **PagedResult<T>**: Para listados paginados

## Generar Codigo Unico Entrada

```csharp
private static string GenerarCodigoUnico() => $"EVT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
// Ej: EVT-A1B2C3D4
```

## PDF Comprobante (QuestPDF)

```csharp
QuestPDF.Settings.License = LicenseType.Community;
var pdf = Document.Create(container => { ... }).GeneratePdf();
```

## BCrypt para Passwords

```csharp
var hash = BCrypt.Net.BCrypt.HashPassword(password);
var valid = BCrypt.Net.BCrypt.Verify(password, hash);
```

## JWT Claims

```csharp
new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
    new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
    new Claim("nombre", usuario.Nombre)
}
```

## Patrones de Respuesta

### Exitoso
```json
{ "id": "guid", "nombre": "...", "estado": "Aprobado" }
```

### Error (GlobalExceptionFilter)
```json
{ "error": "Mensaje descriptivo" }
```

### Paginado
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Entidades Principales

| Entidad | Clave | Relaciones Clave |
|---------|-------|------------------|
| Usuario | Guid | 1:N Eventos, 1:N Compras |
| Evento | Guid | N:1 Usuario, 1:N Sectores, 1:N HistorialEstados |
| Sector | Guid | N:1 Evento, 1:N Entradas |
| Compra | Guid | N:1 Usuario/Evento/Sector, 1:N Entradas |
| Entrada | Guid | N:1 Compra/Sector, CodigoUnico (unique) |
| Devolucion | Guid | N:1 Entrada |
| EventoEstadoHistorial | Guid | N:1 Evento, N:1 SuperAdmin |
| ReservaTemporal | Guid | N:1 Compra, expiracion 10 min |

## Soft Delete

Todas las entidades principales tienen:
```csharp
public bool EsBorrado { get; set; } = false;
```

Query filter global en DbContext:
```csharp
builder.HasQueryFilter(e => !e.EsBorrado);
```

## DeleteBehavior.Restrict

Todas las FK hijas usan `OnDelete(DeleteBehavior.Restrict)` para evitar cascade delete accidental.

## AsNoTracking() en Consultas de Lectura

```csharp
var eventos = await _context.Eventos
    .AsNoTracking()
    .Where(e => e.Estado == EstadoEvento.Aprobado)
    .ToListAsync();
```

## Seed Data (DbInitializer)

```csharp
// SuperAdmin: admin@entradapp.com / Admin123!
// Usuario prueba: usuario@test.com / Usuario123!
```

## Bruno Collection

Importar `bruno/EntradApp.bru` con variables:
- `token` - JWT usuario normal
- `adminToken` - JWT SuperAdmin
- `eventoId`, `sectorId`, `compraId`, `entradaId` - IDs dinamicos