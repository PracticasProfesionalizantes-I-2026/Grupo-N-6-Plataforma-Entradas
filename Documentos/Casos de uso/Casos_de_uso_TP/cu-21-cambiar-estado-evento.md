# Caso de Uso: Cambiar Estado de Evento

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Reglas de negocio (solo admin aprueba/rechaza, solo "Aprobado" visible y vendible, rechazado no publicable sin nueva revision) **implementadas** en el codigo; cada caso borde cuenta con su test unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-21 |
| **Nombre** | Cambiar Estado de Evento |
| **Actor Principal** | Administrador |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Administrador -> aprobar/rechazar; Usuario creador -> notificacion; Usuarios visitantes -> visibilidad |
| **Disparador (Trigger)** | El Administrador selecciona la opcion para modificar el estado de un evento |
| **Prioridad / Frecuencia** | Alta; media frecuencia |
| **Reglas de negocio relacionadas** | Solo un administrador podra aprobar o rechazar eventos; Solo los eventos con estado "Aprobado" seran visibles para los usuarios visitantes y podran habilitar la venta de entradas; Una vez rechazado un evento, no podra publicarse sin una nueva revision administrativa |

---

### 1. BREVE DESCRIPCION
Permite al administrador revisar un evento y modificar su estado de acuerdo con las reglas de negocio establecidas, habilitando o rechazando su publicacion en la plataforma.

### 2. PRECONDICIONES
1. El evento debe existir en el sistema.
2. El actor debe tener permisos de administrador (Token JWT con rol "Admin").
3. El evento debe encontrarse en un estado que permita la transicion solicitada (ej. "Pendiente de aprobacion" para aprobar/rechazar).

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envia una peticion al endpoint `GET /api/admin/eventos/pendientes`.
2. La **Capa de Presentacion** (`AdminController.GetEventosPendientes`) valida rol administrador.
3. La **Capa de Negocio** (`AdminService.GetEventosPendientesAsync`) consulta eventos con estado "Pendiente de aprobacion".
4. El Actor selecciona un evento y envia `PATCH /api/admin/eventos/{id}/estado` con `{nuevoEstado: "Aprobado" | "Rechazado", observaciones?}`.
5. La **Capa de Presentacion** (`AdminController.UpdateEstadoEvento`) valida esquema y rol.
6. La **Capa de Negocio** (`AdminService.UpdateEstadoEventoAsync`):
   a. Verifica que el evento exista y este en "Pendiente de aprobacion".
   b. Valida transicion de estado permitida.
   c. Actualiza estado del evento a "Aprobado" o "Rechazado".
   d. Registra auditoria: adminId, fecha, estadoAnterior, estadoNuevo, observaciones.
   e. Notifica al usuario creador del evento (email/push: "Tu evento fue aprobado/rechazado").
7. El Sistema devuelve un codigo **200 OK** con el evento actualizado (ID, nombre, nuevoEstado, fechaCambio).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **3a. Sin eventos pendientes (HTTP 200 OK):**
  1. Si en el Paso 3 no hay eventos en "Pendiente de aprobacion".
  2. El Sistema devuelve lista vacia con mensaje: "No hay eventos pendientes de revision". Fin del caso de uso (hasta paso 3).

* **4a. Evento inexistente (HTTP 404 Not Found):**
  1. Si en el Paso 4 el `id` no corresponde a ningun evento.
  2. La Capa de Negocio no encuentra la entidad.
  3. El Sistema devuelve **404 Not Found**: "Evento no encontrado". Fin del caso de uso.

* **6a. Evento ya no esta "Pendiente de aprobacion" (HTTP 409 Conflict):**
  1. Si en el Paso 6a el evento ya fue procesado (estado "Aprobado" o "Rechazado").
  2. La Capa de Negocio lanza `EstadoInvalidoException`: "El evento ya fue procesado y no permite modificar su estado".
  3. El Sistema devuelve **409 Conflict** con mensaje. Fin del caso de uso.

* **6b. Transicion de estado no permitida (HTTP 409 Conflict):**
  1. Si en el Paso 6a se intenta cambiar a estado invalido (ej. de "Rechazado" a "Aprobado" sin revision).
  2. La Capa de Negocio lanza `TransicionEstadoInvalidaException`.
  3. El Sistema devuelve **409 Conflict**: "Transicion de estado no permitida". Fin del caso de uso.

* **6c. Error al actualizar estado (HTTP 500 Internal Server Error):**
  1. Si en el Paso 6c la Capa de Persistencia falla al guardar.
  2. El Sistema interrumpe y registra error no controlado.
  3. El Sistema devuelve **500 Internal Server Error**: "No fue posible completar la operacion. Intentelo nuevamente". Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El administrador puede actuar desde panel web admin, coleccion Bruno (`PATCH CambiarEstadoEvento.bru`), Postman, Swagger/Scalar.
2. Las observaciones son opcionales pero recomendadas al rechazar.
3. En todas las variantes el endpoint y resultado (`200 OK` con evento actualizado) son identicos.

### 6. POSTCONDICIONES
1. El estado del evento queda actualizado segun la decision del administrador.
2. El cambio queda registrado en auditoria (tabla `EventosEstadosHistorial`).
3. Eventos aprobados: publicados, visibles para usuarios visitantes, habilitados para venta (CU-10).
4. Eventos rechazados: permanecen ocultos, no pueden comercializar entradas, requieren nueva revision para publicar.

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Estado actualizado exitosamente (aprobado/rechazado). |
| `404` | Not Found | Evento inexistente. |
| `409` | Conflict | Evento ya procesado, transicion no permitida. |
| `500` | Internal Server Error | Error tecnico no controlado durante la actualizacion. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** Esquema JSON del body (`nuevoEstado` requerido, valores permitidos "Aprobado"/"Rechazado"), rol Admin (policy/attribute).
- **Verificacion (Negocio, -> 404/409/500):** Existencia de evento, estado actual "Pendiente de aprobacion", transicion valida, persistencia. Reglas: solo admin, solo "Aprobado" = visible/vendible, rechazado = oculto sin nueva revision.

### Matriz de trazabilidad CU-21 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal (Aprobar) | `200 OK` | `UpdateEstadoEventoAsync_WhenApproving_ReturnsEventoAprobado` | `UpdateEstadoEvento_WhenApproving_Returns200OK` |
| Flujo principal (Rechazar) | `200 OK` | `UpdateEstadoEventoAsync_WhenRejecting_ReturnsEventoRechazado` | `UpdateEstadoEvento_WhenRejecting_Returns200OK` |
| 3a. Sin eventos pendientes | `200 OK` (lista vacia) | `GetEventosPendientesAsync_WhenNone_ReturnsEmptyList` | `GetEventosPendientes_WhenNone_Returns200EmptyList` |
| 4a. Evento inexistente | `404 Not Found` | `UpdateEstadoEventoAsync_WhenNotFound_ThrowsEventoNotFoundException` | `UpdateEstadoEvento_WhenNotFound_Returns404NotFound` |
| 6a. Ya procesado | `409 Conflict` | `UpdateEstadoEventoAsync_WhenAlreadyProcessed_ThrowsEstadoInvalidoException` | `UpdateEstadoEvento_WhenAlreadyProcessed_Returns409Conflict` |
| 6b. Transicion invalida | `409 Conflict` | `UpdateEstadoEventoAsync_WhenInvalidTransition_ThrowsTransicionInvalidaException` | `UpdateEstadoEvento_WhenInvalidTransition_Returns409Conflict` |
| 6c. Error persistencia | `500 Internal Server Error` | `UpdateEstadoEventoAsync_WhenRepositoryThrows_ThrowsException` | `UpdateEstadoEvento_WhenDatabaseError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
