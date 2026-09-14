# Caso de Uso: Consultar porcentaje de ocupación

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-23 |
| **Nombre** | Consultar porcentaje de ocupación |
| **Actor Principal** | Usuario Registrado (propietario del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Propietario → conocer ocupación por sector; Sistema → calcular métricas en tiempo real |
| **Disparador (Trigger)** | El usuario selecciona "Ver ocupación" en uno de sus eventos aprobados |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | RN-03 (incremento dinámico 20% > 80% ocupación) |

---

### 1. BREVE DESCRIPCIÓN
Permite al propietario de un evento consultar el porcentaje de ocupación por sector y global, en tiempo real.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El evento debe existir, estar "Aprobado" y pertenecer al usuario.
3. El evento debe tener al menos un sector configurado.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `GET /api/eventos/{id}/ocupacion` con header Authorization.
2. La **Capa de Presentación** valida el JWT y el ID (GUID).
3. La **Capa de Negocio** verifica que el evento pertenezca al usuario y esté "Aprobado".
4. La **Capa de Negocio** calcula ocupación por sector: (entradas vendidas / capacidad) * 100, y ocupación global.
5. El Sistema devuelve **200 OK** con detalle por sector (capacidad, vendidas, %, precio actual - considerando RN-03) y totales.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. ID inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el ID no es GUID válido.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Evento no encontrado (HTTP 404 Not Found):**
  1. Si en el Paso 3 no existe evento con ese ID.
  2. El Sistema devuelve **404 Not Found**. Fin del caso de uso.

* **3b. Evento no pertenece al usuario (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el evento existe pero pertenece a otro usuario.
  2. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **3c. Evento no aprobado (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento no está en estado "Aprobado".
  2. El Sistema devuelve **409 Conflict** con mensaje "Ocupación solo disponible para eventos aprobados". Fin del caso de uso.

* **3d. Evento sin sectores (HTTP 409 Conflict):**
  1. Si en el Paso 3 el evento no tiene sectores configurados.
  2. El Sistema devuelve **409 Conflict**. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 ocurre un error al calcular/consultar.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Incluir proyección de ingresos según precios actuales (con/sin RN-03 aplicado).
2. Filtrar por sector específico.

### 6. POSTCONDICIONES
1. Se muestra al usuario el porcentaje de ocupación por sector y global.
2. No hay cambio de estado persistente (operación de solo lectura/calculada).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Porcentajes de ocupación calculados y retornados. |
| `400` | Bad Request | ID con formato inválido. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `403` | Forbidden | Evento pertenece a otro usuario. |
| `404` | Not Found | Evento inexistente. |
| `409` | Conflict | Evento no aprobado o sin sectores. |
| `500` | Internal Server Error | Error técnico en cálculo/consulta. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato ID (GUID).
- **Verificación (Negocio, → 403/404/409):** propiedad del evento, existencia, estado "Aprobado", sectores configurados. Cálculo aplica RN-03 para precio actual.

### Matriz de trazabilidad CU-23 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ConsultarOcupacion_WithValidEvento_ReturnsOcupacionDetalle` | `GetOcupacion_WithValidEvento_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `GetOcupacion_WithInvalidToken_Returns401Unauthorized` |
| 2a. ID inválido | `400 Bad Request` | — | `GetOcupacion_WithInvalidId_Returns400BadRequest` |
| 3a. No encontrado | `404 Not Found` | `ConsultarOcupacion_WhenNotExists_ThrowsNotFoundException` | `GetOcupacion_WhenNotExists_Returns404NotFound` |
| 3b. No es propietario | `403 Forbidden` | `ConsultarOcupacion_WhenNotOwner_ThrowsForbiddenException` | `GetOcupacion_WhenNotOwner_Returns403Forbidden` |
| 3c. No aprobado | `409 Conflict` | `ConsultarOcupacion_WhenNotApproved_ThrowsConflictException` | `GetOcupacion_WhenNotApproved_Returns409Conflict` |
| 3d. Sin sectores | `409 Conflict` | `ConsultarOcupacion_WhenNoSectores_ThrowsConflictException` | `GetOcupacion_WhenNoSectores_Returns409Conflict` |
| 4a. Error interno | `500 Internal Server Error` | `ConsultarOcupacion_WhenRepositoryFails_ThrowsException` | `GetOcupacion_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
