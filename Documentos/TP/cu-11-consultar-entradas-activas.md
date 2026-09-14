# Caso de Uso: Consultar entradas activas

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-11 |
| **Nombre** | Consultar entradas activas |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → ver sus entradas válidas para eventos futuros; Sistema → listar entradas asociadas al usuario |
| **Disparador (Trigger)** | El usuario accede a "Mis entradas activas" en su panel |
| **Prioridad / Frecuencia** | Alta; uso frecuente |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado consultar el listado de sus entradas con estado "Activa" (eventos futuros no utilizados).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El usuario solo ve sus propias entradas.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/entradas/activas` con header Authorization.
2. La **Capa de Presentación** valida el JWT.
3. La **Capa de Negocio** filtra entradas del usuario con estado "Activa" y evento con fecha futura.
4. El Sistema devuelve **200 OK** con lista paginada de entradas (incluye evento, sector, asiento, QR).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. Sin entradas activas (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 el usuario no tiene entradas activas.
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **3a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre un error al consultar la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Filtros opcionales: por evento, por fecha rango.
2. Ordenamiento: por fecha evento ascendente/descendente.

### 6. POSTCONDICIONES
1. Se muestra al usuario su listado de entradas activas.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de entradas activas retornada (puede ser vacía). |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 401):** JWT válido.
- **Verificación (Negocio):** filtro por usuario autenticado, estado "Activa", fecha evento futura.

### Matriz de trazabilidad CU-11 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarEntradasActivas_WithValidUser_ReturnsList` | `GetEntradasActivas_WithValidToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetEntradasActivas_WithInvalidToken_Returns401Unauthorized` |
| 2a. Sin entradas | `200 OK` | `ConsultarEntradasActivas_WhenNone_ReturnsEmptyList` | `GetEntradasActivas_WhenEmpty_Returns200EmptyList` |
| 3a. Error interno | `500 Internal Server Error` | `ConsultarEntradasActivas_WhenRepositoryFails_ThrowsException` | `GetEntradasActivas_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
