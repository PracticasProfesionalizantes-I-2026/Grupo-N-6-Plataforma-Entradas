# Caso de Uso: Solicitar Devolucion

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Regla de negocio RN-02 (reembolso 80% y reposicion stock) **implementada** en el codigo; cada caso borde cuenta con su test unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-15 |
| **Nombre** | Solicitar Devolucion |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario Registrado -> recuperar dinero; Sistema -> procesar reembolso y stock |
| **Disparador (Trigger)** | El Usuario Registrado selecciona "Solicitar devolucion" de una entrada adquirida |
| **Prioridad / Frecuencia** | Media; baja frecuencia |
| **Reglas de negocio relacionadas** | RN-02 (el sistema aplicara un reembolso del 80% y repondra automaticamente la entrada al stock disponible) |

---

### 1. BREVE DESCRIPCION
Permite a un Usuario Registrado solicitar la devolucion de una entrada adquirida, recibiendo un reembolso del 80% y liberando la entrada al stock.

### 2. PRECONDICIONES
1. El Usuario Registrado posee una entrada activa para el evento.
2. El actor debe poseer un estado de autenticacion activo (Token JWT valido).
3. La entrada debe estar en estado "Activa" (no utilizada, no devuelta, no vencida).

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envia una peticion al endpoint `GET /api/mis-entradas` para listar sus entradas activas.
2. La **Capa de Presentacion** (`EntradasController.GetMisEntradas`) retorna entradas del usuario.
3. El Actor envia una peticion al endpoint `POST /api/devoluciones/{idEntrada}`.
4. La **Capa de Presentacion** (`DevolucionesController.CreateDevolucion`) valida que la entrada exista y pertenezca al usuario.
5. La **Capa de Negocio** (`DevolucionService.CreateDevolucionAsync`):
   a. Verifica que la entrada sea elegible para devolucion (estado activa, evento no iniciado).
   b. Registra la devolucion en estado "Solicitada".
   c. Aplica reembolso del 80% del valor pagado (RN-02).
   d. Repone la entrada al stock disponible del sector (RN-02).
   e. Actualiza estado de la entrada a "Devuelta".
6. El Sistema devuelve un codigo **200 OK** con detalle: monto reembolsado, entrada repuesta al stock.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **3a. Entrada inexistente o no pertenece al usuario (HTTP 404 Not Found):**
  1. Si en el Paso 4 la entrada no existe o no pertenece al usuario autenticado.
  2. El Sistema (Capa de Presentacion / Negocio) no encuentra la entrada.
  3. El Sistema devuelve **404 Not Found**: "Entrada no encontrada". Fin del caso de uso.

* **5a. Entrada no valida para devolucion (HTTP 409 Conflict):**
  1. Si en el Paso 5a la entrada no cumple condiciones (ya utilizada, ya devuelta, evento iniciado/terminado, fuera de plazo).
  2. La Capa de Negocio lanza `DevolucionNoValidaException`.
  3. El Sistema devuelve **409 Conflict**: "La devolucion no puede realizarse para esta entrada". Fin del caso de uso.

* **5b. Error al procesar reembolso (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5c ocurre error tecnico al procesar el reembolso (pasarela de pagos, BD).
  2. El Sistema interrumpe y registra error no controlado.
  3. El Sistema devuelve **500 Internal Server Error**: "No fue posible procesar la devolucion. Intente nuevamente mas tarde". Fin del caso de uso.

* **5c. Error al reponer stock (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5d la Capa de Persistencia falla al actualizar stock del sector.
  2. El Sistema interrumpe y registra error no controlado.
  3. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El actor puede solicitar devolucion desde panel web, coleccion Bruno (`POST Devolucion.bru`), Postman, Swagger/Scalar.
2. El plazo para devolucion puede ser configurable (ej. hasta 24h antes del evento).
3. En todas las variantes el endpoint y resultado (`200 OK` con detalle reembolso) son identicos.

### 6. POSTCONDICIONES
1. La devolucion queda registrada en tabla `Devoluciones` con estado "Completada".
2. Se aplica reembolso del 80% del valor original (RN-02).
3. La entrada vuelve al stock disponible del sector correspondiente (RN-02).
4. La entrada original pasa a estado "Devuelta" (no reutilizable).

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Devolucion procesada exitosamente con reembolso y stock actualizado. |
| `404` | Not Found | Entrada inexistente o no pertenece al usuario. |
| `409` | Conflict | Entrada no elegible para devolucion (ya usada, devuelta, evento iniciado). |
| `500` | Internal Server Error | Error tecnico en reembolso o actualizacion de stock. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** No aplica parametros de entrada complejos (solo path parameter `idEntrada` validado por routing).
- **Verificacion (Negocio, -> 404/409/500):** RN-02 (elegibilidad: entrada activa, evento no iniciado, dentro de plazo) -> 409; propiedad de entrada -> 404; procesamiento reembolso y stock -> 500.

### Matriz de trazabilidad CU-15 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `CreateDevolucionAsync_WithValidEntrada_ReturnsDevolucionConReembolso` | `CreateDevolucion_WithValidEntrada_Returns200OK` |
| 3a. Entrada no encontrada | `404 Not Found` | `CreateDevolucionAsync_WhenEntradaNotFound_ThrowsEntradaNotFoundException` | `CreateDevolucion_WhenEntradaNotFound_Returns404NotFound` |
| 5a. Entrada no valida | `409 Conflict` | `CreateDevolucionAsync_WhenEntradaNotEligible_ThrowsDevolucionNoValidaException` | `CreateDevolucion_WhenEntradaNotEligible_Returns409Conflict` |
| 5b. Error reembolso | `500 Internal Server Error` | `CreateDevolucionAsync_WhenRefundFails_ThrowsException` | `CreateDevolucion_WhenRefundError_Returns500InternalServerError` |
| 5c. Error stock | `500 Internal Server Error` | `CreateDevolucionAsync_WhenStockUpdateFails_ThrowsException` | `CreateDevolucion_WhenStockError_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
