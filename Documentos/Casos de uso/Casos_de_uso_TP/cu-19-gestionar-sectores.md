# Caso de Uso: Gestionar sectores

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-19 |
| **Nombre** | Gestionar sectores |
| **Actor Principal** | Usuario Registrado (creador del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Creador → definir sectores, capacidades y precios; Sistema → validar consistencia; Super Admin → revisar si evento aprobado |
| **Disparador (Trigger)** | El usuario creador accede a "Gestionar sectores" de su evento |
| **Prioridad / Frecuencia** | Media; uso ocasional por evento |
| **Reglas de negocio relacionadas** | RN-03 (incremento dinámico 20% > 80% ocupación) |

---

### 1. BREVE DESCRIPCIÓN
Permite al creador de un evento agregar, modificar o eliminar sectores (zonas) del evento, definiendo capacidad y precio base de cada uno.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El evento debe existir y pertenecer al usuario autenticado.
3. El evento no debe estar en estado "Finalizado".

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200/201)
1. El Actor envía una petición al endpoint `POST /api/eventos/{id}/sectores` (crear), `PUT /api/sectores/{id}` (modificar), o `DELETE /api/sectores/{id}` (eliminar) con header Authorization y JSON según operación.
2. La **Capa de Presentación** valida el JWT, IDs (GUID) y formato JSON (nombre, capacidad, precioBase).
3. La **Capa de Negocio** verifica que el evento pertenezca al usuario y no esté finalizado.
4. La **Capa de Negocio** valida: capacidad > 0, precioBase >= 0, nombre único por evento.
5. La **Capa de Persistencia** crea/actualiza/elimina el sector.
6. El Sistema devuelve **201 Created** (crear) o **200 OK** (modificar/eliminar) con el sector resultante o confirmación.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 algún ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. JSON inválido o datos inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 2 JSON inválido, capacidad <= 0, precioBase < 0, nombre duplicado en evento.
  2. El Sistema (Capa de Presentación/Negocio) rechaza.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el evento existe pero pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Evento finalizado (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento está "Finalizado".
  2. El Sistema devuelve **409 Conflict**. Fin del caso de uso.

* **4a. Sector con entradas vendidas (HTTP 409 Conflict - solo eliminar/modificar capacidad):**
  1. Si en el Paso 4 se intenta eliminar sector o reducir capacidad por debajo de entradas vendidas.
  2. El Sistema devuelve **409 Conflict** con mensaje "Sector tiene entradas vendidas, no se puede eliminar/reducir". Fin del caso de uso.

* **5a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5 falla la persistencia.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Crear múltiples sectores en lote (bulk create).
2. Reordenar sectores (drag & drop en UI).

### 6. POSTCONDICIONES
1. Sectores creados/actualizados/eliminados en BD.
2. Si evento estaba "Aprobado", pasa a "Pendiente de aprobación" (requiere revisión **Super Admin**).
3. Precio base definido; RN-03 se aplicará automáticamente si ocupación > 80%.

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `201` | Created | Sector creado exitosamente. |
| `200` | OK | Sector modificado/eliminado exitosamente. |
| `400` | Bad Request | IDs inválidos, JSON inválido, capacidad/precio inválidos, nombre duplicado. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Evento pertenece a otro usuario. |
| `404` | Not Found | Evento o sector inexistente. |
| `409` | Conflict | Evento finalizado, sector con entradas vendidas. |
| `500` | Internal Server Error | Error técnico en persistencia. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato IDs, formato JSON, capacidad > 0, precio >= 0.
- **Verificación (Negocio, → 403/404/409):** propiedad del evento, existencia, estado no finalizado, sector sin entradas vendidas para eliminar/reducir, nombre único por evento.

### Matriz de trazabilidad CU-19 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal (crear) | `201 Created` | `GestionarSectores_Crear_WithValidData_ReturnsSector` | `PostSector_WithValidData_Returns201Created` |
| Flujo principal (modificar) | `200 OK` | `GestionarSectores_Modificar_WithValidData_ReturnsSector` | `PutSector_WithValidData_Returns200OK` |
| Flujo principal (eliminar) | `200 OK` | `GestionarSectores_Eliminar_WithValidId_DeletesSector` | `DeleteSector_WithValidId_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `SectorOps_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `SectorOps_WithInvalidId_Returns400BadRequest` |
| 2b. Datos inválidos | `400 Bad Request` | — | `SectorOps_WithInvalidData_Returns400BadRequest` |
| 3a. Evento no encontrado | `404 Not Found` | `GestionarSectores_WhenEventoNotExists_ThrowsNotFoundException` | `SectorOps_WhenEventoNotExists_Returns404NotFound` |
| 3b. No es propietario | `403 Forbidden` | `GestionarSectores_WhenNotOwner_ThrowsForbiddenException` | `SectorOps_WhenNotOwner_Returns403Forbidden` |
| 3c. Evento finalizado | `409 Conflict` | `GestionarSectores_WhenEventoFinalizado_ThrowsConflictException` | `SectorOps_WhenEventoFinalizado_Returns409Conflict` |
| 4a. Sector con entradas | `409 Conflict` | `GestionarSectores_WhenSectorHasTickets_ThrowsConflictException` | `DeleteSector_WhenHasTickets_Returns409Conflict` |
| 5a. Error interno | `500 Internal Server Error` | `GestionarSectores_WhenRepositoryFails_ThrowsException` | `SectorOps_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.

