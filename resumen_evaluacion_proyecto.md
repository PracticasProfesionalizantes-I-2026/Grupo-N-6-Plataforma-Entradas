# Informe de Auditoría y Evaluación Integral de Proyecto

**Proyecto:** EntradApp — Plataforma de Gestión y Venta de Entradas  
**Alumnos / Autores:** Lara Glaría, Paula Quevedo  
**Institución:** Instituto Cooperativo de Enseñanza Superior (ICES) — Centro Universitario Sunchales (CUS)  
**Carrera:** Técnico Superior en Desarrollo de Software  
**Fecha de Auditoría:** Noviembre 2026  
**Roles del Auditor:** Arquitecto de Software Senior, Analista Funcional Senior y Auditor QA  

---

## 1. Visión General del Proyecto y Madurez del SDLC

La presente auditoría evalúa de forma holística el Ciclo de Vida de Desarrollo de Software (SDLC) ejecutado en el proyecto **EntradApp**, abarcando tres dimensiones interconectadas: **Diseño Documental (Negocio vs. Casos de Uso)**, **Implementación Técnica (Diseño vs. Código Fuente)** y **Calidad Estática & Arquitectura (Clean Code, Seguridad, Patrones y Prácticas)**.

### Evaluación del Nivel de Madurez (CMMI / Agile Engineering)
El proyecto se sitúa en un **Nivel 2 avanzado / Nivel 3 inicial de madurez de ingeniería de software**. 
- **Aspectos Destacados:** Existe un esfuerzo sustancial y admirable por mantener una estructura en capas desacoplada en .NET 10 (Clean Architecture / On-Premise Layered Pattern: API, BusinessLogic, DataAccess, Shared, Tests), inversión de dependencias estricta mediante interfaces, implementación de Result Pattern y una suite formal de pruebas unitarias e integrales automatizadas con colecciones de Bruno/Postman para pruebas de integración.
- **Puntos Críticos:** La auditoría constata un fenómeno común pero de alto riesgo en proyectos formativos: **desalineación de fases (drift funcional y técnico)**. Se observan requerimientos funcionales críticos implementados en código que carecen de especificación en Casos de Uso (ej. Control de Acceso / Validación en Puerta), divergencias directas en reglas de negocio (ej. tiempo de reserva temporal de 15 vs 10 minutos, tratamiento de pagos en efectivo vs simulación de estados de transacción), exclusión o no reflejo de stakeholders en los flujos interactivos, y omisiones críticas de concurrencia y seguridad que pondrían en peligro un entorno de producción masivo (ausencia de transacciones atómicas distribuidas, carrera crítica en el descuento de stock bajo SQLite y hardcodeo de llaves criptográficas).

---

## 2. Fortalezas Identificadas

A lo largo del análisis minucioso de la documentación y del repositorio de código, se destacan los siguientes logros y buenas prácticas:

1. **Diseño Arquitectónico Limpio y Estructurado (.NET 10):**
   - Separación estricta de responsabilidades entre presentación (`API`), capa de negocio (`BusinessLogic`), persistencia (`DataAccess`) y contratos comunes (`Shared`).
   - Adopción consistente de Inyección de Dependencias (`IServiceCollection`) en `Program.cs`, evitando instanciaciones acopladas (`new`) en controladores y servicios.
   - Uso de DTOs tipados con validaciones declarativas mediante `DataAnnotations` y atributos personalizados robustos (como `DniListAttribute` con expresiones regulares para validación sintáctica y unicidad local).

2. **Adopción del Result Pattern:**
   - En lugar de propagar excepciones descontroladas para el flujo estándar del dominio, los servicios devuelven objetos `Result<T>` o `Result`, garantizando una gestión predecible de errores de negocio y desacoplando la capa de aplicación del runtime web.

3. **Background Services & Tareas Asíncronas Programadas:**
   - Implementación nativa de `IHostedService` / `BackgroundService` para la gestión de ciclo de vida del negocio:
     - `ReservaTemporalCleanupJob`: Libera stock de carritos expirados periódicamente.
     - `PrecioDinamicoJob`: Evalúa umbrales de aforo para ajuste automático de tarifas.
     - `EventoFinalizadoJob`: Actualiza estados de eventos vencidos sin requerir intervención manual.

4. **Testing Automatizado y E2E:**
   - Presencia de 7 suites de pruebas unitarias (`Moq`, `xUnit`) en `Tests/UnitTests/Services` y 4 controladores con integración (`CustomWebApplicationFactory`) en `Tests/IntegrationTests`.
   - Adición de suite completa de pruebas de API con Bruno (`Documentos/Código/EntradApp/bruno`), permitiendo validar flujos interactivos de forma reproducible.

5. **Generación de Artefactos de Dominio (PDF):**
   - Integración de la librería `QuestPDF` para la emisión del comprobante digital con formato profesional y datos de trazabilidad (DNI, código único, valor pagado).

---

## 3. Análisis de Brechas (Gap Analysis)

El Gap Analysis revela tres vertientes de discrepancia: documental, funcional y técnica.

```
+------------------------------------+       +------------------------------------+
|       DOCUMENTO DE NEGOCIO         |       |       CASOS DE USO (31 CUs)        |
|  - RF-01 a RF-19 (Pagos excluidos) | ----> |  - CU-01 a CU-33 (Brechas CU-18/20)|
|  - Stakeholders: Operador Acceso   |       |  - Omisión de Control de Acceso    |
|  - Reserva 15m / Efectivo estricto |       |  - Reserva 10m / Notificaciones    |
+------------------------------------+       +------------------------------------+
                  \                                     /
                   \                                   /
                    v                                 v
                 +---------------------------------------+
                 |            CÓDIGO FUENTE              |
                 |  - ControlAccesoController (Sin CU)   |
                 |  - Reserva 10m / Pagos simulados      |
                 |  - Race conditions / EF Concurrency   |
                 +---------------------------------------+
```

### 3.1. Fase 1: Documentación vs. Casos de Uso (Trazabilidad, Orfandad y NFRs)
- **Falta de Trazabilidad en Control de Acceso (RF-11, RF-12, RF-13):**  
  En el documento de negocio se definen como requerimientos funcionales clave la validación de tickets mediante código único (RF-11), el bloqueo de entradas reutilizadas (RF-12) y el registro de fecha/hora de validación (RF-13), asociadas al stakeholder "Personal de Control de Acceso". Sin embargo, en la carpeta `Casos_de_uso_TP` **no existe ningún caso de uso** para el control de acceso en puerta (omisión de CU-34 o CU-35).
- **Orfandad y Scope Creep en Casos de Uso del Super Admin (CU-21, CU-31, CU-32, CU-33):**  
  La documentación de negocio en su sección 4.2 inicial afirmaba que no existirían administradores globales y que cada usuario gestionaría sus eventos. No obstante, los Casos de Uso CU-21 y CU-31 a CU-33 introducen la figura de un `Super Admin` con facultades de moderación (aprobar/rechazar eventos con bitácora). Si bien en la sección 5.1 se reincorporaron los RF-16 a RF-18 para moderación, subsiste una contradicción no resuelta en la narrativa de la sección 4.2.
- **Inconsistencia en la Regla Core de Reserva Temporal (RF-05 vs. CU-29):**  
  La documentación en la sección 4.1 y en versiones previas dictamina 15 minutos de retención de inventario en carrito, mientras que el caso de uso `cu-29-habilitar-reserva-temporal.md` establece 10 minutos de forma estricta.
- **Contradicción en Notificaciones:**  
  La sección 4.2 lista taxativamente los canales de notificación (Email, SMS) como *Out of Scope*. Sin embargo, los casos de uso CU-16, CU-21, CU-24, CU-32 y CU-33 establecen pasos explícitos donde el sistema "notifica por email/in-app" al organizador.
- **Reflejo de Requerimientos No Funcionales (NFRs) en Casos de Uso:**  
  Los NFR-03 y NFR-11 exigen tiempos de respuesta menores a 500ms en compra y 300ms en validación de acceso. Ningún caso de uso refleja estos umbrales en sus postcondiciones o criterios de aceptación operativos. Asimismo, NFR-02 (protección y almacenamiento seguro de DNIs) no se traduce en directivas de ofuscación o enmascaramiento en los casos de uso de consulta o comprobantes (CU-10, CU-14).

### 3.2. Fase 2: Diseño vs. Implementación (Código Fuente)
- **Scope Creep / Código Huérfano en `ControlAccesoController`:**  
  El código implementa el endpoint `POST /api/v1/control-acceso/validar` y el método `ValidarEntradaAsync` en `EntradaService`. Esta implementación responde a los RF-11 a RF-13 del negocio, pero representa un *Scope Creep* respecto al catálogo de Casos de Uso aprobados, ya que el equipo codificó el módulo sin contar con la especificación funcional formal (CU).
- **Desfasaje en el Flujo de Pagos (Efectivo vs. Simulación Inmediata):**  
  En `CompraService.cs` (líneas 83-87), la compra se marca como `PendientePago`, se descuenta la reserva, e inmediatamente se auto-aprueba:
  ```csharp
  // Cambiar estado a Aprobado (simulando pago en efectivo inmediato para desarrollo)
  compra.Estado = EstadoCompra.Aprobado;
  compra.FechaPago = DateTime.UtcNow;
  await _compraRepository.UpdateEstadoAsync(compra.Id, EstadoCompra.Aprobado);
  ```
  Esto desvirtúa los endpoints `PATCH /api/v1/compras/{id}/confirmar-pago` y `PATCH /api/v1/compras/{id}/cancelar`, los cuales quedan virtualmente inalcanzables en un flujo ordinario ya que ninguna compra queda en estado `PendientePago`.
- **Precios Dinámicos: Asíncrono vs. Reactivo en Tiempo Real:**  
  El RF-10 prescribe que el sistema incremente un 20% el precio base tan pronto como el aforo supere el 80%. En el código, esto no se ejecuta sincrónicamente durante la transacción de compra, sino en un proceso en segundo plano (`PrecioDinamicoJob`) que corre cada 5 minutos. Si durante esos 5 minutos se venden decenas de entradas superando el 80%, los compradores adquieren entradas al precio viejo, violando la regla de negocio de protección de ingresos.
- **Política de Devoluciones (RF-09 vs. Implementación):**  
  El RF-09 y el CU-15 exigen que la devolución solo proceda si el evento está agotado o activo. El código en `DevolucionService.cs` solo verifica que el evento no haya iniciado (`evento.FechaInicio <= DateTime.UtcNow`), omitiendo la verificación de aforo agotado descrita formalmente en la regla de negocio del documento.

---

## 4. Conclusión de Arquitectura y Calidad Estática

### 4.1. Diagnóstico de Arquitectura y Patrones
- **Puntos Fuertes:** Cumplimiento cabal del patrón repositorio, segregación de interfaces (`ISP`), inversión de dependencias (`DIP`), y uso de middleware global de excepciones (`GlobalExceptionFilter`).
- **Deficiencias Estructurales:**
  - **Falta de Transaccionalidad Atómica (Unit of Work):**  
    En `CompraService.cs` se ejecutan múltiples operaciones de base de datos consecutivas e independientes: `_compraRepository.CreateAsync`, `_entradaRepository.CreateRangeAsync`, `_sectorRepository.IncrementarVendidasAsync`, `_sectorRepository.DecrementarReservadasAsync`, y `_compraRepository.UpdateEstadoAsync`. Al no estar envueltas en una transacción de base de datos (`IDbContextTransaction`), cualquier falla a mitad del proceso (ej. caída de red o error en la generación de entradas) dejará la base de datos en un estado corrupto (compras creadas sin entradas o stock descontado sin registro de compra).
  - **Condición de Carrera (Race Condition / Concurrencia RNF-04):**  
    El método `IncrementarVendidasAsync` y la verificación `ValidarDisponibilidadAsync` leen el stock disponible en memoria y luego actualizan la entidad. En un escenario con 100 usuarios comprando al mismo milisegundo (NFR-04), dos hilos leerán que queda 1 entrada libre, ambos crearán la reserva/compra y se producirá **sobreventa** (overselling). EF Core no tiene configurado un token de concurrencia (`RowVersion` / `IsConcurrencyToken`) en la entidad `Sector`.

### 4.2. Calidad de Código (Clean Code & SOLID)
- **Principio de Responsabilidad Única (SRP):**  
  `CompraService` asume demasiadas responsabilidades: coordina validaciones, calcula montos, invoca reservas, gestiona compras, genera códigos de tickets alfanuméricos (`GenerarCodigoUnico`), crea entidades de entrada y gestiona el paginado de reportes con formateo de fechas.
- **Seguridad y Criptografía Crítica:**  
  La clave secreta para la firma de tokens JWT está hardcodeada directamente en el código de producción (`UsuarioService.cs` línea 128) con el valor literal:  
  `"SuperSecretKeyForDevelopmentOnlyChangeInProduction12345!"`.  
  Cualquier atacante con acceso al repositorio puede forjar tokens con rol `SuperAdmin` y tomar el control total de la API.
- **Almacenamiento y Protección de Datos Personales (RNF-02):**  
  Los números de documento nacional de identidad (DNI) se almacenan en texto plano en las tablas `Usuarios` y `Entradas`, sin encriptación en reposo ni enmascaramiento en las respuestas de endpoints públicos o semipúblicos.

### 4.3. Dictamen Final del Auditor
El proyecto demuestra un excelente dominio de las tecnologías modernas de .NET y una base de ingeniería sumamente prometedora. Sin embargo, para calificar como apto para producción (o entrega final sobresaliente), el equipo debe resolver prioritariamente la consistencia documental (sincronizar los 19 RFs con los 31 CUs y el código de control de acceso), asegurar la clave JWT en variables de entorno, implementar transaccionalidad atómica con control de concurrencia en la compra de entradas y corregir la auto-aprobación del flujo de pagos.

