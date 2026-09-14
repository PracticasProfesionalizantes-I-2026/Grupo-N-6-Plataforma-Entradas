# Caso de Uso: Recuperar contraseña

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-07 |
| **Nombre** | Recuperar contraseña |
| **Actor Principal** | Usuario Visitante (No registrado) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → restablecer su contraseña; Sistema → verificar identidad y permitir cambio seguro |
| **Disparador (Trigger)** | El usuario solicita recuperar su contraseña ingresando su email |
| **Prioridad / Frecuencia** | Media; uso ocasional |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado solicitar el restablecimiento de su contraseña mediante un enlace seguro enviado a su email registrado.

### 2. PRECONDICIONES
1. El usuario debe estar registrado con ese email y tener estado "Activo".
2. El sistema debe tener configurado el servicio de envío de emails.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `POST /api/auth/recuperar-password` con JSON (email).
2. La **Capa de Presentación** valida formato de email.
3. La **Capa de Negocio** verifica que el email exista y el usuario esté activo.
4. El Sistema genera un token de restablecimiento (expiración corta, uso único) y lo guarda.
5. El Sistema envía email con enlace `POST /api/auth/reset-password?token={token}`.
6. El Sistema devuelve **200 OK** (mensaje genérico por seguridad).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. JSON inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 1 el JSON tiene sintaxis incorrecta.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2a. Email inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el email tiene formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Email no registrado (HTTP 200 OK - mensaje genérico):**
  1. Si en el Paso 3 el email no existe en el sistema.
  2. Por seguridad, el Sistema devuelve **200 OK** con mensaje genérico "Si el email existe, recibirá instrucciones".
  3. No se envía email. Fin del caso de uso.

* **3b. Usuario inactivo (HTTP 200 OK - mensaje genérico):**
  1. Si en el Paso 3 el email existe pero el usuario no está activo.
  2. Por seguridad, el Sistema devuelve **200 OK** con mensaje genérico.
  3. No se envía email. Fin del caso de uso.

* **4a. Error envío email (HTTP 500 Internal Server Error):**
  1. Si en el Paso 5 falla el servicio de email.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Token enviado por email; alternativa: SMS o notificación push (futuro).

### 6. POSTCONDICIONES
1. Token de restablecimiento generado y almacenado (hash) con expiración.
2. Email enviado al usuario (si existe y está activo).
3. El usuario puede usar el token para establecer nueva contraseña (CU separado).

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Solicitud procesada (siempre mensaje genérico por seguridad). |
| `400` | Bad Request | JSON inválido o email con formato incorrecto. |
| `500` | Internal Server Error | Error técnico enviando email. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400):** formato de email.
- **Verificación (Negocio):** existencia de email y estado activo (pero respuesta siempre 200 por seguridad).

### Matriz de trazabilidad CU-07 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `RecuperarPassword_WithValidEmail_SendsEmailAndReturnsOk` | `PostRecuperarPassword_WithValidEmail_Returns200OK` |
| 1a. JSON inválido | `400 Bad Request` | — | `PostRecuperarPassword_WithInvalidJson_Returns400BadRequest` |
| 2a. Email inválido | `400 Bad Request` | — | `PostRecuperarPassword_WithInvalidEmail_Returns400BadRequest` |
| 3a. Email no registrado | `200 OK` | `RecuperarPassword_WhenEmailNotExists_ReturnsGenericOk` | `PostRecuperarPassword_WhenEmailNotExists_Returns200OK` |
| 3b. Usuario inactivo | `200 OK` | `RecuperarPassword_WhenUserInactive_ReturnsGenericOk` | `PostRecuperarPassword_WhenUserInactive_Returns200OK` |
| 4a. Error email | `500 Internal Server Error` | `RecuperarPassword_WhenEmailFails_ThrowsException` | `PostRecuperarPassword_WhenEmailFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
