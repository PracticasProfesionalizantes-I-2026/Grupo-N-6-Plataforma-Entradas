# Caso de Uso: Modificar datos personales

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-08 |
| **Nombre** | Modificar datos personales |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → actualizar su información; Sistema → mantener datos vigentes |
| **Disparador (Trigger)** | El usuario registrado accede a su perfil y edita sus datos personales |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | RN-01 (DNI único) |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado modificar sus datos personales (nombre, apellido, email, DNI, fecha de nacimiento).

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El usuario solo puede modificar sus propios datos.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `PUT /api/usuarios/perfil` con JSON (nombre, apellido, email, dni, fechaNacimiento) y header Authorization.
2. La **Capa de Presentación** valida el JWT y los datos del JSON.
3. La **Capa de Negocio** verifica que el nuevo email y DNI no estén en uso por otro usuario (RN-01).
4. La **Capa de Persistencia** actualiza los datos del usuario.
5. El Sistema devuelve **200 OK** con los datos actualizados.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. JSON inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 1 el JSON tiene sintaxis incorrecta.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. Formato inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 email, DNI o fecha tienen formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Email en uso por otro usuario (HTTP 409 Conflict):**
  1. Si en el Paso 3 el nuevo email ya pertenece a otro usuario.
  2. La **Capa de Negocio** lanza `EmailDuplicadoException`.
  3. El Sistema devuelve **409 Conflict**. Fin del caso de uso.

* **3b. DNI en uso por otro usuario (HTTP 409 Conflict):**
  1. Si en el Paso 3 el nuevo DNI ya pertenece a otro usuario (RN-01).
  2. La **Capa de Negocio** lanza `DniDuplicadoException`.
  3. El Sistema devuelve **409 Conflict**. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la persistencia.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. El usuario puede actualizar solo algunos campos (PATCH semántico).
2. Cambio de email requiere re-verificación (futuro).

### 6. POSTCONDICIONES
1. Datos personales actualizados en BD.
2. Si cambió email, se invalida sesión actual (opcional, por seguridad).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Datos actualizados correctamente. |
| `400` | Bad Request | JSON inválido o formatos incorrectos. |
| `401` | Unauthorized | Token JWT inválido, expirado o ausente. |
| `409` | Conflict | Email o DNI ya en uso por otro usuario (RN-01). |
| `500` | Internal Server Error | Error técnico en persistencia. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, formato JSON, formato email/DNI/fecha.
- **Verificación (Negocio, → 409):** unicidad de email y DNI (RN-01) excluyendo al propio usuario.

### Matriz de trazabilidad CU-08 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `ModificarDatosPersonales_WithValidData_UpdatesAndReturnsUser` | `PutPerfil_WithValidData_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `PutPerfil_WithInvalidToken_Returns401Unauthorized` |
| 2a. JSON inválido | `400 Bad Request` | — | `PutPerfil_WithInvalidJson_Returns400BadRequest` |
| 2b. Formato inválido | `400 Bad Request` | — | `PutPerfil_WithInvalidFormat_Returns400BadRequest` |
| 3a. Email duplicado | `409 Conflict` | `ModificarDatosPersonales_WhenEmailExists_ThrowsEmailDuplicadoException` | `PutPerfil_WhenEmailExists_Returns409Conflict` |
| 3b. DNI duplicado | `409 Conflict` | `ModificarDatosPersonales_WhenDniExists_ThrowsDniDuplicadoException` | `PutPerfil_WhenDniExists_Returns409Conflict` |
| 4a. Error interno | `500 Internal Server Error` | `ModificarDatosPersonales_WhenRepositoryFails_ThrowsException` | `PutPerfil_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
