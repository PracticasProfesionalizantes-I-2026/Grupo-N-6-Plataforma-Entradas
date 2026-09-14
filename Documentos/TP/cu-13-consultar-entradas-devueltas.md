# Caso de Uso: Consultar entradas devueltas

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-13 |
| **Nombre** | Consultar entradas devueltas |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → ver historial de devoluciones; Sistema → listar entradas devueltas |
| **Disparador (Trigger)** | El usuario accede a "Mis entradas devueltas" en su panel |
| **Prioridad / Frecuencia** | Baja; uso ocasional |
| **Reglas de negocio relacionadas** | RN-02 (reembolso 80%) |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado consultar el listado de sus entradas con estado "Devuelta" (solicitadas y procesadas).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El usuario solo ve sus propias entradas.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/entradas/devueltas` con header Authorization.
2. La **Capa de Presentación** valida el JWT.
3. La **Capa de Negocio** filtra entradas del usuario con estado "Devuelta".
4. El Sistema devuelve **200 OK** con lista paginada (incluye evento, monto reembolsado 80% según RN-02, fecha devolución).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. Sin entradas devueltas (HTTP 200 OK - lista vacía):**
  1. Si en el Paso 3 el usuario no tiene entradas devueltas.
  2. El Sistema devuelve **200 OK** con lista vacía. Fin del caso de uso.

* **3a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 ocurre un error al consultar la base de datos.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Filtros opcionales: por rango de fechas.
2. Incluir detalle de reembolso (monto, método, estado).

### 6. POSTCONDICIONES
1. Se muestra al usuario su historial de entradas devueltas con montos reembolsados.
2. No hay cambio de estado persistente (operación de solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Lista de entradas devueltas retornada (puede ser vacía). |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `500` | Internal Server Error | Error técnico en consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 401):** JWT válido.
- **Verificación (Negocio):** filtro por usuario autenticado, estado "Devuelta", monto según RN-02.

### Matriz de trazabilidad CU-13 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarEntradasDevueltas_WithValidUser_ReturnsList` | `GetEntradasDevueltas_WithValidToken_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetEntradasDevueltas_WithInvalidToken_Returns401Unauthorized` |
| 2a. Sin entradas | `200 OK` | `ConsultarEntradasDevueltas_WhenNone_ReturnsEmptyList` | `GetEntradasDevueltas_WhenEmpty_Returns200EmptyList` |
| 3a. Error interno | `500 Internal Server Error` | `ConsultarEntradasDevueltas_WhenRepositoryFails_ThrowsException` | `GetEntradasDevueltas_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
