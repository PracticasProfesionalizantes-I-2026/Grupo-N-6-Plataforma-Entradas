# Caso de Uso: Modificar evento

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-17 |
| **Nombre** | Modificar evento |
| **Actor Principal** | Usuario Registrado (creador del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Creador → actualizar información de su evento; Administrador → revisar cambios si evento aprobado; Sistema → mantener datos consistentes |
| **Disparador (Trigger)** | El usuario creador selecciona "Modificar" en uno de sus eventos |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al usuario creador modificar los datos de un evento propio. Si el evento está "Aprobado", los cambios requieren nueva aprobación.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El evento debe existir y pertenecer al usuario autenticado.
3. El evento no debe estar en estado "Cancelado" ni "Finalizado".

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `PUT /api/eventos/{id}` con JSON (datos actualizados) y header Authorization.
2. La **Capa de Presentación** valida el JWT, ID (GUID) y formato JSON.
3. La **Capa de Negocio** verifica que el evento pertenezca al usuario y no esté en estado terminal.
4. La **Capa de Negocio** actualiza los datos del evento.
5. Si el evento estaba "Aprobado", su estado cambia a "Pendiente de aprobación".
6. El Sistema devuelve **200 OK** con el evento actualizado.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es un GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. JSON inválido o campos inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el JSON tiene sintaxis incorrecta o campos con formato inválido (fecha pasada, capacidad negativa, etc.).
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el evento existe pero pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Estado terminal (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento está "Cancelado" o "Finalizado".
  2. El Sistema devuelve **409 Conflict** con mensaje "No se puede modificar un evento en este estado". Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la persistencia.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Modificación parcial (PATCH) vs. completa (PUT).
2. Cambio de sectores requiere re-configuración (ver CU-18).

### 6. POSTCONDICIONES
1. Datos del evento actualizados en BD.
2. Si estaba "Aprobado", pasa a "Pendiente de aprobación" (requiere revisión admin).
3. Historial de cambios registrado (auditoría).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Evento modificado correctamente. |
| `400` | Bad Request | ID inválido, JSON inválido, campos con formato incorrecto. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Evento pertenece a otro usuario. |
| `404` | Not Found | Evento inexistente. |
| `409` | Conflict | Evento en estado terminal (Cancelado/Finalizado). |
| `500` | Internal Server Error | Error técnico en persistencia. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato ID, formato JSON, campos obligatorios, rangos.
- **Verificación (Negocio, → 403/404/409):** propiedad del evento, existencia, estado no terminal.

### Matriz de trazabilidad CU-17 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ModificarEvento_WithValidData_UpdatesAndReturnsEvento` | `PutEvento_WithValidData_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `PutEvento_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `PutEvento_WithInvalidId_Returns400BadRequest` |
| 2b. JSON/campos inválidos | `400 Bad Request` | — | `PutEvento_WithInvalidFields_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `ModificarEvento_WhenNotExists_ThrowsNotFoundException` | `PutEvento_WhenNotExists_Returns404NotFound` |
| 3b. No es propietario | `403 Forbidden` | `ModificarEvento_WhenNotOwner_ThrowsForbiddenException` | `PutEvento_WhenNotOwner_Returns403Forbidden` |
| 3c. Estado terminal | `409 Conflict` | `ModificarEvento_WhenTerminalState_ThrowsConflictException` | `PutEvento_WhenTerminalState_Returns409Conflict` |
| 4a. Error interno | `500 Internal Server Error` | `ModificarEvento_WhenRepositoryFails_ThrowsException` | `PutEvento_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
