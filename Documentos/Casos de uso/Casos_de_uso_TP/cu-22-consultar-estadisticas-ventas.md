# Caso de Uso: Consultar Estadisticas de Ventas

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Reglas de negocio (solo propietario consulta sus eventos, info actualizada al momento) **implementadas** en el codigo; cada caso borde cuenta con su test unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-22 |
| **Nombre** | Consultar Estadisticas de Ventas |
| **Actor Principal** | Usuario Registrado (propietario del evento) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario Registrado -> ver estadisticas; Sistema -> calcular y mostrar |
| **Disparador (Trigger)** | El usuario registrado selecciona "Estadisticas" de uno de sus eventos |
| **Prioridad / Frecuencia** | Media; baja frecuencia |
| **Reglas de negocio relacionadas** | Solo el propietario del evento podra consultar las estadisticas correspondientes a dicho evento; El sistema debera mostrar informacion actualizada al momento de realizar la consulta |

---

### 1. BREVE DESCRIPCION
Permite al usuario registrado consultar informacion estadistica sobre las ventas de sus eventos (entradas vendidas, ingresos, ocupacion por sector).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado en la plataforma (Token JWT valido).
2. El usuario debe ser propietario del evento (FK `UsuarioId` en evento).
3. El evento debe existir en el sistema.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envia una peticion al endpoint `GET /api/mis-eventos`.
2. La **Capa de Presentacion** (`MisEventosController.GetMisEventos`) valida autenticacion y retorna eventos del usuario.
3. El Actor selecciona un evento y envia `GET /api/eventos/{id}/estadisticas`.
4. La **Capa de Presentacion** (`EventosController.GetEstadisticas`) valida que el usuario sea propietario del evento.
5. La **Capa de Negocio** (`EstadisticasService.GetEstadisticasEventoAsync`):
   a. Verifica propiedad del evento (usuario autenticado = creador).
   b. Calcula: total entradas vendidas, ingresos totales, entradas por sector, porcentaje ocupacion por sector, entradas disponibles.
   c. Obtiene datos actualizados en tiempo real de la **Capa de Persistencia**.
6. El Sistema devuelve un codigo **200 OK** con objeto: `{eventoId, nombre, totalVendidas, ingresosTotal, sectores[{nombre, capacidad, vendidas, disponibles, ocupacionPorcentaje, ingresos}], fechaConsulta}`.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. Usuario sin eventos (HTTP 200 OK):**
  1. Si en el Paso 2 el usuario no tiene eventos creados.
  2. El Sistema devuelve lista vacia. Fin del caso de uso (hasta paso 2).

* **4a. Evento inexistente (HTTP 404 Not Found):**
  1. Si en el Paso 4 el `id` no corresponde a ningun evento.
  2. La Capa de Negocio no encuentra la entidad.
  3. El Sistema devuelve **404 Not Found**: "Evento no encontrado". Fin del caso de uso.

* **5a. Usuario no es propietario (HTTP 403 Forbidden):**
  1. Si en el Paso 5a el usuario autenticado no es el creador del evento.
  2. La Capa de Negocio lanza `AccesoNoAutorizadoException`.
  3. El Sistema devuelve **403 Forbidden**: "No tiene permiso para consultar estadisticas de este evento". Fin del caso de uso.

* **5b. Error al recuperar informacion (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5b ocurre error tecnico al calcular o consultar BD.
  2. El Sistema interrumpe y registra error no controlado.
  3. El Sistema devuelve **500 Internal Server Error**: "No fue posible obtener las estadisticas. Intente nuevamente mas tarde". Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El actor puede consultar desde panel web, coleccion Bruno (`GET EstadisticasEvento.bru`), Postman, Swagger/Scalar.
2. Las estadisticas pueden incluir filtros temporales (ultima semana, mes, todo el periodo).
3. En todas las variantes el endpoint y resultado (`200 OK` con estadisticas actualizadas) son identicos.

### 6. POSTCONDICIONES
1. Las estadisticas del evento son visualizadas por el usuario propietario.
2. La informacion refleja el estado actualizado al momento de la consulta (tiempo real).

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Estadisticas calculadas y retornadas exitosamente. |
| `403` | Forbidden | Usuario autenticado no es propietario del evento. |
| `404` | Not Found | Evento inexistente. |
| `500` | Internal Server Error | Error tecnico no controlado durante el calculo o consulta. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** No aplica parametros complejos (solo path `id` validado por routing).
- **Verificacion (Negocio, -> 403/404/500):** Propiedad del evento (usuario = creador) -> 403; existencia evento -> 404; calculo tiempo real y persistencia -> 500. Regla: solo propietario, info actualizada.

### Matriz de trazabilidad CU-22 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `GetEstadisticasEventoAsync_WithValidOwner_ReturnsEstadisticasCompletas` | `GetEstadisticasEvento_WithValidOwner_Returns200OK` |
| 2a. Sin eventos | `200 OK` (lista vacia) | `GetMisEventosAsync_WhenNone_ReturnsEmptyList` | `GetMisEventos_WhenNone_Returns200EmptyList` |
| 4a. Evento inexistente | `404 Not Found` | `GetEstadisticasEventoAsync_WhenNotFound_ThrowsEventoNotFoundException` | `GetEstadisticasEvento_WhenNotFound_Returns404NotFound` |
| 5a. No es propietario | `403 Forbidden` | `GetEstadisticasEventoAsync_WhenNotOwner_ThrowsAccesoNoAutorizadoException` | `GetEstadisticasEvento_WhenNotOwner_Returns403Forbidden` |
| 5b. Error calculo/BD | `500 Internal Server Error` | `GetEstadisticasEventoAsync_WhenRepositoryThrows_ThrowsException` | `GetEstadisticasEvento_WhenDatabaseError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
