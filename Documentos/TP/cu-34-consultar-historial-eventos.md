# Caso de Uso: Consultar historial de eventos aprobados/rechazados

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-34 |
| **Nombre** | Consultar historial de eventos aprobados/rechazados |
| **Actor Principal** | Administrador |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Admin → auditar decisiones de revisión; Sistema → mantener trazabilidad |
| **Disparador (Trigger)** | El administrador accede a "Historial de revisiones" en el panel de administración |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al administrador consultar el historial de eventos que han sido revisados (aprobados o rechazados), con detalles de quién los revisó, cuándo, y el motivo en caso de rechazo.

### 2. PRECONDICIONES
1. El administrador debe estar autenticado (Token JWT válido con rol "Admin").

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/admin/eventos/historial` con header Authorization y parámetros de paginación/filtro (estado: Aprobado/Rechazado, fechaDesde, fechaHasta, adminId).
2. La **Capa de Presentación** valida el JWT, rol "Admin", y parámetros.
3. La **Capa de Negocio** consulta eventos con estado "Aprobado" o "Rechazado" y aplica filtros/paginación.
4. El Sistema devuelve **200 OK** con lista paginada (incluye evento, estado, adminRevisor, fechaRevision, motivoRechazo si aplica).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **1b. Sin rol Admin (HTTP 403 Forbidden):**
  1. Si en el Paso 2 el token es válido pero el usuario no tiene rol "Admin".
  2. El Sistema rechaza por autorización.
  3. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **2a. Parámetros inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 1 parámetros de paginación/filtro tienen formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Sin historial (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 no hay eventos revisados con los filtros aplicados.
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre error consultando BD.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Exportar historial a CSV/Excel (ver CU-25 patrón).
2. Filtro por creador del evento.

### 6. POSTCONDICIONES
1. Se muestra al administrador el historial de revisiones con filtros aplicados.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Historial de revisiones retornado (puede ser vacío). |
| `400` | Bad Request | Parámetros de paginación/filtro inválidos. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Usuario sin rol Admin. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, rol Admin, formato parámetros.
- **Verificación (Negocio, → 403):** rol "Admin". Filtro por estados "Aprobado"/"Rechazado".

### Matriz de trazabilidad CU-34 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarHistorialRevisiones_WithAdmin_ReturnsPagedList` | `GetHistorialRevisiones_WithAdminToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetHistorialRevisiones_WithInvalidToken_Returns401Unauthorized` |
| 1b. Sin rol Admin | `403 Forbidden` | — | `GetHistorialRevisiones_WithUserToken_Returns403Forbidden` |
| 2a. Parámetros inválidos | `400 Bad Request` | — | `GetHistorialRevisiones_WithInvalidParams_Returns400BadRequest` |
| 3a. Sin historial | `200 OK` | `ConsultarHistorialRevisiones_WhenNone_ReturnsEmptyList` | `GetHistorialRevisiones_WhenEmpty_Returns200EmptyList` |
| 4a. Error interno | `500 Internal Server Error` | `ConsultarHistorialRevisiones_WhenRepositoryFails_ThrowsException` | `GetHistorialRevisiones_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
