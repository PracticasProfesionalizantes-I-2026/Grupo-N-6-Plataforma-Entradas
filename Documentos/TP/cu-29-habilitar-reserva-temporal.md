# Caso de Uso: Habilitar reserva temporal de entradas

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-29 |
| **Nombre** | Habilitar reserva temporal de entradas |
| **Actor Principal** | Sistema |
| **Alcance / Nivel** | Sistema; subfunción |
| **Stakeholders e intereses** | Sistema → reservar stock durante proceso de compra; Usuario → tener entradas garantizadas mientras completa datos |
| **Disparador (Trigger)** | El sistema valida disponibilidad y límites, y procede a reservar temporalmente las entradas (invocado desde CU-10 paso 5) |
| **Prioridad / Frecuencia** | Alta; invocado en cada compra válida |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Operación interna que marca las entradas seleccionadas como "Reservadas temporalmente" por un tiempo límite (ej. 10 minutos), reduciendo el stock disponible para otros usuarios mientras se completa el proceso de compra.

### 2. PRECONDICIONES
1. Validaciones previas exitosas: disponibilidad (CU-26), límite (CU-27), DNIs (CU-28).
2. Sector, evento, cantidad y lista de DNIs confirmados.
3. Configuración de tiempo de expiración de reserva (ej. 10 min).

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200 interno)
1. El Sistema (Capa de Negocio) recibe: eventoId, sectorId, cantidad, listaDnis[], userId, expiracionReserva.
2. La **Capa de Negocio** crea registros de "ReservaTemporal" en BD (uno por entrada) con: entradaId (pre-generado o placeholder), dni, userId, estado="Reservada", expiraEn.
3. La **Capa de Negocio** decrementa stock disponible del sector (capacidad - vendidas - reservadas).
4. El Sistema retorna al flujo llamante (CU-10) los IDs de reserva temporal y tiempo restante.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. Stock cambió (condición de carrera) (Excepción de dominio):**
  1. Si en el Paso 3 al decrementar stock, este ya no es suficiente (otra compra simultánea).
  2. La **Capa de Negocio** detecta conflicto (optimistic locking o check) y lanza `StockInsuficienteException`.
  3. Rollback de reservas creadas en Paso 2.
  4. El flujo llamante (CU-10) retorna **409 Conflict** al usuario.

* **3a. Error creando reservas (Excepción técnica):**
  1. Si en el Paso 2 falla la inserción en BD.
  2. El Sistema lanza excepción técnica.
  3. El flujo llamante retorna **500 Internal Server Error**.

* **4a. Expiración de reserva (proceso separado):**
  1. Job programado (background) detecta reservas con expiraEn < ahora.
  2. Libera stock (incrementa disponible) y marca reservas como "Expiradas".
  4. No retorna al usuario (proceso asíncrono).

### 5. SUB-VARIACIONES (opcional)
1. Reserva atómica en transacción vs. compensación (saga).
2. Tiempo de reserva configurable por evento o global.

### 6. POSTCONDICIONES
1. Entradas marcadas como "Reservadas temporalmente" en BD.
2. Stock disponible del sector reducido.
3. Temporizador de expiración iniciado (job o TTL).
4. Usuario tiene ventana de tiempo para completar compra (ingresar pago, confirmar).

---

## Anexo: matrices de referencia

### Códigos HTTP usados (en contexto del CU llamante)

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Reserva temporal creada (interno). |
| `409` | Conflict | Stock insuficiente por carrera (manejado por CU-10). |
| `500` | Internal Server Error | Error técnico creando reserva (manejado por CU-10). |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación):** N/A (invocación interna).
- **Verificación (Negocio):** stock suficiente atómico, creación de reservas, decremento stock.

### Matriz de trazabilidad CU-29 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | Éxito (interno) | `HabilitarReservaTemporal_WithValidData_CreatesReservations` | (probado vía CU-10 integración) |
| 2a. Carrera stock | `StockInsuficienteException` | `HabilitarReservaTemporal_WhenConcurrency_ThrowsException` | `ComprarEntradas_WhenConcurrencyOnReserve_Returns409Conflict` |
| 3a. Error BD | Excepción técnica | `HabilitarReservaTemporal_WhenDbFails_ThrowsException` | `ComprarEntradas_WhenDbFailsOnReserve_Returns500InternalServerError` |
| 4a. Expiración (job) | N/A (async) | `LiberarReservasExpiradas_Job_ReleasesStock` | (test de job separado) |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Al ser subfunción interna, sus tests son principalmente unitarios; la integración se verifica vía CU-10. Los tests se ejecutan con `dotnet test`.
