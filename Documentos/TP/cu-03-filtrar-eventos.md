# Caso de Uso: Filtrar eventos

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-03 |
| **Nombre** | Filtrar eventos |
| **Actor Principal** | Usuario Visitante (No registrado) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Visitante → encontrar eventos según criterios; Sistema → mostrar resultados filtrados |
| **Disparador (Trigger)** | El visitante aplica filtros en el listado de eventos |
| **Prioridad / Frecuencia** | Alta; uso frecuente |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario visitante filtrar el listado de eventos según criterios como fecha, categoría, ubicación o rango de precio.

### 2. PRECONDICIONES
1. El sistema debe tener eventos publicados (estado "Aprobado") en la base de datos.
2. El visitante accede a la plataforma sin necesidad de autenticación.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/eventos` con parámetros de filtro (query string: fechaDesde, fechaHasta, categoriaId, ubicacion, precioMin, precioMax).
2. La **Capa de Presentación** valida que los parámetros tengan formato correcto.
3. La **Capa de Negocio** aplica los filtros sobre los eventos con estado "Aprobado".
4. El Sistema devuelve un código **200 OK** con la lista paginada de eventos filtrados.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Parámetros inválidos (HTTP 400 Bad Request):**
  1. Si en el Paso 1 algún parámetro tiene formato incorrecto (ej. fecha inválida, precioMin > precioMax).
  2. El Sistema (Capa de Presentación) rechaza la petición por error de validación.
  3. El Sistema devuelve un código **400 Bad Request** detallando el error. Fin del caso de uso.

* **2a. Sin resultados (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 no hay eventos que cumplan los criterios.
  2. El Sistema devuelve **200 OK** con lista vacía y metadata de paginación. Fin del caso de uso.

* **3a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre un error al consultar la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El visitante puede combinar múltiples filtros simultáneamente.
2. Los filtros son opcionales; si no se envían, se listan todos los eventos aprobados.

### 6. POSTCONDICIONES
1. Se muestra al visitante la lista de eventos que cumplen los criterios aplicados.
2. No hay cambio de estado persistente en el sistema (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de eventos filtrados retornada correctamente (puede ser vacía). |
| `400` | Bad Request | Parámetros de filtro con formato inválido. |
| `500` | Internal Server Error | Error técnico no controlado durante la consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400):** formato de fechas, rangos numéricos, existencia de categorías referenciadas.
- **Verificación (Negocio):** aplicación de filtros sobre eventos con estado "Aprobado" únicamente.

### Matriz de trazabilidad CU-03 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `FiltrarEventos_WithValidFilters_ReturnsFilteredList` | `GetEventos_WithFilters_Returns200OK` |
| 1a. Parámetros inválidos | `400 Bad Request` | — (validación de esquema) | `GetEventos_WithInvalidFilters_Returns400BadRequest` |
| 2a. Sin resultados | `200 OK` | `FiltrarEventos_WithNoMatches_ReturnsEmptyList` | `GetEventos_WithNoMatches_Returns200EmptyList` |
| 3a. Error interno | `500 Internal Server Error` | `FiltrarEventos_WhenRepositoryFails_ThrowsException` | `GetEventos_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. En los flujos resueltos en la Capa de Presentación (1a) el test aplicable es el de integración HTTP. Los tests se ejecutan con `dotnet test`.
