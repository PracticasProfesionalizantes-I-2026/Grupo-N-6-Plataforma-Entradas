# Caso de Uso: Ver detalle de evento

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-04 |
| **Nombre** | Ver detalle de evento |
| **Actor Principal** | Usuario Visitante (No registrado) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Visitante → conocer información completa del evento; Sistema → mostrar detalle público |
| **Disparador (Trigger)** | El visitante selecciona un evento del listado para ver su detalle |
| **Prioridad / Frecuencia** | Alta; uso frecuente |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario visitante consultar la información completa de un evento aprobado (descripción, fecha, lugar, sectores, precios, disponibilidad).

### 2. PRECONDICIONES
1. El evento debe existir y tener estado "Aprobado".
2. El visitante accede sin necesidad de autenticación.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/eventos/{id}`.
2. La **Capa de Presentación** valida que el ID tenga formato válido (GUID).
3. La **Capa de Negocio** busca el evento y verifica que su estado sea "Aprobado".
4. El Sistema devuelve un código **200 OK** con el detalle completo del evento (incluye sectores, precios, disponibilidad).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 1 el ID no es un GUID válido.
  2. El Sistema (Capa de Presentación) rechaza la petición por error de validación.
  3. El Sistema devuelve un código **400 Bad Request**. Fin del caso de uso.

* **2a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe ningún evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **2b. Evento no aprobado (HTTP 404 Not Found):**
  1. Si en el Paso 3 el evento existe pero su estado no es "Aprobado" (ej. "Pendiente", "Rechazado", "Borrador").
  2. El Sistema no expone el evento y devuelve **404 Not Found**. Fin del caso de uso.

* **3a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre un error al consultar la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El visitante puede acceder al detalle desde el listado, resultados de búsqueda, o enlace directo.

### 6. POSTCONDICIONES
1. Se muestra al visitante la información completa del evento.
2. No hay cambio de estado persistente en el sistema (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Detalle del evento retornado correctamente. |
| `400` | Bad Request | ID con formato inválido. |
| `404` | Not Found | Evento inexistente o no aprobado. |
| `500` | Internal Server Error | Error técnico no controlado durante la consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400):** formato del ID (GUID).
- **Verificación (Negocio, → 404):** existencia del evento y verificación de estado "Aprobado".

### Matriz de trazabilidad CU-04 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ObtenerDetalleEvento_WithValidId_ReturnsEventoDetalle` | `GetEventoById_WithValidId_Returns200OK` |
| 1a. ID inválido | `400 Bad Request` | — (validación de esquema) | `GetEventoById_WithInvalidId_Returns400BadRequest` |
| 2a. Evento no encontrado | `404 Not Found` | `ObtenerDetalleEvento_WhenNotExists_ThrowsNotFoundException` | `GetEventoById_WhenNotExists_Returns404NotFound` |
| 2b. Evento no aprobado | `404 Not Found` | `ObtenerDetalleEvento_WhenNotApproved_ThrowsNotFoundException` | `GetEventoById_WhenNotApproved_Returns404NotFound` |
| 3a. Error interno | `500 Internal Server Error` | `ObtenerDetalleEvento_WhenRepositoryFails_ThrowsException` | `GetEventoById_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
