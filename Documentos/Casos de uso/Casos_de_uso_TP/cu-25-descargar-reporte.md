# Caso de Uso: Descargar reporte

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-25 |
| **Nombre** | Descargar reporte |
| **Actor Principal** | Usuario Registrado (propietario del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Propietario → descargar reporte generado; Sistema → servir archivo generado |
| **Disparador (Trigger)** | El usuario accede a "Mis reportes" y selecciona descargar uno completado |
| **Prioridad / Frecuencia** | Baja; uso esporádico |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite al propietario de un evento descargar un reporte histórico previamente generado (ver CU-24) en el formato solicitado (PDF/Excel).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El reporte debe existir, estar en estado "Completado" y pertenecer a un evento del usuario.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/reportes/{id}/descargar` con header Authorization.
2. La **Capa de Presentación** valida el JWT y el ID del reporte (GUID).
3. La **Capa de Negocio** verifica que el reporte pertenezca a un evento del usuario y esté "Completado".
4. El Sistema recupera el archivo generado (storage/blob).
5. El Sistema devuelve **200 OK** con `Content-Type` según formato (application/pdf, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet) y el archivo en el body.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Reporte no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe reporte con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Reporte no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el reporte existe pero su evento pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Reporte no completado (HTTP 409 Conflict):**
  1. Si en el Paso 3 el reporte está en estado "Procesando", "Fallido" o "Pendiente".
  2. El Sistema devuelve **409 Conflict** con mensaje "Reporte no disponible para descarga". Fin del caso de uso.

* **4a. Archivo no encontrado en storage (HTTP 404 Not Found):**
  1. Si en el Paso 4 el archivo generado no existe en el almacenamiento.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **5a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4/5 falla la lectura del archivo.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Descarga directa vs. URL firmada temporal (pre-signed URL) para archivos grandes.

### 6. POSTCONDICIONES
1. Archivo de reporte entregado al usuario.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Archivo de reporte descargado correctamente. |
| `400` | Bad Request | ID con formato inválido. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Reporte pertenece a evento de otro usuario. |
| `404` | Not Found | Reporte inexistente o archivo no encontrado en storage. |
| `409` | Conflict | Reporte en estado no descargable (Procesando/Fallido/Pendiente). |
| `500` | Internal Server Error | Error técnico leyendo archivo. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato ID (GUID).
- **Verificación (Negocio, → 403/404/409):** propiedad del reporte (vía evento), existencia, estado "Completado", archivo existe en storage.

### Matriz de trazabilidad CU-25 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `DescargarReporte_WithValidReporte_ReturnsFile` | `GetDescargarReporte_WithValidId_Returns200File` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetDescargarReporte_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `GetDescargarReporte_WithInvalidId_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `DescargarReporte_WhenNotExists_ThrowsNotFoundException` | `GetDescargarReporte_WhenNotExists_Returns404NotFound` |
| 3b. No es propietario | `403 Forbidden` | `DescargarReporte_WhenNotOwner_ThrowsForbiddenException` | `GetDescargarReporte_WhenNotOwner_Returns403Forbidden` |
| 3c. No completado | `409 Conflict` | `DescargarReporte_WhenNotCompleted_ThrowsConflictException` | `GetDescargarReporte_WhenNotCompleted_Returns409Conflict` |
| 4a. Archivo no en storage | `404 Not Found` | `DescargarReporte_WhenFileMissing_ThrowsNotFoundException` | `GetDescargarReporte_WhenFileMissing_Returns404NotFound` |
| 5a. Error interno | `500 Internal Server Error` | `DescargarReporte_WhenStorageFails_ThrowsException` | `GetDescargarReporte_WhenStorageFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
