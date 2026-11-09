# Caso de Uso: Validar Entrada en Control de Acceso

> Especificación elaborada siguiendo la guía estándar del proyecto (`GUIA-Especificacion-Casos-de-Uso.md`).
> Cobertura y trazabilidad directa con los requerimientos funcionales **RF-11, RF-12 y RF-13** y los requerimientos no funcionales **RNF-02 y RNF-11**.
> Módulos implementados en código: `ControlAccesoController.cs`, `EntradaService.cs`, `EntradaRepository.cs` y tests en `ControlAccesoControllerTests`.

| Campo | Valor |
| --- | --- |
| **ID del Caso de Uso** | CU-34 |
| **Nombre** | Validar Entrada en Control de Acceso |
| **Actor Principal** | Personal de Control de Acceso / Operador de Acceso (o Super Admin) |
| **Alcance / Nivel** | Sistema / API REST; meta de subfunción operativa en puerta |
| **Stakeholders e intereses** | Personal de Control de Acceso -> validar autenticidad y permitir/denegar ingreso en menos de 300 ms; Comprador/Asistente -> acceder al evento de forma ágil y segura; Organizador del Evento -> prevenir fraudes, entradas duplicadas o adulteradas; Super Admin -> auditar registros de acceso. |
| **Disparador (Trigger)** | El operador en puerta escanea el código QR o ingresa manualmente el código único del ticket presentado por el asistente. |
| **Prioridad / Frecuencia** | Alta; muy alta frecuencia durante el día y horario del evento. |
| **Reglas de negocio relacionadas** | **RN-ACC-01:** Cada entrada posee un código único alfanumérico generado por el sistema con formato `EVT-[A-F0-9]{8}` (RF-11).<br>**RN-ACC-02:** Una entrada solo puede ser validada e ingresar una única vez; si ya fue validada previamente o está en estado `Utilizada`, se debe rechazar el ingreso (RF-12).<br>**RN-ACC-03:** Al ser validada exitosamente, el sistema debe registrar en auditoría la fecha y hora exacta (UTC) de ingreso y cambiar su estado a `Utilizada` (RF-13).<br>**RN-ACC-04:** Entradas en estado `Cancelada` o `Devuelta` no permiten el acceso bajo ninguna circunstancia. |

---

### 1. BREVE DESCRIPCIÓN
Permite al Personal de Control de Acceso validar en tiempo real la autenticidad y el estado de una entrada mediante su código único alfanumérico (leído desde QR o ingresado por terminal), registrando la fecha/hora de acceso y bloqueando cualquier intento de reingreso o reutilización fraudulenta del ticket.

---

### 2. PRECONDICIONES
1. El Personal de Control de Acceso u Operador ha iniciado sesión y cuenta con credenciales activas y autorizadas (Token JWT válido con rol/política habilitada).
2. El evento se encuentra aprobado y en curso o próximo a iniciar.
3. La base de datos y la API de EntradApp se encuentran operativas y accesibles con latencia reducida (RNF-11: respuesta < 300 ms).

---

### 3. FLUJO PRINCIPAL (Camino Feliz - HTTP 200 Entrada Válida)
1. El Asistente presenta su comprobante con el código único o código QR en el punto de acceso.
2. El Actor (Personal de Control de Acceso) envía una petición HTTP `POST` al endpoint `/api/v1/control-acceso/validar` enviando el payload JSON:
   ```json
   {
     "codigoUnico": "EVT-A1B2C3D4"
   }
   ```
3. La **Capa de Presentación** (`ControlAccesoController.Validar`):
   a. Verifica la autenticación mediante token JWT en cabecera `Authorization: Bearer <token>`.
   b. Valida que el campo `codigoUnico` no sea nulo ni vacío.
   c. Valida mediante expresión regular que el código respete el formato canónico `^EVT-[A-F0-9]{8}$`.
4. La **Capa de Negocio** (`EntradaService.ValidarEntradaAsync`):
   a. Normaliza el código a mayúsculas y consulta la entidad `Entrada` en la persistencia mediante `IEntradaRepository.GetByCodigoUnicoAsync`.
   b. Verifica que la entrada exista y no esté marcada con soft delete (`EsBorrado = false`).
   c. Verifica que el estado actual de la entrada sea estrictamente `EstadoEntrada.Activa`.
   d. Actualiza el estado de la entrada a `EstadoEntrada.Utilizada` y asigna `FechaValidacion = DateTime.UtcNow`.
   e. Persiste los cambios en la base de datos a través de `_entradaRepository.UpdateEstadoAsync`.
5. La Capa de Negocio retorna un objeto `Result.Success(EntradaResponseDTO)`.
6. La Capa de Presentación responde con código HTTP **200 OK** y el detalle completo de validación:
   ```json
   {
     "valido": true,
     "entradaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
     "codigoUnico": "EVT-A1B2C3D4",
     "dni": "12345678",
     "sectorNombre": "Platea Baja",
     "eventoNombre": "Festival de Rock 2026",
     "eventoFechaInicio": "2026-12-15T21:00:00Z",
     "eventoLugar": "Estadio Central",
     "estadoAnterior": 1,
     "fechaValidacion": "2026-12-15T20:45:12Z",
     "error": null
   }
   ```
7. El sistema visualiza en pantalla del operador la confirmación en verde y se habilita el acceso físico del asistente.

---

### 4. FLUJOS ALTERNATIVOS Y EXCEPCIONES

* **2a. Código único ausente o en blanco (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el payload contiene `codigoUnico` nulo, vacío o con espacios en blanco.
  2. El Sistema retorna HTTP **400 Bad Request** con el mensaje: `{"error": "El código único es requerido"}`. Fin del caso de uso.

* **2b. Formato de código único inválido (HTTP 400 Bad Request):**
  1. Si en el Paso 2 el código no coincide con la máscara `^EVT-[A-F0-9]{8}$` (ej. longitud errónea o caracteres inválidos).
  2. El Sistema retorna HTTP **400 Bad Request** con el mensaje: `{"error": "Formato de código único inválido. Debe ser como: EVT-A1B2C3D4"}`. Fin del caso de uso.

* **4a. Entrada no encontrada (HTTP 200 con Valido = false):**
  1. Si en el Paso 4a no existe ningún registro en la base de datos con dicho código único.
  2. La Capa de Negocio retorna `Result.Failure("Entrada no encontrada")`.
  3. El Sistema responde con HTTP **200 OK** indicando:
     ```json
     {
       "valido": false,
       "error": "Entrada no encontrada",
       "codigoUnico": "EVT-99999999"
     }
     ```
  4. El operador visualiza alerta en rojo y niega el ingreso. Fin del caso de uso.

* **4b. Entrada ya utilizada previamente (HTTP 200 con Valido = false - Prevención de Reingreso):**
  1. Si en el Paso 4c la entrada ya posee estado `EstadoEntrada.Utilizada`.
  2. La Capa de Negocio detecta que la entrada no está en estado activa y retorna `Result.Failure("La entrada ya fue utilizada")`.
  3. El Sistema responde con HTTP **200 OK**:
     ```json
     {
       "valido": false,
       "error": "La entrada ya fue utilizada",
       "codigoUnico": "EVT-A1B2C3D4"
     }
     ```
  4. El operador visualiza advertencia de duplicidad/reingreso no permitido y retiene al asistente para control de seguridad. Fin del caso de uso.

* **4c. Entrada devuelta o cancelada (HTTP 200 con Valido = false):**
  1. Si en el Paso 4c la entrada se encuentra en estado `Devuelta` o `Cancelada`.
  2. La Capa de Negocio retorna `Result.Failure("La entrada no está activa")`.
  3. El Sistema responde con HTTP **200 OK**:
     ```json
     {
       "valido": false,
       "error": "La entrada no está activa",
       "codigoUnico": "EVT-A1B2C3D4"
     }
     ```
  4. Se deniega el ingreso. Fin del caso de uso.

* **Aut-1. Token JWT ausente o inválido (HTTP 401 Unauthorized):**
  1. Si la petición no incluye cabecera de autenticación o el token ha expirado.
  2. El middleware de autenticación rechaza la solicitud retornando HTTP **401 Unauthorized**. Fin del caso de uso.

* **Aut-2. Permisos insuficientes (HTTP 403 Forbidden):**
  1. Si el usuario autenticado no posee el rol o directiva requerida para control de accesos.
  2. El middleware de autorización retorna HTTP **403 Forbidden**. Fin del caso de uso.

---

### 5. POSTCONDICIONES
* **Éxito (Camino Feliz):**
  - La entrada cambia su estado en base de datos a `EstadoEntrada.Utilizada` (valor ordinal 2).
  - Queda asentado el timestamp exacto de ingreso en el campo `FechaValidacion` (en UTC).
  - El DNI del portador y los datos del sector quedan auditados y confirmados en la terminal de acceso.
* **Fallo:**
  - El estado de la entrada en base de datos permanece inalterado.
  - No se modifica el timestamp de validación.
  - Queda registrado el intento denegado en los logs de la aplicación.

---

### 6. REQUERIMIENTOS NO FUNCIONALES APLICABLES
* **RNF-11 (Rendimiento en Puerta):** El tiempo total de procesamiento y respuesta del endpoint `/api/v1/control-acceso/validar` no debe superar los **300 ms** bajo condiciones normales de carga concurrente.
* **RNF-02 (Protección de Datos Personales):** El DNI devuelto en la respuesta debe ser verificado visualmente contra la documentación física del asistente por parte del personal de acceso, limitando su exposición a pantallas operativas autorizadas.
* **RNF-12 (Alta Disponibilidad):** El servicio de control de acceso debe garantizar continuidad operativa durante los picos de afluencia de ingreso al evento.

