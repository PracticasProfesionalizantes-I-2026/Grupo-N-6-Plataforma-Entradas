# Caso de Uso: Visualizar comprobante

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-14 |
| **Nombre** | Visualizar comprobante |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → ver/descargar comprobante de compra; Sistema → generar documento fiscal |
| **Disparador (Trigger)** | El usuario selecciona "Ver comprobante" en una entrada activa o compra |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado visualizar y descargar el comprobante de compra (PDF) de una entrada específica, el cual incluye un **código único alfanumérico** por entrada para validación externa en el acceso al evento.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. La entrada debe pertenecer al usuario autenticado.
3. La compra debe estar confirmada (entrada en estado "Activa" o "Utilizada").

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/entradas/{id}/comprobante` con header Authorization.
2. La **Capa de Presentación** valida el JWT y el formato del ID (GUID).
3. La **Capa de Negocio** verifica que la entrada pertenezca al usuario y esté en estado válido.
4. El Sistema genera el comprobante PDF (o recupera si ya generado) que incluye **código QR y código único alfanumérico** por entrada para validación externa.
5. El Sistema devuelve **200 OK** con `Content-Type: application/pdf` y el archivo que contiene el código único alfanumérico para validación en control de acceso externo.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es un GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Entrada no encontrada (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe entrada con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Entrada no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 la entrada existe pero pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Entrada en estado inválido (HTTP 409 Conflict):**
  1. Si en el Paso 3 la entrada está en estado "Devuelta", "Cancelada" o "Pendiente pago".
  2. El Sistema devuelve **409 Conflict** con mensaje "No hay comprobante disponible para este estado". Fin del caso de uso.

* **4a. Error generando PDF (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la generación del PDF.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Descarga directa vs. visualización en navegador (header `Content-Disposition`).
2. Comprobante fiscal vs. ticket simple (según configuración).

### 6. POSTCONDICIONES
1. Comprobante PDF entregado al usuario con **código único alfanumérico** por entrada.
2. El código único permite validación externa en aplicación de control de acceso (fuera de este sistema).
3. No hay cambio de estado persistente (operación de solo lectura/generación).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Comprobante PDF con código único alfanumérico retornado correctamente. |
| `400` | Bad Request | ID con formato inválido. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Entrada pertenece a otro usuario. |
| `404` | Not Found | Entrada inexistente. |
| `409` | Conflict | Entrada en estado que no genera comprobante. |
| `500` | Internal Server Error | Error técnico generando PDF. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato ID (GUID).
- **Verificación (Negocio, → 403/404/409):** propiedad de la entrada, existencia, estado válido para comprobante.

### Matriz de trazabilidad CU-14 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `VisualizarComprobante_WithValidEntrada_ReturnsPdfWithAlfanumericCode` | `GetComprobante_WithValidEntrada_Returns200PdfWithCode` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetComprobante_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `GetComprobante_WithInvalidId_Returns400BadRequest` |
| 3a. Entrada no encontrada | `404 Not Found` | `VisualizarComprobante_WhenNotExists_ThrowsNotFoundException` | `GetComprobante_WhenNotExists_Returns404NotFound` |
| 3b. Entrada de otro usuario | `403 Forbidden` | `VisualizarComprobante_WhenNotOwner_ThrowsForbiddenException` | `GetComprobante_WhenNotOwner_Returns403Forbidden` |
| 3c. Estado inválido | `409 Conflict` | `VisualizarComprobante_WhenInvalidState_ThrowsConflictException` | `GetComprobante_WhenInvalidState_Returns409Conflict` |
| 4a. Error PDF | `500 Internal Server Error` | `VisualizarComprobante_WhenPdfFails_ThrowsException` | `GetComprobante_WhenPdfFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.

