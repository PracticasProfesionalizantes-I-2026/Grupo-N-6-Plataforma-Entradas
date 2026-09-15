# Caso de Uso: Aprobar evento

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-32 |
| **Nombre** | Aprobar evento |
| **Actor Principal** | **Super Admin** |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Super Admin → aprobar evento válido; Creador → que su evento se publique; Usuario visitante → ver evento aprobado; Sistema → habilitar venta de entradas |
| **Disparador (Trigger)** | El **Super Admin** selecciona "Aprobar" en un evento pendiente de revisión |
| **Prioridad / Frecuencia** | Alta; uso frecuente |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al **Super Admin** aprobar un evento con estado "Pendiente de aprobación", cambiando su estado a "Aprobado" y habilitando su visibilidad pública y venta de entradas.

### 2. PRECONDICIONES
1. El **Super Admin** debe estar autenticado (Token JWT válido con rol "SuperAdmin").
2. El evento debe existir y estar en estado "Pendiente de aprobación".
3. El evento debe tener al menos un sector configurado.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El **Super Admin** envía una petición al endpoint `PUT /api/admin/eventos/{id}/aprobar` con header Authorization.
2. La **Capa de Presentación** valida el JWT, rol "SuperAdmin", y ID (GUID).
3. La **Capa de Negocio** verifica que el evento exista y esté en "Pendiente de aprobación".
4. La **Capa de Negocio** valida que el evento tenga sectores configurados.
5. La **Capa de Negocio** cambia estado a "Aprobado" y registra adminId, fechaAprobacion.
6. 
7. El Sistema devuelve **200 OK** con el evento actualizado (estado "Aprobado").

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **1b. Sin rol Admin (HTTP 403 Forbidden):**
  1. Si en el Paso 2 el token es válido pero el usuario no tiene rol "SuperAdmin".
  2. El Sistema rechaza por autorización.
  3. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no está pendiente (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento existe pero no está en "Pendiente de aprobación" (ej. ya "Aprobado", "Rechazado", "Cancelado").
  2. El Sistema devuelve **409 Conflict** con mensaje "El evento no está en estado revisable". Fin del caso de uso.

* **3c. Evento sin sectores (HTTP 409 Conflict):**
  1. Si en el Paso 4 el evento no tiene sectores configurados.
  2. El Sistema devuelve **409 Conflict** con mensaje "El evento debe tener al menos un sector para ser aprobado". Fin del caso de uso.

* **5a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5 falla la persistencia.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Aprobación con observaciones/condiciones (campo opcional en request).
2. Aprobación masiva (bulk) - futuro.

### 6. POSTCONDICIONES
1. Estado del evento cambiado a "Aprobado" en BD.
2. Evento visible para usuarios visitantes y disponible para venta (CU-10).
3. 
4. Registro de auditoría: **superAdminId**, fecha, acción "Aprobar".

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Evento aprobado exitosamente. |
| `400` | Bad Request | ID con formato inválido. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Usuario sin rol SuperAdmin. |
| `404` | Not Found | Evento inexistente. |
| `409` | Conflict | Evento no está pendiente o no tiene sectores. |
| `500` | Internal Server Error | Error técnico en persistencia. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, rol SuperAdmin en claims, formato ID (GUID).
- **Verificación (Negocio, → 403/404/409):** existencia evento, estado "Pendiente", sectores configurados.

### Matriz de trazabilidad CU-32 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `AprobarEvento_WithValidEvento_UpdatesState` | `PutAprobarEvento_WithValidId_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `PutAprobarEvento_WithInvalidToken_Returns401Unauthorized` |
| 1b. Sin rol Admin | `403 Forbidden` | — | `PutAprobarEvento_WithNonSuperAdminToken_Returns403Forbidden` |
| 2a. ID inválido | `400 Bad Request` | — | `PutAprobarEvento_WithInvalidId_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `AprobarEvento_WhenNotExists_ThrowsNotFoundException` | `PutAprobarEvento_WhenNotExists_Returns404NotFound` |
| 3b. No pendiente | `409 Conflict` | `AprobarEvento_WhenNotPending_ThrowsConflictException` | `PutAprobarEvento_WhenNotPending_Returns409Conflict` |
| 3c. Sin sectores | `409 Conflict` | `AprobarEvento_WhenNoSectores_ThrowsConflictException` | `PutAprobarEvento_WhenNoSectores_Returns409Conflict` |
| 5a. Error interno | `500 Internal Server Error` | `AprobarEvento_WhenRepositoryFails_ThrowsException` | `PutAprobarEvento_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.

