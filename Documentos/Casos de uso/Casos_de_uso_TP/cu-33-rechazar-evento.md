# Caso de Uso: Rechazar evento

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-33 |
| **Nombre** | Rechazar evento |
| **Actor Principal** | Administrador |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Admin → rechazar evento no válido; Creador → conocer motivo de rechazo; Sistema → mantener evento oculto |
| **Disparador (Trigger)** | El administrador selecciona "Rechazar" en un evento pendiente de revisión, ingresando motivo |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al administrador rechazar un evento con estado "Pendiente de aprobación", cambiando su estado a "Rechazado" y notificando al creador con el motivo.

### 2. PRECONDICIONES
1. El administrador debe estar autenticado (Token JWT válido con rol "Admin").
2. El evento debe existir y estar en estado "Pendiente de aprobación".

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `PUT /api/admin/eventos/{id}/rechazar` con JSON (motivoRechazo) y header Authorization.
2. La **Capa de Presentación** valida el JWT, rol "Admin", ID (GUID), y que motivoRechazo no esté vacío (máx. 500 chars).
3. La **Capa de Negocio** verifica que el evento exista y esté en "Pendiente de aprobación".
4. La **Capa de Negocio** cambia estado a "Rechazado", registra adminId, fechaRechazo, motivoRechazo.
5. El Sistema notifica al creador del evento (email/in-app): "Tu evento fue rechazado. Motivo: {motivo}".
6. El Sistema devuelve **200 OK** con el evento actualizado (estado "Rechazado").

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **1b. Sin rol Admin (HTTP 403 Forbidden):**
  1. Si en el Paso 2 el token es válido pero el usuario no tiene rol "Admin".
  2. El Sistema rechaza por autorización.
  3. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. Motivo faltante o inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 motivoRechazo está vacío, solo espacios, o supera 500 chars.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no está pendiente (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento existe pero no está en "Pendiente de aprobación" (ej. ya "Aprobado", "Rechazado", "Cancelado").
  2. El Sistema devuelve **409 Conflict** con mensaje "El evento no está en estado revisable". Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la persistencia o notificación.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Rechazo con solicitud de correcciones (estado "Devuelto a creador" vs "Rechazado definitivo").
2. Rechazo masivo (bulk) - futuro.

### 6. POSTCONDICIONES
1. Estado del evento cambiado a "Rechazado" en BD.
2. Evento permanece oculto para usuarios visitantes; no habilita venta de entradas.
3. Creador notificado del rechazo con motivo.
4. Registro de auditoría: adminId, fecha, acción "Rechazar", motivo.
5. Según regla de negocio: "Una vez rechazado un evento, no podrá publicarse sin una nueva revisión administrativa".

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Evento rechazado exitosamente. |
| `400` | Bad Request | ID inválido, motivo faltante/inválido. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Usuario sin rol Admin. |
| `404` | Not Found | Evento inexistente. |
| `409` | Conflict | Evento no está pendiente. |
| `500` | Internal Server Error | Error técnico en persistencia/notificación. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, rol Admin, formato ID, motivoRechazo obligatorio y longitud.
- **Verificación (Negocio, → 403/404/409):** existencia evento, estado "Pendiente".

### Matriz de trazabilidad CU-33 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `RechazarEvento_WithValidMotivo_UpdatesStateAndNotifies` | `PutRechazarEvento_WithValidData_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `PutRechazarEvento_WithInvalidToken_Returns401Unauthorized` |
| 1b. Sin rol Admin | `403 Forbidden` | — | `PutRechazarEvento_WithUserToken_Returns403Forbidden` |
| 2a. ID inválido | `400 Bad Request` | — | `PutRechazarEvento_WithInvalidId_Returns400BadRequest` |
| 2b. Motivo inválido | `400 Bad Request` | — | `PutRechazarEvento_WithInvalidMotivo_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `RechazarEvento_WhenNotExists_ThrowsNotFoundException` | `PutRechazarEvento_WhenNotExists_Returns404NotFound` |
| 3b. No pendiente | `409 Conflict` | `RechazarEvento_WhenNotPending_ThrowsConflictException` | `PutRechazarEvento_WhenNotPending_Returns409Conflict` |
| 4a. Error interno | `500 Internal Server Error` | `RechazarEvento_WhenRepositoryFails_ThrowsException` | `PutRechazarEvento_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
