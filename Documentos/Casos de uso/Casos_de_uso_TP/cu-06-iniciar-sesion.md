# Caso de Uso: Iniciar sesión

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-06 |
| **Nombre** | Iniciar sesión |
| **Actor Principal** | Usuario Visitante (No registrado) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Usuario → obtener token de acceso; Sistema → autenticar y autorizar |
| **Disparador (Trigger)** | El usuario envía sus credenciales (email y password) |
| **Prioridad / Frecuencia** | Alta; uso muy frecuente |
| **Reglas de negocio relacionadas** | — |

---

### 1. BREVE DESCRIPCIÓN
Permite a un usuario registrado autenticarse en la plataforma mediante email y contraseña, obteniendo un token JWT para accesos posteriores.

### 2. PRECONDICIONES
1. El usuario debe estar registrado y tener estado "Activo".
2. El usuario no tiene una sesión válida activa.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200)
1. El Actor envía una petición al endpoint `POST /api/auth/login` con JSON (email, password).
2. La **Capa de Presentación** valida que ambos campos estén presentes.
3. La **Capa de Negocio** verifica las credenciales contra el hash almacenado.
4. El Sistema genera un token JWT con claims (userId, email, roles, exp).
5. El Sistema devuelve **200 OK** con el token y datos básicos del usuario.

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. JSON inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 1 el JSON tiene sintaxis incorrecta.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2a. Credenciales faltantes (HTTP 400 Bad Request):**
  1. Si en el Paso 2 falta email o password.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. Credenciales inválidas (HTTP 401 Unauthorized):**
  1. Si en el Paso 3 el email no existe o el password no coincide.
  2. La **Capa de Negocio** lanza `CredencialesInvalidasException`.
  3. El Sistema devuelve **401 Unauthorized** con mensaje genérico. Fin del caso de uso.

* **3b. Usuario inactivo/bloqueado (HTTP 403 Forbidden):**
  1. Si en el Paso 3 el usuario existe pero su estado no es "Activo".
  2. La **Capa de Negocio** lanza `UsuarioInactivoException`.
  3. El Sistema devuelve **403 Forbidden**. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la generación del token.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Login con "recordar sesión" (token con mayor expiración).
2. Login desde web, móvil, o API directa.

### 6. POSTCONDICIONES
1. Token JWT emitido y entregado al cliente.
2. El usuario puede acceder a endpoints protegidos con el token.

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `200` | OK | Login exitoso, token JWT retornado. |
| `400` | Bad Request | JSON inválido o campos faltantes. |
| `401` | Unauthorized | Credenciales incorrectas. |
| `403` | Forbidden | Usuario inactivo o bloqueado. |
| `500` | Internal Server Error | Error técnico generando token. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400):** presencia de email y password.
- **Verificación (Negocio, → 401/403):** verificación de hash de password, estado del usuario.

### Matriz de trazabilidad CU-06 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `200 OK` | `Login_WithValidCredentials_ReturnsToken` | `PostLogin_WithValidCredentials_Returns200OK` |
| 1a. JSON inválido | `400 Bad Request` | — | `PostLogin_WithInvalidJson_Returns400BadRequest` |
| 2a. Credenciales faltantes | `400 Bad Request` | — | `PostLogin_WithMissingFields_Returns400BadRequest` |
| 3a. Credenciales inválidas | `401 Unauthorized` | `Login_WithInvalidPassword_ThrowsCredencialesInvalidasException` | `PostLogin_WithInvalidPassword_Returns401Unauthorized` |
| 3b. Usuario inactivo | `403 Forbidden` | `Login_WhenUserInactive_ThrowsUsuarioInactivoException` | `PostLogin_WhenUserInactive_Returns403Forbidden` |
| 4a. Error interno | `500 Internal Server Error` | `Login_WhenTokenGenerationFails_ThrowsException` | `PostLogin_WhenTokenFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
