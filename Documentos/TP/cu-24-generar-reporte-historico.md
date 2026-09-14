# Caso de Uso: Generar reporte histórico

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-24 |
| **Nombre** | Generar reporte histórico |
| **Actor Principal** | Usuario Registrado (propietario del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Propietario → obtener reporte histórico de ventas/ocupación; Sistema → consolidar datos históricos |
| **Disparador (Trigger)** | El usuario solicita "Generar reporte histórico" para uno de sus eventos |
| **Prioridad / Frecuencia** | Baja; uso esporádico |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al propietario de un evento generar un reporte histórico consolidado (ventas por día, ocupación por sector, ingresos, devoluciones) para un rango de fechas.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El evento debe existir, estar "Aprobado" o "Finalizado" y pertenecer al usuario.
3. El evento debe tener al menos una venta registrada.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 202)
1. El Actor envía una petición al endpoint `POST /api/eventos/{id}/reportes/historico` con JSON (fechaDesde, fechaHasta, formato: PDF/Excel) y header Authorization.
2. La **Capa de Presentación** valida el JWT, ID (GUID), JSON y rango de fechas válido.
3. La **Capa de Negocio** verifica que el evento pertenezca al usuario y esté en estado válido.
4. El Sistema encola la generación del reporte (proceso asíncrono) y devuelve **202 Accepted** con ID de tarea/reporte.
5. El Sistema procesa en background: consulta ventas, devoluciones, ocupación por día/sector, genera archivo.
6. Cuando finaliza, el reporte queda disponible para descarga (ver CU-25).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. JSON inválido o rango inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 JSON inválido, fechaDesde > fechaHasta, fechas futuras, formato no soportado.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el evento existe pero pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Evento sin ventas (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento no tiene ventas en el rango solicitado.
  2. El Sistema devuelve **409 Conflict** con mensaje "No hay datos para generar reporte en el rango seleccionado". Fin del caso de uso.

* **4a. Error encolando tarea (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla el encolado de la tarea asíncrona.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Formato PDF vs. Excel (CSV).
2. Incluir/excluir devoluciones, incluir/excluir detalle por sector.
3. Programar generación recurrente (futuro).

### 6. POSTCONDICIONES
1. Tarea de generación de reporte encolada con estado "Procesando".
2. Reporte generado asíncronamente y almacenado para descarga posterior.
3. Usuario notificado (email/in-app) cuando reporte esté listo.

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `202` | Accepted | Generación de reporte encolada exitosamente. |
| `400` | Bad Request | ID inválido, JSON inválido, rango de fechas inválido, formato no soportado. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Evento pertenece a otro usuario. |
| `404` | Not Found | Evento inexistente. |
| `409` | Conflict | Evento sin ventas en rango solicitado. |
| `500` | Internal Server Error | Error técnico encolando tarea. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato ID, JSON, rango fechas, formato salida.
- **Verificación (Negocio, → 403/404/409):** propiedad del evento, existencia, estado válido, existencia de datos en rango.

### Matriz de trazabilidad CU-24 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `202 Accepted` | `GenerarReporteHistorico_WithValidData_EnqueuesTask` | `PostReporteHistorico_WithValidData_Returns202Accepted` |
| 1a. Token inválido | `401 Unauthorized` | — | `PostReporteHistorico_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `PostReporteHistorico_WithInvalidId_Returns400BadRequest` |
| 2b. JSON/rango inválido | `400 Bad Request` | — | `PostReporteHistorico_WithInvalidRange_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `GenerarReporteHistorico_WhenNotExists_ThrowsNotFoundException` | `PostReporteHistorico_WhenNotExists_Returns404NotFound` |
| 3b. No es propietario | `403 Forbidden` | `GenerarReporteHistorico_WhenNotOwner_ThrowsForbiddenException` | `PostReporteHistorico_WhenNotOwner_Returns403Forbidden` |
| 3c. Sin ventas | `409 Conflict` | `GenerarReporteHistorico_WhenNoSales_ThrowsConflictException` | `PostReporteHistorico_WhenNoSales_Returns409Conflict` |
| 4a. Error encolando | `500 Internal Server Error` | `GenerarReporteHistorico_WhenQueueFails_ThrowsException` | `PostReporteHistorico_WhenQueueFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
