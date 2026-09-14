# Caso de Uso: Registrarse

> Especificación elaborada siguiendo la guía
> `GUIA-Especificacion-Casos-de-Uso.md` (sección 3).
> Estructura basada en el ejemplo `CU-02 Alta de Medico.md`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-05 |
| **Nombre** | Registrarse |
| **Actor Principal** | Usuario Visitante (No registrado) |
| **Alcance / Nivel** | Sistema; meta de usuario |
| **Stakeholders e intereses** | Visitante → crear cuenta para acceder a funcionalidades; Sistema → registrar nuevo usuario |
| **Disparador (Trigger)** | El visitante completa y envía el formulario de registro |
| **Prioridad / Frecuencia** | Alta; uso ocasional por usuario |
| **Reglas de negocio relacionadas** | RN-01 (DNI único por usuario) |

---

### 1. BREVE DESCRIPCIÓN
Permite a un visitante crear una cuenta de usuario registrando sus datos personales, credenciales y DNI.

### 2. PRECONDICIONES
1. El visitante no tiene una sesión activa.
2. El DNI ingresado no debe estar registrado previamente en el sistema.

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 201)
1. El Actor envía una petición al endpoint `POST /api/usuarios/registro` con JSON (nombre, apellido, email, password, dni, fechaNacimiento).
2. La **Capa de Presentación** valida formato y campos obligatorios.
3. La **Capa de Negocio** verifica que el DNI no exista (RN-01) y que el email no esté registrado.
4. La **Capa de Persistencia** crea el usuario con rol "Registrado" y estado "Activo".
5. El Sistema devuelve **201 Created** con datos del usuario creado (sin password).

### 4. FLUJOS ALTERNATIVOS (Caminos Tristes / Excepciones)

* **1a. JSON inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 1 el JSON tiene sintaxis incorrecta.
  2. El Sistema (Capa de Presentación) rechaza la petición.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **2a. Campos obligatorios faltantes (HTTP 400 Bad Request):**
  1. Si en el Paso 2 faltan campos requeridos.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request** con detalle. Fin del caso de uso.

* **2b. Formato inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 email, DNI o fecha tienen formato incorrecto.
  2. El Sistema (Capa de Presentación) rechaza por validación.
  3. El Sistema devuelve **400 Bad Request**. Fin del caso de uso.

* **3a. DNI ya registrado (HTTP 409 Conflict):**
  1. Si en el Paso 3 el DNI ya existe en el sistema (RN-01).
  2. La **Capa de Negocio** lanza excepción `DniDuplicadoException`.
  3. El Sistema devuelve **409 Conflict** con mensaje "El DNI ya está registrado". Fin del caso de uso.

* **3b. Email ya registrado (HTTP 409 Conflict):**
  1. Si en el Paso 3 el email ya existe.
  2. La **Capa de Negocio** lanza excepción `EmailDuplicadoException`.
  3. El Sistema devuelve **409 Conflict**. Fin del caso de uso.

* **4a. Error interno (HTTP 500 Internal Server Error):**
  1. Si en el Paso 4 falla la persistencia.
  2. El Sistema devuelve **500 Internal Server Error**. Fin del caso de uso.

### 5. SUB-VARIACIONES (opcional)
1. Registro vía formulario web, API directa, o app móvil.

### 6. POSTCONDICIONES
1. Usuario creado en BD con rol "Registrado" y estado "Activo".
2. El usuario puede iniciar sesión inmediatamente.

---

## Anexo: matrices de referencia

### Códigos HTTP usados

| Código HTTP | Nombre Técnico | Contexto de Aplicación en el Caso de Uso |
| --- | --- | --- |
| `201` | Created | Usuario registrado exitosamente. |
| `400` | Bad Request | JSON inválido, campos faltantes, formatos incorrectos. |
| `409` | Conflict | DNI o email ya registrados (RN-01). |
| `500` | Internal Server Error | Error técnico en persistencia. |

### Nota: Validación vs. Verificación aplicada

- **Validación (Presentación, → 400):** campos obligatorios, formato email, formato DNI, longitud password, fecha nacimiento válida.
- **Verificación (Negocio, → 409):** unicidad de DNI (RN-01), unicidad de email.

### Matriz de trazabilidad CU-05 → Test

| Paso del CU | Excepción / Código | Test unitario (BusinessLogic) | Test integración (HTTP) |
| --- | --- | --- | --- |
| Flujo principal | `201 Created` | `RegistrarUsuario_WithValidData_CreatesAndReturnsUser` | `PostRegistro_WithValidData_Returns201Created` |
| 1a. JSON inválido | `400 Bad Request` | — | `PostRegistro_WithInvalidJson_Returns400BadRequest` |
| 2a. Campos faltantes | `400 Bad Request` | — | `PostRegistro_WithMissingFields_Returns400BadRequest` |
| 2b. Formato inválido | `400 Bad Request` | — | `PostRegistro_WithInvalidFormat_Returns400BadRequest` |
| 3a. DNI duplicado | `409 Conflict` | `RegistrarUsuario_WhenDniExists_ThrowsDniDuplicadoException` | `PostRegistro_WhenDniExists_Returns409Conflict` |
| 3b. Email duplicado | `409 Conflict` | `RegistrarUsuario_WhenEmailExists_ThrowsEmailDuplicadoException` | `PostRegistro_WhenEmailExists_Returns409Conflict` |
| 4a. Error interno | `500 Internal Server Error` | `RegistrarUsuario_WhenRepositoryFails_ThrowsException` | `PostRegistro_WhenDbFails_Returns500InternalServerError` |

> Regla de oro: cada flujo del caso de uso debe tener al menos un test. Los tests se ejecutan con `dotnet test`.
