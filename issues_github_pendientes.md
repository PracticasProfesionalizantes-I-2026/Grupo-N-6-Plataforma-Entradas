# Backlog de Issues Pendientes — Auditoría de Calidad y SDLC

Este checklist contiene los hallazgos detectados durante la auditoría integral de las fases de Diseño, Implementación y Calidad de Código. Está formateado para ser copiado directamente como issues y tareas en GitHub.

---

### 🔴 RIESGO ALTO (Crítico para la ejecución y completitud core)

- [ ] **[SEC-01] Eliminar clave secreta JWT hardcodeada en el código fuente de producción**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/BusinessLogic/Services/UsuarioService.cs` (Línea 128) y `API/Program.cs` (Línea 42)
  - **Descripción:** Se encuentra hardcodeada la cadena literal `"SuperSecretKeyForDevelopmentOnlyChangeInProduction12345!"` directamente en `GenerarTokenJwt`. La inyección de dependencias `IConfiguration` está inyectada en el servicio pero es ignorada, lo que permite forjar tokens con cualquier rol (`SuperAdmin`).
  - **Acción Correctiva:** Leer la clave obligatoriamente desde `_configuration["Jwt:Key"]` lanzando una excepción si no está configurada, e inyectar el valor en tiempo de ejecución mediante secretos de usuario (`dotnet user-secrets`) o variables de entorno en producción.

- [ ] **[CONC-01] Implementar transacción atómica y control de concurrencia en la compra de entradas**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/BusinessLogic/Services/CompraService.cs` (Líneas 64-91) y `DataAccess/Entities/Sector.cs`
  - **Descripción:** Las operaciones de creación de compra, inserción masiva de entradas, actualización de stock vendido y decremento de reserva se ejecutan en llamadas separadas a base de datos sin un contexto transaccional (`IDbContextTransaction`). Si ocurre un fallo intermedio, la base de datos queda inconsistente. Además, la verificación de disponibilidad y la actualización de stock carecen de bloqueo pesimista o token de concurrencia optimista (`[ConcurrencyCheck]` / `RowVersion`), lo que causa sobreventa ante solicitudes concurrentes (violando el RNF-04).
  - **Acción Correctiva:** Envolver el flujo de `CrearCompraAsync` dentro de una transacción (`await using var transaction = await _context.Database.BeginTransactionAsync()`) y agregar control de concurrencia optimista o actualización atómica SQL (`UPDATE Sectores SET EntradasVendidas = EntradasVendidas + @cant WHERE Id = @id AND (Capacidad - EntradasVendidas - EntradasReservadas) >= @cant`).

- [ ] **[FLOW-01] Corregir la auto-aprobación inmediata de compras y desacoplamiento con el flujo de pago**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/BusinessLogic/Services/CompraService.cs` (Líneas 83-87) y `API/Controllers/ComprasController.cs`
  - **Descripción:** En `CrearCompraAsync`, inmediatamente tras persistir la compra en estado `PendientePago`, el código sobreescribe el estado a `Aprobado` (`compra.Estado = EstadoCompra.Aprobado; compra.FechaPago = DateTime.UtcNow;`). Esto anula por completo el ciclo de vida del carrito, la reserva temporal y vuelve inalcanzables los endpoints `PATCH /api/v1/compras/{id}/confirmar-pago` y `PATCH /api/v1/compras/{id}/cancelar`.
  - **Acción Correctiva:** Mantener el estado en `PendientePago` al crear la compra, delegando la confirmación y cambio a `Aprobado` exclusivamente al método `ConfirmarPagoAsync` tras la recepción del pago en efectivo o simulación por caja.

- [ ] **[DOC-01] Documentar Caso de Uso formal para Control de Acceso y Validación de Entradas (RF-11 a RF-13)**
  - **Módulo / Archivo:** `Documentos/Casos de uso/Casos_de_uso_TP/` (Crear `cu-34-validar-entrada-control-acceso.md`) y `Documentos/Business/Documentación de Proyecto de Software.md`
  - **Descripción:** Los requerimientos funcionales RF-11 (validar código único), RF-12 (impedir reutilización) y RF-13 (registrar fecha/hora de validación) junto con el actor "Personal de Control de Acceso" están implementados en `ControlAccesoController.cs` y `EntradaService.cs`, pero carecen de Caso de Uso en la especificación funcional.
  - **Acción Correctiva:** Redactar y anexar el Caso de Uso formal con actor, precondiciones, flujo principal, flujo alternativo (entrada ya utilizada, código inválido, entrada devuelta) y postcondiciones, alineándolo a la matriz de trazabilidad del proyecto.

---

### 🟡 RIESGO MEDIO (Mantenibilidad y Flujos Secundarios)

- [ ] **[BIZ-01] Sincronizar discrepancia de tiempo en reserva temporal (15 vs 10 minutos)**
  - **Módulo / Archivo:** `Documentos/Business/Documentación de Proyecto de Software.md` (Sección 4.1 y RF-05), `Documentos/Casos de uso/Casos_de_uso_TP/cu-29-habilitar-reserva-temporal.md`, y `BusinessLogic/Services/ReservaTemporalService.cs`
  - **Descripción:** El documento de negocio en la sección 4.1 estipula 15 minutos de reserva en carrito, mientras que el CU-29 y el código (`ReservaTemporalOptions.DuracionMinutos = 10`) tienen configurados 10 minutos.
  - **Acción Correctiva:** Definir un valor unificado de negocio y actualizar la documentación o parametrizar el valor en `appsettings.json` respetando el contrato aprobado.

- [ ] **[ARCH-01] Refactorizar cálculo de precio dinámico a evaluación síncrona en compra (RF-10)**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/API/Middleware/BackgroundJobs.cs` (`PrecioDinamicoJob`) y `BusinessLogic/Services/PrecioDinamicoService.cs`
  - **Descripción:** El incremento automático del 20% al superar el 80% de aforo se ejecuta en un cron cada 5 minutos. Durante esa ventana temporal de 5 minutos, compras masivas pueden sobrepasar el 80% y adquirir entradas al precio base sin incremento.
  - **Acción Correctiva:** Invocar la evaluación de umbral de ocupación y ajuste de precio de forma síncrona dentro del flujo de `CrearCompraAsync` / `ConfirmarPagoAsync` para el sector afectado, manteniendo el job en background únicamente como mecanismo de reconciliación eventual.

- [ ] **[SMELL-01] Refactorizar `CompraService` para mitigar violación de Principio de Responsabilidad Única (SRP)**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/BusinessLogic/Services/CompraService.cs`
  - **Descripción:** La clase `CompraService` concentra 6 dependencias, genera identificadores de dominio (`GenerarCodigoUnico`), procesa listas de DTOs, aplica lógica de carritos y administra validaciones de parámetros de reportes.
  - **Acción Correctiva:** Extraer la generación de códigos únicos y formateo de tickets a un servicio de dominio especializado (`ITicketCodeGenerator`), y desacoplar las validaciones de consulta de reportes delegándolas a la capa de validadores (ej. FluentValidation o clases de soporte de consulta).

- [ ] **[DATA-01] Ajustar validación de condición "Evento Agotado" en Devoluciones (RF-09 vs CU-15)**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/BusinessLogic/Services/DevolucionService.cs` (Línea 38) y `cu-15-solicitar-devolucion.md`
  - **Descripción:** El RF-09 y el CU-15 exigen que la devolución se admita para eventos agotados o con entradas activas, pero el servicio únicamente comprueba si `evento.FechaInicio <= DateTime.UtcNow`. Si la política del negocio restringe devoluciones a eventos agotados con reventa automática, la verificación de aforo no se está aplicando.
  - **Acción Correctiva:** Clarificar la regla de negocio: si las devoluciones aplican a cualquier evento futuro, actualizar la redacción del RF-09 y CU-15; de lo contrario, validar `sector.StockDisponible == 0` en `DevolucionService.cs`.

- [ ] **[SEC-02] Incorporar política de roles adecuada en `ControlAccesoController`**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/API/Controllers/ControlAccesoController.cs` (Línea 10)
  - **Descripción:** El controlador tiene asignada la directiva `[Authorize(Policy = "SuperAdmin")]`. Un operador de control de acceso en la puerta del evento no debe requerir credenciales de Super Administrador con acceso a todo el sistema.
  - **Acción Correctiva:** Crear el rol y política `"PersonalAcceso"` o `"Organizador"` con permisos restringidos exclusivamente a la validación de tickets mediante código único.

---

### 🟢 RIESGO BAJO (Deuda técnica y Mejoras Menores)

- [ ] **[DOC-02] Depurar inconsistencia de notificaciones en Casos de Uso (CU-16, CU-21, CU-24, CU-32, CU-33)**
  - **Módulo / Archivo:** `Documentos/Casos de uso/Casos_de_uso_TP/`
  - **Descripción:** Los casos de uso indican en sus flujos que el sistema envía correos electrónicos o notificaciones in-app al organizador, a pesar de que el documento de negocio (sección 4.2) declara las notificaciones formalmente fuera del alcance (Out of Scope).
  - **Acción Correctiva:** Reemplazar las referencias de "envío de email" por "actualización de estado en plataforma visible desde el panel de gestión".

- [ ] **[DOC-03] Resolver saltos y orfandad de numeración en casos de uso (CU-18 y CU-20)**
  - **Módulo / Archivo:** `Documentos/Casos de uso/Casos_de_uso_TP/`
  - **Descripción:** El repositorio contiene `cu-19-gestionar-sectores.md` y salta de `cu-17` a `cu-19`, mientras que `cu-20-consultar-eventos-creados.md` fue agregado posteriormente generando desfases con referencias previas.
  - **Acción Correctiva:** Normalizar el índice maestro de casos de uso en `Casos_de_uso.md` con un mapa de trazabilidad claro de identificadores.

- [ ] **[PRIV-01] Aplicar enmascaramiento de DNIs en logs y comprobantes digitales (RNF-02)**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/BusinessLogic/Services/EntradaService.cs` (Línea 86) y `DTOs/Entrada/EntradaDTOs.cs`
  - **Descripción:** El RNF-02 exige confidencialidad y almacenamiento seguro de datos personales sensibles (DNIs). Actualmente el DNI se expone en texto plano en comprobantes generados y DTOs de listados.
  - **Acción Correctiva:** Enmascarar el DNI en los DTOs de salida y en el comprobante QuestPDF (ejemplo: `******78` o `12.***.678`), mostrando el documento completo solo al titular autenticado o en el control de acceso en puerta.

- [ ] **[CLEAN-01] Eliminar bloqueos síncronos innecesarios y dependencias huérfanas en tests**
  - **Módulo / Archivo:** `Documentos/Código/EntradApp/DataAccess/DbInitializer.cs` y archivos de solución `.sln`
  - **Descripción:** Existen dos archivos de solución (`EntradApp.sln` en la raíz de `Código` y otro dentro de `EntradApp/`). Las rutas relativas de proyectos en la solución raíz presentan discrepancias de directorio que causan advertencias en herramientas de build automático (`dotnet test`).
  - **Acción Correctiva:** Eliminar la solución redundante en `Documentos/Código/EntradApp.sln` y mantener únicamente la solución interna con las rutas canónicas relativas a los proyectos.

