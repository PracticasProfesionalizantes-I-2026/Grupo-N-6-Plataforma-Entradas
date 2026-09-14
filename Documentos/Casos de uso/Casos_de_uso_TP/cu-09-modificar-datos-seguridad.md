# Caso de Uso: Modificar datos de seguridad

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-09 |
| **Nombre** | Modificar datos de seguridad |
| **Actor Principal** | Usuario Registrado |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → cambiar contraseña; Sistema → garantizar seguridad de credenciales |
| **Disparador (Trigger)** | El usuario registrado accede a configuración de seguridad y solicita cambio de contraseña |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario autenticado cambiar su contraseña actual por una nueva, verificando la contraseña actual.

### 2. PRECONDICIONES
1. El usuario debe estar autenticado (Token JWT válido).
2. El usuario debe conocer su contraseña actual.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `PUT /api/usuarios/password` con JSON (passwordActual, passwordNueva, passwordNuevaConfirmacion) y header Authorization.
2. La **Capa de Presentación** valida el JWT, presencia de campos y que passwordNueva coincida con confirmación.
3. La **Capa de Negocio** verifica que passwordActual coincida con el hash almacenado.
4. La **Capa de Negocio** hashea la passwordNueva y actualiza en BD.
5. El Sistema devuelve **200 OK** (sin datos sensibles).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. Token inválido/ausente (HTTP 401 Unauthorized):**
  1. Si en el Paso 1 el header Authorization falta o el token es inválido/expirado.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **2a. JSON inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 1 el JSON tiene sintaxis incorrecta.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2b. Campos faltantes o no coinciden (HTTP 400 Bad Request):**
  1. Si en el Paso 2 falta passwordActual, passwordNueva, o no coinciden nueva y confirmación.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2c. Password nueva no cumple política (HTTP 400 Bad Request):**
  1. Si en el Paso 2 passwordNueva no cumple longitud mínima, complejidad, etc.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Password actual incorrecta (HTTP 401 Unauthorized):**
  1. Si en el Paso 3 passwordActual no coincide con el hash.
  2. La **Capa de Negocio** lanza `PasswordActualIncorrectaException`.
  3. El Sistema devuelve **401 Unauthorized**. Fin del caso de uso.

* **3b. Password nueva igual a actual (HTTP 400 Bad Request):**
  1. Si en el Paso 3 la passwordNueva es idéntica a la actual.
  2. La **Capa de Negocio** rechaza el cambio.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la persistencia.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Cambio de contraseña forzado por admin (flujo distinto).
2. Uso de 2FA para confirmar cambio (futuro).

### 6. POSTCONDICIONES
1. Contraseña actualizada (hash nuevo) en BD.
2. Se invalidan todos los tokens JWT existentes del usuario (refresh tokens revocados).
3. Usuario debe volver a iniciar sesión.

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Contraseña cambiada exitosamente. |
| `400` | Bad Request | JSON inválido, campos faltantes, passwords no coinciden, política no cumplida, password igual a actual. |
| `401` | Unauthorized | Token inválido o password actual incorrecta. |
| `500` | Internal Server Error | Error técnico en persistencia. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400/401):** JWT válido, campos obligatorios, coincidencia nueva/confirmación, política de password.
- **Verificación (Negocio, → 401/400):** verificación hash password actual, password nueva ≠ actual.

### Matriz de trazabilidad CU-09 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `CambiarPassword_WithValidData_UpdatesHashAndRevokesTokens` | `PutPassword_WithValidData_Returns200OK` |
| 1a. Token inválido | `401 Unauthorized` | — | `PutPassword_WithInvalidToken_Returns401Unauthorized` |
| 2a. JSON inválido | `400 Bad Request` | — | `PutPassword_WithInvalidJson_Returns400BadRequest` |
| 2b. Campos faltantes/no coinciden | `400 Bad Request` | — | `PutPassword_WithMismatchedPasswords_Returns400BadRequest` |
| 2c. Política no cumplida | `400 Bad Request` | — | `PutPassword_WithWeakPassword_Returns400BadRequest` |
| 3a. Password actual incorrecta | `401 Unauthorized` | `CambiarPassword_WithWrongCurrentPassword_ThrowsException` | `PutPassword_WithWrongCurrentPassword_Returns401Unauthorized` |
| 3b. Password igual a actual | `400 Bad Request` | `CambiarPassword_WhenNewEqualsCurrent_ThrowsException` | `PutPassword_WhenNewEqualsCurrent_Returns400BadRequest` |
| 4a. Error interno | `500 Internal Server Error` | `CambiarPassword_WhenRepositoryFails_ThrowsException` | `PutPassword_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
