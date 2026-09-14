# Caso de Uso: Validar límite máximo de compra

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-27 |
| **Nombre** | Validar límite máximo de compra |
| **Actor Principal** | Sistema |
| **Alcance / Nivel** | Sistema; subfunción |
| **Stakeholders e intereses** | Sistema → prevenir acaparamiento; Usuario → conocer límite permitido |
| **Disparador (Trigger)** | El sistema recibe una solicitud de compra que requiere verificar límite por usuario/evento (invocado desde CU-10) |
| **Prioridad / Frecuencia** | Alta; invocado en cada intento de compra |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Validación interna que verifica si el usuario ya alcanzó el límite máximo de entradas permitidas para el evento (ej. máx. 6 entradas por usuario por evento).

### 2. PRECONDICIONES
1. El usuario debe estar identificado (userId).
2. El evento debe existir y estar "Aprobado".
3. Debe existir configuración de límite máximo (parámetro del sistema o del evento).

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200 interno)
1. El Sistema (Capa de Negocio) recibe parámetros: userId, eventoId, cantidadNueva.
2. La **Capa de Negocio** consulta cuántas entradas "Activas" + "Utilizadas" tiene el usuario para ese evento.
3. Suma cantidadNueva + entradasExistentes.
4. Si total <= límiteMáximo, validación exitosa.
5. El Sistema retorna resultado positivo al flujo llamante (CU-10).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **2a. Límite excedido (Excepción de dominio):**
  1. Si en el Paso 4 total > límiteMáximo.
  2. La **Capa de Negocio** lanza `LimiteMaximoExcedidoException` con (límite, actuales, solicitadas).
  3. El flujo llamante (CU-10) maneja y retorna **409 Conflict** al usuario.

* **3a. Error técnico (Excepción no controlada):**
  1. Si en el Paso 2 falla la consulta a BD.
  2. El Sistema lanza excepción técnica.
  3. El flujo llamante retorna **500 Internal Server Error**.

### 5. SUB-VARIACIONES (opcional)
1. Límite configurable por evento vs. global del sistema.
2. Límite por sector vs. global por evento.

### 6. POSTCONDICIONES
1. Resultado de validación retornado al caso de uso llamante.
2. No hay cambio de estado persistente (solo lectura).

---

## Anexo: matrices de referencia

### Códigos HTTP usados (en contexto del CU llamante)

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Validación exitosa (interno). |
| `409` | Conflict | Límite máximo excedido (manejado por CU-10). |
| `500` | Internal Server Error | Error técnico en validación (manejado por CU-10). |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación):** N/A (invocación interna).
- **Verificación (Negocio):** conteo entradas usuario/evento vs. límite configurado.

### Matriz de trazabilidad CU-27 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | Éxito (interno) | `ValidarLimiteMaximo_WithinLimit_ReturnsTrue` | (probado vía CU-10 integración) |
| 2a. Límite excedido | `LimiteMaximoExcedidoException` | `ValidarLimiteMaximo_WhenExceeded_ThrowsException` | `ComprarEntradas_WhenLimitExceeded_Returns409Conflict` |
| 3a. Error técnico | Excepción técnica | `ValidarLimiteMaximo_WhenDbFails_ThrowsException` | `ComprarEntradas_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Al ser subfunción interna, sus tests son principalmente unitarios; la integración se verifica vía CU-10. Los tests se ejecutan con `dotnet test`.
