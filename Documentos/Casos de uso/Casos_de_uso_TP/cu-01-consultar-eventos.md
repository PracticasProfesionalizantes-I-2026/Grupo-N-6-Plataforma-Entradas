# Caso de Uso: Consultar Eventos

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Reglas de negocio **implementadas** en el codigo; cada caso borde cuenta con su test
> unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-01 |
| **Nombre** | Consultar Eventos |
| **Actor Principal** | Usuario Visitante |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario Visitante -> visualizar eventos disponibles; Sistema -> exponer eventos aprobados |
| **Disparador (Trigger)** | El usuario visitante selecciona la opcion "Eventos" desde la plataforma |
| **Prioridad / Frecuencia** | Alta; alta frecuencia |
| **Reglas de negocio relacionadas** | Solo los eventos que hayan sido aprobados por un administrador podran estar disponibles para los usuarios de la plataforma |

---

### 1. BREVE DESCRIPCION
Permite al usuario visitante consultar los eventos disponibles en la plataforma y acceder a la informacion basica de cada uno.

### 2. PRECONDICIONES
1. El sistema debe estar en funcionamiento y con la Capa de Persistencia accesible (base de datos inicializada).
2. El usuario puede acceder a la lista de eventos disponibles sin necesidad de estar autenticado.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envia una peticion al endpoint `GET /api/eventos`.
2. La **Capa de Presentacion** (`EventosController.GetEventos`) recibe la peticion.
3. La **Capa de Negocio** (`EventoService.GetEventosDisponiblesAsync`) consulta los eventos con estado "Aprobado".
4. La **Capa de Persistencia** recupera los registros de la tabla `Eventos` filtrados por estado aprobado.
5. El Sistema devuelve un codigo **200 OK** con la lista de eventos (ID, nombre, fecha, lugar, estado).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **3a. No existen eventos disponibles (HTTP 200 OK):**
  1. Si en el Paso 3 la consulta no retorna ningun registro (no hay eventos aprobados).
  2. El Sistema (Capa de Negocio) devuelve una lista vacia.
  3. El Sistema devuelve un codigo **200 OK** con lista vacia y mensaje informativo: "No hay eventos disponibles para consultar". Fin del caso de uso.

* **3b. Error al recuperar los eventos (HTTP 500 Internal Server Error):**
  1. Si en el Paso 3 o 4 ocurre un error tecnico al acceder a la base de datos (conexion, timeout, corrupcion).
  2. El Sistema interrumpe la operacion y registra el error como no controlado.
  3. El Sistema devuelve un codigo **500 Internal Server Error** con mensaje: "No fue posible obtener los eventos disponibles. Intente nuevamente mas tarde". Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El actor puede realizar la consulta desde el panel web, desde la coleccion de Bruno (`GET Eventos.bru`) o desde un cliente HTTP (Postman, Swagger/Scalar).
2. En todas las variantes el endpoint y el resultado (`200 OK` con lista de eventos) son identicos.

### 6. POSTCONDICIONES
1. El usuario puede visualizar los eventos disponibles en la plataforma.
2. Solo se exponen eventos con estado "Aprobado" (impacto en visibilidad para otros actores).

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Consulta exitosa de eventos (lista con datos o vacia). |
| `500` | Internal Server Error | Error tecnico no controlado durante la consulta a la base de datos. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** No aplica en este caso de uso (operacion de solo lectura sin cuerpo de peticion).
- **Verificacion (Negocio, -> 200/500):** La Capa de Negocio verifica que solo se retornen eventos con estado "Aprobado" (regla de negocio implicita). Los errores tecnicos en persistencia resultan en 500.

### Matriz de trazabilidad CU-01 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `GetEventosDisponiblesAsync_ReturnsListOfEventos` | `GetEventos_ReturnsSuccessAndListOfEventos` |
| 3a. Sin eventos disponibles | `200 OK` (lista vacia) | `GetEventosDisponiblesAsync_WhenNoEventos_ReturnsEmptyList` | `GetEventos_WhenNoEventos_ReturnsEmptyList` |
| 3b. Error al recuperar eventos | `500 Internal Server Error` | `GetEventosDisponiblesAsync_WhenRepositoryThrows_ThrowsException` | `GetEventos_WhenDatabaseError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
