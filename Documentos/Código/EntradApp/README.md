# EntradApp - Plataforma de Venta de Entradas

Sistema de ticketing para eventos con arquitectura N-Tier en .NET 10, siguiendo Clean Architecture y Domain-Driven Design.

## Arquitectura

```
Controller -> Service -> Repository -> DbContext
```

## Inicio Rapido

### Prerrequisitos
- .NET 10 SDK
- SQLite (incluido en .NET)

### Ejecucion

```bash
cd Documentos/Codigo/EntradApp
dotnet restore
dotnet run --project API
```

La API estara disponible en:
- API: http://localhost:5000
- Scalar (OpenAPI): http://localhost:5000/scalar/v1

### Usuarios de prueba

| Rol | Email | Password |
|-----|-------|----------|
| SuperAdmin | admin@entradapp.com | Admin123! |
| Usuario | usuario@test.com | Usuario123! |

## Endpoints Principales

### Autenticacion
- POST /api/v1/auth/register - Registrar usuario
- POST /api/v1/auth/login - Login (retorna JWT)

### Eventos (Publico)
- GET /api/v1/eventos - Listar eventos aprobados
- GET /api/v1/eventos/{id} - Detalle de evento aprobado
- GET /api/v1/eventos/{id}/sectores - Sectores con precios actuales

### Eventos (Usuario Registrado)
- POST /api/v1/eventos - Proponer evento
- GET /api/v1/eventos/mis-eventos - Mis eventos
- PUT /api/v1/eventos/{id} - Modificar evento propio
- DELETE /api/v1/eventos/{id} - Borrado logico

### Sectores (Usuario - Creador)
- POST /api/v1/eventos/{eventoId}/sectores - Agregar sector
- PUT /api/v1/sectores/{id} - Modificar sector
- DELETE /api/v1/sectores/{id} - Eliminar sector

### Compras
- POST /api/v1/compras - Iniciar compra
- PATCH /api/v1/compras/{id}/confirmar-pago - Confirmar pago efectivo
- PATCH /api/v1/compras/{id}/cancelar - Cancelar compra pendiente
- GET /api/v1/compras - Mis compras

### Entradas
- GET /api/v1/entradas - Mis entradas activas
- GET /api/v1/entradas/utilizadas - Historial usadas
- GET /api/v1/entradas/devueltas - Historial devoluciones
- GET /api/v1/entradas/{id}/comprobante - PDF con codigo unico

### Devoluciones
- POST /api/v1/devoluciones/{entradaId} - Solicitar devolucion (80% + stock++)

### Admin (SuperAdmin)
- GET /api/v1/admin/eventos/pendientes - Pendientes + Historial
- PATCH /api/v1/admin/eventos/{id}/estado - Aprobar/Rechazar

### Internos (Job / SuperAdmin)
- POST /api/v1/interno/precios/evaluar - Evaluar incremento 20% > 80%
- POST /api/v1/interno/validar/disponibilidad - Validar stock
- POST /api/v1/interno/validar/limite - Validar limite 4/user/evento
- POST /api/v1/interno/validar/dnis - Validar DNIs duplicados

### Reportes (Propietario)
- GET /api/v1/eventos/{id}/transacciones - Registro transacciones
- GET /api/v1/eventos/{id}/estadisticas - Ventas, ocupacion, ingresos
- GET /api/v1/eventos/{id}/ocupacion - % ocupacion por sector

## Autenticacion

Usa JWT Bearer Token:

```bash
# 1. Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"usuario@test.com","password":"Usuario123!"}'

# 2. Usar token
curl -H "Authorization: Bearer <TOKEN>" http://localhost:5000/api/v1/eventos/mis-eventos
```

## Testing

```bash
# Unit tests
dotnet test Tests/UnitTests

# Integration tests
dotnet test Tests/IntegrationTests

# Todos
dotnet test EntradApp.sln
```

## Coleccion Bruno

Importar `bruno/EntradApp.bru` en Bruno.

## Configuracion (appsettings.json)

```json
{
  "ConnectionStrings": { "DefaultConnection": "Data Source=entradapp.db" },
  "Jwt": { "Key": "...", "Issuer": "EntradApp", "Audience": "EntradAppUsers" },
  "PrecioDinamico": { "UmbralOcupacion": 80, "PorcentajeIncremento": 20, "IntervaloMinutos": 5 },
  "ReservaTemporal": { "DuracionMinutos": 10, "LimpiezaIntervaloMinutos": 1 }
}
```

## Reglas de Negocio Clave

| Regla | Descripcion |
|-------|-------------|
| RF-03 | Max 4 entradas por usuario por evento (acumulado) |
| RF-05 | Reserva temporal 10 min (job limpieza cada 1 min) |
| RF-06/07 | DNI obligatorio por entrada + unico por evento |
| RF-09 | Devolucion 80% + stock++ (eventos con entradas activas) |
| RF-10 | Precio +20% si ocupacion > 80% (job cada 5 min, idempotente) |
| Estados Evento | Borrador -> Pendiente -> Aprobado/Rechazado -> Cancelado/Finalizado |
| Rechazado | NO vuelve a Pendiente sin nueva revision completa |
| Soft Delete | Todas las entidades principales |

## Estructura de Carpetas

```
EntradApp/
├── EntradApp.sln
├── Shared/           # DTOs, Exceptions, Enums, Common
├── DataAccess/       # Entities, Repositories, DbContext, Migrations
├── BusinessLogic/    # Interfaces + Services
├── API/              # Controllers, Filters, Middleware, Jobs
├── Tests/
│   ├── UnitTests/    # xUnit + Moq
│   └── IntegrationTests/ # WebApplicationFactory
└── bruno/            # Coleccion Bruno
```