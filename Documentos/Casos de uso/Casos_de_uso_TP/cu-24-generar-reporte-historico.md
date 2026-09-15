# Caso de Uso: Consultar registro de transacciones

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.
> Simplificado para MVP: solo registro de transacciones (sin generación async de reportes PDF/Excel).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-24 |
| **Nombre** | Consultar registro de transacciones |
| **Actor Principal** | Usuario Registrado (propietario del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Propietario → consultar transacciones de sus eventos; Sistema → exponer datos de auditoría |
| **Disparador (Trigger)** | El usuario solicita "Ver transacciones" para uno de sus eventos |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al propietario de un evento consultar el registro de transacciones (compras, devoluciones) de sus eventos con filtros por fecha y estado.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El evento debe existir y pertenecer al usuario.
3. El evento debe tener al menos una transacción registrada.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/eventos/{id}/transacciones` con header Authorization y parámetros de filtro opcionales (fechaDesde, fechaHasta, estado, página, tamaño).
2. La **Capa de Presentación** valida el JWT, ID (GUID) y parámetros de paginación/filtro.
3. La **Capa de Negocio** verifica que el evento pertenezca al usuario.
4. La **Capa de Negocio** consulta transacciones (compras y devoluciones) aplicando filtros y paginación.
5. El Sistema devuelve **200 OK** con lista paginada de transacciones (ID, tipo: compra/devolución, fecha, monto, entradas, estado, comprador).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. Parámetros inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 2 parámetros de paginación o rango de fechas tienen formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el evento existe pero pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Sin transacciones (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 4 no hay transacciones con los filtros aplicados.
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 ocurre error consultando la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Filtros opcionales: por tipo (compra/devolución), por rango de fechas, por estado de transacción.
2. Ordenamiento: por fecha descendente (más reciente primero).
3. Exportar a CSV (futuro, no en MVP).

### 6. POSTCONDICIONES
1. Se muestra al usuario el listado paginado de transacciones de su evento.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de transacciones retornada (puede ser vacía). |
| `400` | Bad Request | ID inválido, parámetros de paginación/filtro inválidos. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Evento pertenece a otro usuario. |
| `404` | Not Found | Evento inexistente. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato ID, parámetros paginación/filtro.
- **Verificación (Negocio, → 403/404):** propiedad del evento, existencia.

### Matriz de trazabilidad CU-24 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarTransacciones_WithValidEvento_ReturnsPagedList` | `GetTransacciones_WithValidToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetTransacciones_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `GetTransacciones_WithInvalidId_Returns400BadRequest` |
| 2b. Parámetros inválidos | `400 Bad Request` | — | `GetTransacciones_WithInvalidParams_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `ConsultarTransacciones_WhenNotExists_ThrowsNotFoundException` | `GetTransacciones_WhenNotExists_Returns404NotFound` |
| 3b. No es propietario | `403 Forbidden` | `ConsultarTransacciones_WhenNotOwner_ThrowsForbiddenException` | `GetTransacciones_WhenNotOwner_Returns403Forbidden` |
| 3c. Sin transacciones | `200 OK` | `ConsultarTransacciones_WhenNone_ReturnsEmptyList` | `GetTransacciones_WhenEmpty_Returns200EmptyList` |
| 4a. Error interno | `500 Internal Server Error` | `ConsultarTransacciones_WhenRepositoryFails_ThrowsException` | `GetTransacciones_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
