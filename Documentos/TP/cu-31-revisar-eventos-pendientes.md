# Caso de Uso: Revisar eventos pendientes

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-31 |
| **Nombre** | Revisar eventos pendientes |
| **Actor Principal** | Administrador |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Admin → revisar eventos antes de publicar; Creador → que su evento sea evaluado; Sistema → mantener calidad de eventos publicados |
| **Disparador (Trigger)** | El administrador accede al panel de "Eventos pendientes de revisión" |
| **Prioridad / Frecuencia** | Alta; uso frecuente (diario) |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al administrador listar y acceder al detalle de todos los eventos con estado "Pendiente de aprobación" para su revisión.

### 2. PRECONDICIONES
1. El administrador debe estar autenticado (Token JWT válido con rol "Admin").
2. Debe haber eventos en estado "Pendiente de aprobación".

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/admin/eventos/pendientes` con header Authorization y parámetros de paginación/filtro.
2. La **Capa de Presentación** valida el JWT y que el usuario tenga rol "Admin".
3. La **Capa de Negocio** filtra eventos con estado "Pendiente de aprobación" y aplica paginación.
4. El Sistema devuelve **200 OK** con lista paginada (incluye nombre, creador, fecha evento, fecha solicitud, sectores).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **1b. Sin rol Admin (HTTP 403 Forbidden):**
  1. Si en el Paso 2 el token es válido pero el usuario no tiene rol "Admin".
  2. El Sistema (Capa de Presentación/Negocio) rechaza por autorización.
  3. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **2a. Parámetros inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 1 parámetros de paginación/filtro tienen formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Sin eventos pendientes (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 no hay eventos en estado "Pendiente".
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre error consultando BD.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Filtros: por fecha rango, por creador, por categoría.
2. Ordenamiento: por fecha solicitud (más antiguo primero), por fecha evento.

### 6. POSTCONDICIONES
1. Se muestra al administrador el listado de eventos pendientes de revisión.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de eventos pendientes retornada (puede ser vacía). |
| `400` | Bad Request | Parámetros de paginación/filtro inválidos. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Usuario autenticado pero sin rol Admin. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato parámetros.
- **Verificación (Negocio, → 403):** rol "Admin" en claims del token. Filtro por estado "Pendiente de aprobación".

### Matriz de trazabilidad CU-31 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `RevisarEventosPendientes_WithAdmin_ReturnsPagedList` | `GetEventosPendientes_WithAdminToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetEventosPendientes_WithInvalidToken_Returns401Unauthorized` |
| 1b. Sin rol Admin | `403 Forbidden` | — | `GetEventosPendientes_WithUserToken_Returns403Forbidden` |
| 2a. Parámetros inválidos | `400 Bad Request` | — | `GetEventosPendientes_WithInvalidParams_Returns400BadRequest` |
| 3a. Sin eventos | `200 OK` | `RevisarEventosPendientes_WhenNone_ReturnsEmptyList` | `GetEventosPendientes_WhenEmpty_Returns200EmptyList` |
| 4a. Error interno | `500 Internal Server Error` | `RevisarEventosPendientes_WhenRepositoryFails_ThrowsException` | `GetEventosPendientes_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
