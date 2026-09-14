# Caso de Uso: Comprar Entradas

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Reglas de negocio RN-01 (DNI unico por evento), RN-03 (incremento dinamico de precios) **implementadas** en el codigo; cada caso borde cuenta con su test unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-10 |
| **Nombre** | Comprar Entradas |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario Registrado -> adquirir entradas; Sistema -> validar disponibilidad y stock; Evento -> actualizar ocupacion |
| **Disparador (Trigger)** | El Usuario Registrado selecciona "Comprar entradas" para un evento disponible |
| **Prioridad / Frecuencia** | Alta; alta frecuencia |
| **Reglas de negocio relacionadas** | RN-01 (no se permitira registrar DNIs duplicados para un mismo evento); RN-03 (incremento automatico del 20% cuando aforo > 80%) |

---

### 1. BREVE DESCRIPCION
Permite a un Usuario Registrado comprar entradas para un evento disponible, seleccionando sector, cantidad e ingresando DNI por cada entrada.

### 2. PRECONDICIONES
1. El evento se encuentra aprobado y dispone de entradas disponibles.
2. El actor debe poseer un estado de autenticacion activo (Token JWT valido).
3. El usuario debe estar registrado en la plataforma.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 201)
1. El Actor envia una peticion al endpoint `GET /api/eventos/{id}/sectores` para ver sectores disponibles.
2. La **Capa de Presentacion** (`EventosController.GetSectores`) retorna sectores con capacidad y precio (aplicando RN-03 si corresponde).
3. El Actor envia una peticion al endpoint `POST /api/compras` con JSON: `{eventoId, sectorId, cantidad, dnis[]}`.
4. La **Capa de Presentacion** (`ComprasController.CreateCompra`) valida esquema del JSON y campos requeridos.
5. La **Capa de Negocio** (`CompraService.CreateCompraAsync`):
   a. Valida disponibilidad en el sector (CU-26: `ValidarDisponibilidadAsync`).
   b. Valida limite maximo de compra por usuario (CU-27: `ValidarLimiteMaximoAsync`).
   c. Reserva temporalmente las entradas (CU-29: `HabilitarReservaTemporalAsync`).
   d. Valida DNIs unicos para el evento (RN-01 / CU-28: `ValidarDnisDuplicadosAsync`).
   e. Registra la compra y actualiza stock en la **Capa de Persistencia**.
6. El Sistema devuelve un codigo **201 Created** con el comprobante (ID compra, entradas, total, QR).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **3a. JSON invalido o ilegible (HTTP 400 Bad Request):**
  1. Si en el Paso 3 el cuerpo no es JSON valido.
  2. El Sistema (Capa de Presentacion / model binding) rechaza por error de esquema.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **4a. Datos obligatorios faltantes (HTTP 400 Bad Request):**
  1. Si en el Paso 4 faltan `eventoId`, `sectorId`, `cantidad` o `dnis[]`.
  2. El Sistema (Capa de Presentacion) rechaza por `ModelState.IsValid == false`.
  3. El Sistema devuelve **400 Bad Request** detallando campo faltante. Fin del caso de uso.

* **5a. Evento no aprobado o inexistente (HTTP 404 Not Found):**
  1. Si en el Paso 5 el evento no existe o no esta "Aprobado".
  2. La Capa de Negocio no encuentra la entidad o estado invalido.
  3. El Sistema devuelve **404 Not Found**: "Evento no disponible para compra". Fin del caso de uso.

* **5b. Sector sin disponibilidad (HTTP 409 Conflict):**
  1. Si en el Paso 5a la validacion de disponibilidad (CU-26) falla (stock insuficiente).
  2. La Capa de Negocio lanza `SinDisponibilidadException`.
  3. El Sistema devuelve **409 Conflict**: "No hay entradas disponibles en el sector seleccionado". Fin del caso de uso.

* **5c. Limite maximo de compra excedido (HTTP 409 Conflict):**
  1. Si en el Paso 5b la validacion de limite (CU-27) detecta exceso de entradas por usuario.
  2. La Capa de Negocio lanza `LimiteCompraExcedidoException`.
  3. El Sistema devuelve **409 Conflict**: "Supera el limite maximo de entradas por compra". Fin del caso de uso.

* **5d. DNI duplicado para el evento (HTTP 409 Conflict):**
  1. Si en el Paso 5d la validacion RN-01 / CU-28 detecta DNI ya registrado en el evento.
  2. La Capa de Negocio lanza `DniDuplicadoException`.
  3. El Sistema devuelve **409 Conflict**: "El DNI ingresado ya posee una entrada para este evento". Fin del caso de uso.

* **5e. Reserva temporal expirada (HTTP 409 Conflict):**
  1. Si en el Paso 5c la reserva temporal (CU-29) expiro antes de confirmar.
  2. La Capa de Negocio libera la reserva y lanza `ReservaExpiradaException`.
  3. El Sistema devuelve **409 Conflict**: "La reserva temporal ha expirado. Intente nuevamente". Fin del caso de uso.

* **5f. Error interno en la persistencia (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5e la Capa de Persistencia falla al guardar.
  2. El Sistema interrumpe y registra error no controlado.
  3. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El actor puede comprar desde panel web, coleccion Bruno (`POST ComprarEntradas.bru`), Postman, Swagger/Scalar.
2. La cantidad de entradas puede variar (1 a N, segun limite CU-27).
3. En todas las variantes el esquema y resultado (`201 Created` con comprobante) son identicos.

### 6. POSTCONDICIONES
1. La compra queda registrada persistentemente en tablas `Compras` y `Entradas`.
2. Las entradas quedan asociadas a los DNIs ingresados.
3. El stock del sector se actualiza (disminuye segun cantidad comprada).
4. Se genera comprobante con codigo QR para validacion en ingreso.

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `201` | Created | Compra registrada exitosamente con comprobante generado. |
| `400` | Bad Request | JSON invalido, campos faltantes o invalidos en el DTO. |
| `404` | Not Found | Evento inexistente o no aprobado. |
| `409` | Conflict | Violacion RN-01 (DNI duplicado), sin stock, limite excedido, reserva expirada. |
| `500` | Internal Server Error | Error tecnico no controlado durante la persistencia. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** Esquema JSON, campos requeridos, tipos de datos, longitudes (DTO `CompraCreateDTO` + `ModelState`).
- **Verificacion (Negocio, -> 404/409/500):** RN-01 (DNI unico via `ValidarDnisDuplicadosAsync`), RN-03 (precio dinamico en `GetSectores`), disponibilidad real (CU-26), limite compra (CU-27), reserva temporal (CU-29). Defensa en profundidad: aunque Presentacion valide, Negocio re-verifica.

### Matriz de trazabilidad CU-10 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `201 Created` | `CreateCompraAsync_WithValidData_ReturnsCompraConComprobante` | `CreateCompra_WithValidData_Returns201Created` |
| 3a. JSON invalido | `400 Bad Request` | -- (model binding) | `CreateCompra_WithInvalidJson_Returns400BadRequest` |
| 4a. Campos faltantes | `400 Bad Request` | -- (validacion DTO) | `CreateCompra_WithMissingFields_Returns400BadRequest` |
| 5a. Evento no aprobado | `404 Not Found` | `CreateCompraAsync_WhenEventoNotApproved_ThrowsEventoNotFoundException` | `CreateCompra_WhenEventoNotApproved_Returns404NotFound` |
| 5b. Sin disponibilidad | `409 Conflict` | `CreateCompraAsync_WhenNoStock_ThrowsSinDisponibilidadException` | `CreateCompra_WhenNoStock_Returns409Conflict` |
| 5c. Limite excedido | `409 Conflict` | `CreateCompraAsync_WhenLimitExceeded_ThrowsLimiteCompraExcedidoException` | `CreateCompra_WhenLimitExceeded_Returns409Conflict` |
| 5d. DNI duplicado (RN-01) | `409 Conflict` | `CreateCompraAsync_WhenDniDuplicate_ThrowsDniDuplicadoException` | `CreateCompra_WhenDniDuplicate_Returns409Conflict` |
| 5e. Reserva expirada | `409 Conflict` | `CreateCompraAsync_WhenReservaExpired_ThrowsReservaExpiradaException` | `CreateCompra_WhenReservaExpired_Returns409Conflict` |
| 5f. Error persistencia | `500 Internal Server Error` | `CreateCompraAsync_WhenRepositoryThrows_ThrowsException` | `CreateCompra_WhenDatabaseError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
