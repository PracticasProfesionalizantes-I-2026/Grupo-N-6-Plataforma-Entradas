# Caso de Uso: Validar DNIs duplicados

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-28 |
| **Nombre** | Validar DNIs duplicados |
| **Actor Principal** | Sistema |
| **Alcance / Nivel** | Sistema; subfunción |
| **Stakeholders e intereses** | Sistema → garantizar RN-01 (DNI único por evento); Usuario → recibir error claro si DNI ya usado |
| **Disparador (Trigger)** | El sistema procesa la lista de DNIs ingresados en una compra (invocado desde CU-10 paso 6) |
| **Prioridad / Frecuencia** | Alta; invocado en cada compra con múltiples entradas |
| **Reglas de negocio relacionadas** | RN-01 (No se permitirá registrar DNIs duplicados para un mismo evento) |

---

### 1. BREVE DESCRIPCIÓN
Validación interna que verifica que ningún DNI de la lista ingresada en la compra ya esté registrado para ese mismo evento (ni en la compra actual ni en compras previas).
**Nota:** El DNI se usa para trazabilidad de compra (RN-01). El **código único alfanumérico** (generado por entrada en CU-10/CU-14) es el usado para validación externa en control de acceso; son campos distintos.

### 2. PRECONDICIONES
1. La compra ya pasó validaciones de disponibilidad y límite (CU-26, CU-27).
2. Se recibió lista de DNIs (uno por entrada solicitada).
3. El evento debe existir y estar "Aprobado".

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200 interno)
1. El Sistema (Capa de Negocio) recibe: eventoId, listaDnis[].
2. La **Capa de Negocio** verifica duplicados dentro de la propia lista (DNI repetido en la compra actual).
3. La **Capa de Negocio** consulta BD: ¿existe alguna entrada "Activa" o "Utilizada" para este evento con alguno de estos DNIs?
4. Si no hay duplicados internos ni en BD, validación exitosa.
5. El Sistema retorna resultado positivo al flujo llamante (CU-10).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. DNI duplicado en lista actual (Excepción de dominio):**
  1. Si en el Paso 2 la listaDnis contiene valores repetidos.
  2. La **Capa de Negocio** lanza `DniDuplicadoEnListaException` con el DNI repetido.
  3. El flujo llamante (CU-10) maneja y retorna **409 Conflict** al usuario (RN-01).

* **3a. DNI ya registrado en evento (Excepción de dominio):**
  1. Si en el Paso 3 algún DNI ya tiene entrada "Activa" o "Utilizada" para este evento.
  2. La **Capa de Negocio** lanza `DniYaRegistradoException` con el DNI y tipo entrada existente.
  3. El flujo llamante (CU-10) maneja y retorna **409 Conflict** al usuario (RN-01).

* **4a. Error técnico (Excepción no controlada):**
  1. Si en el Paso 3 falla la consulta a BD.
  2. El Sistema lanza excepción técnica.
  3. El flujo llamante retorna **500 Internal Server Error**.

### 5. SUB-VARIACIONES (opcional)
1. Validación en memoria (lista actual) vs. consulta BD (historial).
2. Case-insensitive y normalización (trim) de DNI antes de comparar.

### 6. POSTCONDICIONES
1. Resultado de validación retornado al caso de uso llamante.
2. No hay cambio de estado persistente (solo lectura); el registro de DNIs se hace en CU-10.
3. El **código único alfanumérico** (campo separado del DNI) se genera por entrada para validación externa en control de acceso (sistema externo).

---

## Anexo: matrices de referencia

### Códigos HTTP usados (en contexto del CU llamante)

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Validación exitosa (interno). |
| `409` | Conflict | DNI duplicado en lista o ya registrado en evento (RN-01, manejado por CU-10). |
| `500` | Internal Server Error | Error técnico en validación (manejado por CU-10). |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación):** N/A (invocación interna). La normalización trim() de DNI podría hacerse aquí.
- **Verificación (Negocio, → 409):** duplicados en lista actual, existencia en BD para mismo evento (RN-01).
- **Nota:** DNI ≠ código único alfanumérico. El DNI identifica al comprador (trazabilidad); el código único identifica la entrada física (validación acceso externo).

### Matriz de trazabilidad CU-28 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | Éxito (interno) | `ValidarDnisDuplicados_WithUniqueDnis_ReturnsTrue` | (probado vía CU-10 integración) |
| 2a. Duplicado en lista | `DniDuplicadoEnListaException` | `ValidarDnisDuplicados_WhenDuplicateInList_ThrowsException` | `ComprarEntradas_WhenDuplicateDniInList_Returns409Conflict` |
| 3a. Ya registrado en evento | `DniYaRegistradoException` | `ValidarDnisDuplicados_WhenDniExistsInEvent_ThrowsException` | `ComprarEntradas_WhenDniAlreadyInEvent_Returns409Conflict` |
| 4a. Error técnico | Excepción técnica | `ValidarDnisDuplicados_WhenDbFails_ThrowsException` | `ComprarEntradas_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Al ser subfunción interna, sus tests son principalmente unitarios; la integración se verifica vía CU-10. Los tests se ejecutan con `dotnet test`.

