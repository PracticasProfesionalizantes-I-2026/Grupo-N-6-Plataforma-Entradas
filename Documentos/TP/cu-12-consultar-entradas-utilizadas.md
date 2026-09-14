# Caso de Uso: Consultar entradas utilizadas

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-12 |
| **Nombre** | Consultar entradas utilizadas |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → ver historial de eventos asistidos; Sistema → listar entradas usadas |
| **Disparador (Trigger)** | El usuario accede a "Mis entradas utilizadas" en su panel |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado consultar el listado de sus entradas con estado "Utilizada" (eventos pasados donde se validó el ingreso).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El usuario solo ve sus propias entradas.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/entradas/utilizadas` con header Authorization.
2. La **Capa de Presentación** valida el JWT.
3. La **Capa de Negocio** filtra entradas del usuario con estado "Utilizada".
4. El Sistema devuelve **200 OK** con lista paginada de entradas (incluye evento, sector, fecha validación).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. Sin entradas utilizadas (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 el usuario no tiene entradas utilizadas.
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **3a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre un error al consultar la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Filtros opcionales: por rango de fechas, por evento.
2. Ordenamiento: por fecha de validación descendente.

### 6. POSTCONDICIONES
1. Se muestra al usuario su historial de entradas utilizadas.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de entradas utilizadas retornada (puede ser vacía). |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 401):** JWT válido.
- **Verificación (Negocio):** filtro por usuario autenticado, estado "Utilizada".

### Matriz de trazabilidad CU-12 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarEntradasUtilizadas_WithValidUser_ReturnsList` | `GetEntradasUtilizadas_WithValidToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetEntradasUtilizadas_WithInvalidToken_Returns401Unauthorized` |
| 2a. Sin entradas | `200 OK` | `ConsultarEntradasUtilizadas_WhenNone_ReturnsEmptyList` | `GetEntradasUtilizadas_WhenEmpty_Returns200EmptyList` |
| 3a. Error interno | `500 Internal Server Error` | `ConsultarEntradasUtilizadas_WhenRepositoryFails_ThrowsException` | `GetEntradasUtilizadas_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
