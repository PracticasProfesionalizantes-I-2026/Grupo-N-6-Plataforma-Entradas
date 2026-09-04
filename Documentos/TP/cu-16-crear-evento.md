# Caso de Uso: Crear Evento

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Reglas de negocio (estado "Pendiente de aprobacion", solo "Aprobado" visible) **implementadas** en el codigo; cada caso borde cuenta con su test unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-16 |
| **Nombre** | Crear Evento |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario Registrado -> crear evento; Administrador -> revisar y aprobar; Sistema -> validar y almacenar |
| **Disparador (Trigger)** | El usuario registrado selecciona "Crear Evento" desde la plataforma |
| **Prioridad / Frecuencia** | Media; media frecuencia |
| **Reglas de negocio relacionadas** | Todo evento registrado quedara con estado "Pendiente de aprobacion" hasta ser revisado por un administrador; Solo los eventos con estado "Aprobado" podran ser visibles para los usuarios visitantes y estar disponibles para la venta de entradas |

---

### 1. BREVE DESCRIPCION
Permite a un usuario registrado crear un evento ingresando la informacion requerida para su posterior revision y aprobacion.

### 2. PRECONDICIONES
1. El usuario inicio sesion en la plataforma (Token JWT valido).
2. El sistema debe estar en funcionamiento con Capa de Persistencia accesible.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 201)
1. El Actor envia una peticion al endpoint `GET /api/eventos/formulario` (opcional, para obtener estructura).
2. El Actor envia una peticion al endpoint `POST /api/eventos` con JSON: `{nombre, descripcion, fechaInicio, fechaFin, lugar, direccion, sectores[]}` donde cada sector tiene `{nombre, capacidad, precioBase}`.
3. La **Capa de Presentacion** (`EventosController.CreateEvento`) valida esquema JSON y campos requeridos (DTO `EventoCreateDTO`).
4. La **Capa de Negocio** (`EventoService.CreateEventoAsync`):
   a. Valida logica: fechaInicio futura, fechaFin >= fechaInicio, capacidad > 0, precioBase >= 0.
   b. Crea entidad `Evento` con estado "Pendiente de aprobacion".
   c. Asocia evento al usuario creador (ID desde JWT).
   d. Crea sectores asociados con capacidad y precio base.
   e. Persiste en **Capa de Persistencia** (tablas `Eventos`, `Sectores`).
5. El Sistema devuelve un codigo **201 Created** con el evento creado (ID, nombre, estado "Pendiente de aprobacion", sectores).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. JSON invalido o ilegible (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el cuerpo no es JSON valido.
  2. El Sistema (Capa de Presentacion / model binding) rechaza por error de esquema.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Campos obligatorios faltantes (HTTP 400 Bad Request):**
  1. Si en el Paso 3 faltan `nombre`, `fechaInicio`, `lugar` o `sectores[]`.
  2. El Sistema (Capa de Presentacion) rechaza por `ModelState.IsValid == false`.
  3. El Sistema devuelve **400 Bad Request** detallando campo faltante. Fin del caso de uso.

* **4a. Datos invalidos - fecha pasada (HTTP 400 Bad Request):**
  1. Si en el Paso 4a `fechaInicio` es anterior a hoy.
  2. La Capa de Negocio lanza `ValidacionException`: "La fecha de inicio debe ser futura".
  3. El Sistema devuelve **400 Bad Request** con mensaje de validacion. Fin del caso de uso.

* **4b. Datos invalidos - fecha fin anterior a inicio (HTTP 400 Bad Request):**
  1. Si en el Paso 4a `fechaFin` < `fechaInicio`.
  2. La Capa de Negocio lanza `ValidacionException`: "La fecha de fin no puede ser anterior a la de inicio".
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **4c. Datos invalidos - capacidad o precio negativo (HTTP 400 Bad Request):**
  1. Si en el Paso 4a algun sector tiene `capacidad <= 0` o `precioBase < 0`.
  2. La Capa de Negocio lanza `ValidacionException` con detalle del sector.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **4d. Cancelacion del registro (sin HTTP error):**
  1. Si el usuario decide cancelar antes de confirmar (no envia POST).
  2. El Sistema descarta la informacion ingresada y regresa al listado de eventos del usuario.
  3. El caso de uso finaliza sin error.

* **4e. Error al registrar el evento (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4e la Capa de Persistencia falla al guardar.
  2. El Sistema interrumpe y registra error no controlado.
  3. El Sistema devuelve **500 Internal Server Error**: "No fue posible registrar el evento. Intente nuevamente mas tarde". Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El actor puede crear desde panel web, coleccion Bruno (`POST CrearEvento.bru`), Postman, Swagger/Scalar.
2. Los sectores pueden ser 1 a N (minimo 1 sector requerido).
3. En todas las variantes el esquema y resultado (`201 Created` con estado "Pendiente de aprobacion") son identicos.

### 6. POSTCONDICIONES
1. El evento queda registrado en tabla `Eventos` con ID unico (GUID).
2. El evento queda asociado al usuario que lo creo (FK `UsuarioId`).
3. El evento se almacena con estado "Pendiente de aprobacion".
4. Los sectores quedan registrados en tabla `Sectores` vinculados al evento.
5. El evento queda disponible para revision por parte de un administrador (CU-21).

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `201` | Created | Evento registrado exitosamente con estado "Pendiente de aprobacion". |
| `400` | Bad Request | JSON invalido, campos faltantes, fechas invalidas, capacidad/precio invalidos. |
| `500` | Internal Server Error | Error tecnico no controlado durante la persistencia. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** Esquema JSON, campos requeridos `[Required]`, tipos, longitudes `[MaxLength]` (DTO `EventoCreateDTO`, `SectorCreateDTO` + `ModelState`).
- **Verificacion (Negocio, -> 400/500):** Logica de dominio: fechas validas (futuras, coherentes), capacidad > 0, precio >= 0, minimo 1 sector. Defensa en profundidad: Negocio re-verifica aunque Presentacion ya valide.

### Matriz de trazabilidad CU-16 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `201 Created` | `CreateEventoAsync_WithValidData_ReturnsCreatedEvento` | `CreateEvento_WithValidData_Returns201Created` |
| 2a. JSON invalido | `400 Bad Request` | -- (model binding) | `CreateEvento_WithInvalidJson_Returns400BadRequest` |
| 3a. Campos faltantes | `400 Bad Request` | -- (validacion DTO) | `CreateEvento_WithMissingFields_Returns400BadRequest` |
| 4a. Fecha pasada | `400 Bad Request` | `CreateEventoAsync_WhenFechaPasada_ThrowsValidacionException` | `CreateEvento_WhenFechaPasada_Returns400BadRequest` |
| 4b. Fecha fin < inicio | `400 Bad Request` | `CreateEventoAsync_WhenFechaFinAntesInicio_ThrowsValidacionException` | `CreateEvento_WhenFechaFinAntesInicio_Returns400BadRequest` |
| 4c. Capacidad/precio invalido | `400 Bad Request` | `CreateEventoAsync_WhenInvalidSectorData_ThrowsValidacionException` | `CreateEvento_WhenInvalidSectorData_Returns400BadRequest` |
| 4e. Error persistencia | `500 Internal Server Error` | `CreateEventoAsync_WhenRepositoryThrows_ThrowsException` | `CreateEvento_WhenDatabaseError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
