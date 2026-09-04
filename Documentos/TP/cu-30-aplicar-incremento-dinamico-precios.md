# Caso de Uso: Aplicar Incremento Dinamico de Precios

> Especificacion elaborada siguiendo la guia
> `GUIA-Especificacion-Casos-de-Uso.md` (seccion 3).
> Regla de negocio RN-03 (incremento automatico 20% cuando aforo > 80%) **implementada** en el codigo; cada caso borde cuenta con su test unitario e integracion (ver matriz de trazabilidad).

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-30 |
| **Nombre** | Aplicar Incremento Dinamico de Precios |
| **Actor Principal** | Sistema |
| **Alcance / Nivel** | Sistema; subfuncion |
| **Stakeholders e intereses** | Sistema -> optimizar ingresos; Usuarios -> precios actualizados |
| **Disparador (Trigger)** | El sistema detecta que el aforo vendido supera el 80% de la capacidad del sector |
| **Prioridad / Frecuencia** | Alta; automatica/continua (job programado o evento de dominio) |
| **Reglas de negocio relacionadas** | RN-03 (el sistema incrementara automaticamente en un 20% el precio base de un sector cuando el aforo vendido supere el 80% de su capacidad) |

---

### 1. BREVE DESCRIPCION
Permite aplicar automaticamente un incremento del 20% al precio de las entradas de un sector cuando el nivel de ocupacion supera el 80% de su capacidad.

### 2. PRECONDICIONES
1. El evento se encuentra activo (estado "Aprobado") y posee sectores con entradas disponibles.
2. El sistema de monitoreo de ocupacion esta operativo (background job / event-driven).
3. La Capa de Persistencia es accesible para leer ocupacion y actualizar precios.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200 Interno)
1. El **Sistema** (job programado `PrecioDinamicoJob` o evento `EntradaVendidaEvent`) ejecuta `PrecioDinamicoService.EvaluarYAplicarIncrementoAsync()`.
2. La **Capa de Negocio** (`PrecioDinamicoService`) itera sectores del evento activo:
   a. Calcula ocupacion actual: `entradasVendidas / capacidad * 100`.
   b. Si `ocupacion > 80%` Y precio actual == precioBase (no incrementado aun):
      i. Calcula nuevo precio: `precioBase * 1.20` (incremento 20% - RN-03).
      ii. Actualiza `Sector.precioActual` en **Capa de Persistencia**.
      iii. Registra auditoria: sectorId, precioAnterior, precioNuevo, fecha, trigger "ocupacion > 80%".
3. El Sistema finaliza el job/evento con exito (interno, sin respuesta HTTP publica).
   * Nota: Si se expone via API interna para testing, retorna **200 OK** con sectores actualizados.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. No se alcanza el nivel de ocupacion (sin cambio):**
  1. Si en el Paso 2b la ocupacion calculada <= 80%.
  2. El Sistema mantiene el precio base del sector (`precioActual == precioBase`).
  3. No hay modificacion en persistencia. Fin de la iteracion para ese sector.

* **2b. Precio ya incrementado (idempotencia):**
  1. Si en el Paso 2b la ocupacion > 80% PERO `precioActual > precioBase` (ya se aplico incremento).
  2. El Sistema no aplica doble incremento (idempotencia).
  3. Mantiene precio actual. Fin de la iteracion para ese sector.

* **2c. Error al leer ocupacion (HTTP 500 Interno):**
  1. Si en el Paso 2a falla la consulta de entradas vendidas / capacidad.
  2. El Sistema registra error y reintenta (politica de reintentos del job).
  3. Si falla persistentemente: alerta a administradores, log **500 Internal Server Error** interno.

* **2d. Error al actualizar precio (HTTP 500 Interno):**
  1. Si en el Paso 2b.ii la Capa de Persistencia falla al guardar `precioActual`.
  2. El Sistema registra error, no aplica incremento, reintenta en siguiente ejecucion.
  3. Log **500 Internal Server Error** interno.

### 5. SUB-VARIACIONES (opcional)
1. Trigger por job programado (ej. cada 5 min) vs. trigger por evento de dominio (`EntradaVendidaEvent`).
2. El porcentaje de incremento (20%) y umbral (80%) son configurables (appsettings).
3. Puede aplicar a eventos en curso (fechaInicio <= hoy <= fechaFin).

### 6. POSTCONDICIONES
1. El precio de las entradas del sector queda actualizado a `precioBase * 1.20` cuando corresponde (RN-03).
2. La auditoria registra el cambio para trazabilidad.
3. Las nuevas compras (CU-10) usaran el precio actualizado automaticamente.

---

## Anexo: matrices de referencia

### Codigos HTTP usados

| Codigo HTTP | Nombre Tecnico | Contexto de Aplicacion en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | (Interno/API testing) Evaluacion completada, precios actualizados si correspondia. |
| `500` | Internal Server Error | Error tecnico al leer ocupacion o persistir nuevo precio. |

### Nota: Validacion vs. Verificacion aplicada

- **Validacion (Presentacion, -> 400):** No aplica (proceso interno sin peticion HTTP externa).
- **Verificacion (Negocio, -> 200/500):** RN-03: calculo ocupacion > 80%, idempotencia (no doble incremento), persistencia de precioActual. Errores tecnicos -> 500 interno (job reintenta).

### Matriz de trazabilidad CU-30 -> Test

| Paso del CU | Excepcion / Codigo | Test unitario (BusinessLogic) | Test integracion (HTTP) |
| --- | --- | --- | --- |
| Flujo principal (incrementa) | `200 OK` (interno) | `EvaluarYAplicarIncrementoAsync_WhenOcupacionSupera80_UpdatesPrecio` | `EvaluarIncremento_WhenOcupacionSupera80_Returns200WithUpdatedSectores` |
| 2a. Ocupacion <= 80% | `200 OK` (sin cambios) | `EvaluarYAplicarIncrementoAsync_WhenOcupacionBajo80_KeepsPrecioBase` | `EvaluarIncremento_WhenOcupacionBajo80_Returns200NoChanges` |
| 2b. Ya incrementado (idempotencia) | `200 OK` (sin cambios) | `EvaluarYAplicarIncrementoAsync_WhenAlreadyIncremented_KeepsCurrentPrecio` | `EvaluarIncremento_WhenAlreadyIncremented_Returns200NoChanges` |
| 2c. Error lectura ocupacion | `500 Internal Server Error` | `EvaluarYAplicarIncrementoAsync_WhenOcupacionReadFails_ThrowsException` | `EvaluarIncremento_WhenOcupacionReadFails_Returns500InternalServerError` |
| 2d. Error actualizacion precio | `500 Internal Server Error` | `EvaluarYAplicarIncrementoAsync_WhenPrecioUpdateFails_ThrowsException` | `EvaluarIncremento_WhenPrecioUpdateFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test EntradApp.slnx`.
