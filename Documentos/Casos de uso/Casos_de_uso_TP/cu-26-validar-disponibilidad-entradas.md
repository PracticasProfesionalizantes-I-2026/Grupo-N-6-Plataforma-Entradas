# Caso de Uso: Validar disponibilidad de entradas

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-26 |
| **Nombre** | Validar disponibilidad de entradas |
| **Actor Principal** | Sistema |
| **Alcance / Nivel** | Sistema; subfunción |
| **Stakeholders e intereses** | Sistema → garantizar integridad de stock; Usuario → recibir confirmación de disponibilidad real |
| **Disparador (Trigger)** | El sistema recibe una solicitud de compra/reserva que requiere verificar stock (invocado desde CU-10) |
| **Prioridad / Frecuencia** | Alta; invocado en cada intento de compra |
| **Reglas de negocio relacionadas** | RN-01 (DNI único por evento), RN-03 (incremento dinámico > 80%) |

---

### 1. BREVE DESCRIPCIÓN
Validación interna que verifica en tiempo real si hay entradas disponibles en el sector solicitado antes de proceder con la reserva temporal.

### 2. PRECONDICIONES
1. El evento debe estar "Aprobado" y activo.
2. El sector debe existir y pertenecer al evento.
3. La cantidad solicitada debe ser > 0.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200 interno)
1. El Sistema (Capa de Negocio) recibe parámetros: eventoId, sectorId, cantidad.
2. La **Capa de Negocio** consulta stock actual del sector (capacidad - entradas vendidas - entradas reservadas temporalmente).
3. Si stock >= cantidad, la validación es exitosa.
4. El Sistema retorna resultado positivo al flujo llamante (CU-10).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. Stock insuficiente (Excepción de dominio):**
  1. Si en el Paso 2 el stock disponible < cantidad solicitada.
  2. La **Capa de Negocio** lanza `StockInsuficienteException` con stock actual.
  3. El flujo llamante (CU-10) maneja la excepción y retorna **409 Conflict** al usuario.

* **2b. Sector no disponible (Excepción de dominio):**
  1. Si en el Paso 2 el sector no existe, no pertenece al evento, o evento no está "Aprobado".
  2. La **Capa de Negocio** lanza `SectorNoDisponibleException`.
  3. El flujo llamante retorna **404 Not Found** o **409 Conflict**.

* **3a. Error de concurrencia (Excepción de dominio):**
  1. Si en el Paso 2 ocurre condición de carrera (stock cambia entre lectura y reserva).
  2. La **Capa de Negocio** detecta conflicto y lanza `ConcurrenciaStockException`.
  3. El flujo llamante reintenta o retorna **409 Conflict**.

* **4a. Error técnico (Excepción no controlada):**
  1. Si en el Paso 2 falla la consulta a BD.
  2. El Sistema lanza excepción técnica.
  3. El flujo llamante retorna **500 Internal Server Error**.

### 5. SUB-VARIACIONES (opcional)
1. Validación simple (solo lectura) vs. validación con reserva atómica (transacción).
2. Considera RN-03: precio puede haber cambiado por ocupación > 80%.

### 6. POSTCONDICIONES
1. Resultado de validación retornado al caso de uso llamante.
2. No hay cambio de estado persistente (solo lectura); la reserva temporal se hace en CU-10 paso 5.

---

## Anexo: matrices de referencia

### Códigos HTTP usados (en contexto del CU llamante)

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Validación exitosa (interno, no HTTP directo). |
| `409` | Conflict | Stock insuficiente, sector no disponible, concurrencia (manejado por CU-10). |
| `404` | Not Found | Sector/evento inexistente (manejado por CU-10). |
| `500` | Internal Server Error | Error técnico en validación (manejado por CU-10). |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación):** N/A (invocación interna).
- **Verificación (Negocio):** stock real vs. solicitado, existencia sector/evento, estado evento, concurrencia.

### Matriz de trazabilidad CU-26 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | Éxito (interno) | `ValidarDisponibilidad_WithStock_ReturnsTrue` | (probado vía CU-10 integración) |
| 2a. Stock insuficiente | `StockInsuficienteException` | `ValidarDisponibilidad_WhenStockLow_ThrowsException` | `ComprarEntradas_WhenNoStock_Returns409Conflict` |
| 2b. Sector no disponible | `SectorNoDisponibleException` | `ValidarDisponibilidad_WhenSectorInvalid_ThrowsException` | `ComprarEntradas_WhenInvalidSector_Returns404NotFound` |
| 3a. Concurrencia | `ConcurrenciaStockException` | `ValidarDisponibilidad_WhenConcurrency_ThrowsException` | `ComprarEntradas_WhenConcurrency_Returns409Conflict` |
| 4a. Error técnico | Excepción técnica | `ValidarDisponibilidad_WhenDbFails_ThrowsException` | `ComprarEntradas_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Al ser subfunción interna, sus tests son principalmente unitarios; la integración se verifica vía CU-10. Los tests se ejecutan con `dotnet test`.
