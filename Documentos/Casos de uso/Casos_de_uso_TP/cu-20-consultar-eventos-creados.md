# Caso de Uso: Consultar eventos creados

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-20 |
| **Nombre** | Consultar eventos creados |
| **Actor Principal** | Usuario Registrado (creador) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Creador → ver listado de sus eventos con estados; Sistema → filtrar por propietario |
| **Disparador (Trigger)** | El usuario accede a "Mis eventos" en su panel |
| **Prioridad / Frecuencia** | Alta; uso frecuente |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado consultar el listado paginado de todos los eventos que ha creado, con sus respectivos estados (Borrador, Pendiente, Aprobado, Rechazado, Cancelado, Finalizado).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El usuario solo ve sus propios eventos.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/eventos/mis-eventos` con header Authorization y parámetros de paginación/filtro opcionales (estado, página, tamaño).
2. La **Capa de Presentación** valida el JWT y parámetros.
3. La **Capa de Negocio** filtra eventos por propietario (usuario autenticado) y aplica filtros/paginación.
4. El Sistema devuelve **200 OK** con lista paginada de eventos (incluye nombre, fecha, estado, ventas, ocupación).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. Parámetros inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 2 parámetros de paginación o filtro tienen formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Sin eventos creados (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 el usuario no ha creado eventos.
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre un error al consultar la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Filtro por estado: "Pendiente", "Aprobado", "Rechazado", etc.
2. Ordenamiento: por fecha creación, por fecha evento, por estado.
3. Incluir métricas resumidas por evento (entradas vendidas, recaudación).

### 6. POSTCONDICIONES
1. Se muestra al usuario su listado de eventos creados con estados y métricas.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de eventos creados retornada (puede ser vacía). |
| `400` | Bad Request | Parámetros de paginación/filtro inválidos. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato parámetros paginación/filtro.
- **Verificación (Negocio):** filtro por propietario (usuario autenticado).

### Matriz de trazabilidad CU-20 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarEventosCreados_WithValidUser_ReturnsPagedList` | `GetMisEventos_WithValidToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetMisEventos_WithInvalidToken_Returns401Unauthorized` |
| 2a. Parámetros inválidos | `400 Bad Request` | — | `GetMisEventos_WithInvalidParams_Returns400BadRequest` |
| 3a. Sin eventos | `200 OK` | `ConsultarEventosCreados_WhenNone_ReturnsEmptyList` | `GetMisEventos_WhenEmpty_Returns200EmptyList` |
| 4a. Error interno | `500 Internal Server Error` | `ConsultarEventosCreados_WhenRepositoryFails_ThrowsException` | `GetMisEventos_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.

