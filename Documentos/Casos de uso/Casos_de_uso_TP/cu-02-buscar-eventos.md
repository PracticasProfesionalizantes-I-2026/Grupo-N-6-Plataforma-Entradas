# Caso de Uso: Buscar Eventos

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Reglas de negocio **implementadas** en el codigo; cada caso borde cuenta con su test
> unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-02 |
| **Nombre** | Buscar Eventos |
| **Actor Principal** | Usuario Visitante |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario Visitante -> encontrar eventos por criterio; Sistema -> filtrar eficientemente |
| **Disparador (Trigger)** | El usuario visitante ingresa un criterio de busqueda y selecciona "Buscar" |
| **Prioridad / Frecuencia** | Alta; alta frecuencia |
| **Reglas de negocio relacionadas** | Los resultados deben corresponder al criterio ingresado por el usuario |

---

### 1. BREVE DESCRIPCION
Permite al usuario visitante buscar eventos disponibles en la plataforma mediante criterios de busqueda.

### 2. PRECONDICIONES
1. El sistema debe estar en funcionamiento y con la Capa de Persistencia accesible.
2. El usuario debe encontrarse en la seccion de eventos disponibles.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envia una peticion al endpoint `GET /api/eventos?search={criterio}`.
2. La **Capa de Presentacion** (`EventosController.BuscarEventos`) valida que el parametro de busqueda este presente.
3. La **Capa de Negocio** (`EventoService.BuscarEventosAsync`) filtra los eventos aprobados por el criterio (nombre, descripcion, lugar).
4. La **Capa de Persistencia** ejecuta la consulta filtrada sobre la tabla `Eventos`.
5. El Sistema devuelve un codigo **200 OK** con la lista de eventos que coinciden (ID, nombre, fecha, lugar).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. Criterio de busqueda vacio (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el parametro `search` no se envia o llega vacio.
  2. El Sistema (Capa de Presentacion) rechaza la peticion por validacion de esquema.
  3. El Sistema devuelve un codigo **400 Bad Request** con mensaje: "Debe ingresar un criterio de busqueda". Fin del caso de uso.

* **3a. No se encuentran eventos coincidentes (HTTP 200 OK):**
  1. Si en el Paso 3 la busqueda no retorna resultados.
  2. El Sistema (Capa de Negocio) devuelve una lista vacia.
  3. El Sistema devuelve un codigo **200 OK** con lista vacia y mensaje: "No se encontraron eventos que coincidan con el criterio de busqueda". Fin del caso de uso.

* **3b. Error al recuperar los resultados (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 o 4 ocurre un error tecnico al acceder a la base de datos.
  2. El Sistema interrumpe la operacion y registra el error como no controlado.
  3. El Sistema devuelve un codigo **500 Internal Server Error** con mensaje: "No fue posible realizar la busqueda. Intente nuevamente mas tarde". Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El actor puede realizar la busqueda desde el panel web, desde la coleccion de Bruno (`GET BuscarEventos.bru`) o desde un cliente HTTP (Postman, Swagger/Scalar).
2. El criterio de busqueda puede ser parcial (coincidencia por contenido) o exacto.
3. En todas las variantes el endpoint y el resultado (`200 OK` con lista filtrada) son identicos.

### 6. POSTCONDICIONES
1. Se muestran al usuario los eventos que coinciden con el criterio de busqueda.
2. Si no existen coincidencias, se informa dicha situacion con lista vacia.

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Busqueda exitosa (lista con datos o vacia). |
| `400` | Bad Request | Criterio de busqueda vacio o invalido. |
| `500` | Internal Server Error | Error tecnico no controlado durante la busqueda en base de datos. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** Verificacion de que el parametro `search` este presente y no vacio (model binding + validacion de query string).
- **Verificacion (Negocio, -> 200/500):** La Capa de Negocio filtra solo eventos con estado "Aprobado" y aplica el criterio de busqueda. Los errores tecnicos en persistencia resultan en 500.

### Matriz de trazabilidad CU-02 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `BuscarEventosAsync_WithValidCriteria_ReturnsMatchingEventos` | `BuscarEventos_WithValidCriteria_Returns200AndResults` |
| 2a. Criterio vacio | `400 Bad Request` | -- (validacion de esquema en Presentacion) | `BuscarEventos_WithEmptyCriteria_Returns400BadRequest` |
| 3a. Sin coincidencias | `200 OK` (lista vacia) | `BuscarEventosAsync_WhenNoMatches_ReturnsEmptyList` | `BuscarEventos_WhenNoMatches_Returns200EmptyList` |
| 3b. Error al buscar | `500 Internal Server Error` | `BuscarEventosAsync_WhenRepositoryThrows_ThrowsException` | `BuscarEventos_WhenDatabaseError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
