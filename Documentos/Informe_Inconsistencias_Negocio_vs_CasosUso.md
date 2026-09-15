# Informe de Inconsistencias: Documentación de Negocio vs Casos de Uso Generados

**Fecha:** 14/09/2026
**Proyecto:** EntradApp
**Documento analizado:** `Documentos/Business/Documentación de Proyecto de Software.md`
**Casos de uso analizados:** 31 archivos en `Documentos/TP/` (CU-01 a CU-34, exceptuando CU-18 original y CU-20)

---

## Resumen Ejecutivo

Se identificaron **12 inconsistencias críticas**, **6 ambigüedades** y **4 elementos desfazados** entre la documentación de negocio y los casos de uso generados. Las principales áreas de conflicto son: sistema de pagos, rol de administrador, notificaciones, límites de compra, y validación de entradas (control de acceso).

---

## 1. Inconsistencias Críticas (Bloqueantes para coherencia)

### INC-01: Duración de reserva temporal

| Fuente | Valor |
|--------|-------|
| Documentación (RF-05, Sección 4.1) | **15 minutos** |
| CU-29 (Habilitar reserva temporal) | **10 minutos** (hardcodeado en precondiciones y flujo) |
| **Impacto** | Discrepancia en regla de negocio core del MVP. Afecta testing y configuración. |

### INC-02: Sistema de pagos - Contradicción fundamental

| Documentación (Sección 4.2 - Out of Scope) | Casos de uso / RFs |
|--------------------------------------------|-------------------|
| "NO integración con sistemas externos de pago; el proceso se limitará a pagos en **efectivo**" | RF-11: "validar estado del pago antes de confirmar compra" |
| | RF-12: "cancelar reserva si pago no se completa" |
| | RF-13: "registrar resultado transacción pago (aprobado, rechazado, pendiente)" |
| | RF-14: "generar comprobante **una vez confirmado el pago**" |
| | CU-10 paso 6-8: ingresa DNI → registra compra → genera comprobante (implícito pago) |
| **Impacto** | **Mayor inconsistencia**. La doc dice solo efectivo (sin validación externa), pero 4 RFs y CU-10 asumen flujo de pago con estados. Requiere decisión: ¿simular pago interno? ¿integrar gateway? ¿quitar RFs 11-14? |

### INC-03: Rol Administrador Global vs "Sin admins globales"

| Documentación (Sección 4.2) | Casos de uso generados |
|----------------------------|------------------------|
| "NO gestión de administradores globales de la plataforma. **Todos los usuarios registrados podrán gestionar sus propios eventos**" | CU-31 a CU-34 crean rol **Administrador** con permisos exclusivos: ver pendientes, aprobar, rechazar, consultar historial de TODOS los eventos |
| | CU-21 (Cambiar estado evento) dice "Actor Principal: Administrador" |
| **Impacto** | Decisión de arquitectura: ¿hay admin global o solo creadores auto-gestionan? Si no hay admin, ¿quién aprueba eventos (CU-16 dice "sujeto a aprobación por administrador")? |

### INC-04: Notificaciones - Excluidas en doc, presentes en CUs

| Documentación (Sección 4.2) | Casos de uso que las incluyen |
|----------------------------|------------------------------|
| "NO sistema de notificaciones (email, SMS u otros)" | CU-16 paso 8: "informa que quedará sujeto a aprobación" |
| | CU-21 paso 7: "notifica el resultado al usuario creador" |
| | CU-32 paso 6: "notifica al creador: Tu evento fue aprobado" |
| | CU-33 paso 5: "notifica al creador: Tu evento fue rechazado. Motivo: {motivo}" |
| | CU-24 paso 6: "Usuario notificado (email/in-app) cuando reporte esté listo" |
| **Impacto** | 5 CUs asumen capacidad de notificación que la doc excluye del MVP. |

### INC-05: Validación de entradas / Control de acceso (Faltante)

| Requerimientos funcionales | Casos de uso |
|---------------------------|--------------|
| RF-15: "validar entradas mediante código único asociado a cada ticket" | **No hay CU explícito** |
| RF-16: "impedir uso de entrada ya validada previamente" | **No hay CU explícito** |
| RF-17: "registrar fecha y hora de validación de cada entrada" | **No hay CU explícito** |
| Stakeholder: "Personal de control de acceso" (Sección 3.1) | **Sin CU asociado** |
| **Impacto** | 3 RFs core del negocio (validación en puerta) no tienen CU. El stakeholder "Personal de control de acceso" queda sin representación. |

### INC-06: Límite de entradas por transacción

| Documentación (RF-03) | CU-27 (Validar límite máximo) |
|----------------------|------------------------------|
| "El sistema debe permitir a un usuario solicitar la compra de **entre 1 y 4 entradas por transacción**" | Límite descrito como "configurable por evento o global", **no especifica 4** como máximo duro |
| **Impacto** | Regla de negocio explícita (1-4) no reflejada como constraint en validación. |

### INC-07: Stakeholder "Sistema externo de pagos" contradice Out of Scope

| Sección 3.1 (Stakeholders) | Sección 4.2 (Out of Scope) |
|---------------------------|---------------------------|
| Tabla incluye: "Sistema externo de pagos — Procesar pagos de forma segura... Funciones: Procesar pagos, validación de transacciones" | "NO integración con sistemas externos de pago" |
| **Impacto** | Stakeholder listado pero explícitamente excluido. Confusión en alcance. |

### INC-08: Usuarios registrados vs Visitante - Gestión de eventos

| Documentación (Sección 4.2) | TP Original (Casos_de_uso.md) | CUs generados |
|----------------------------|------------------------------|---------------|
| "Todos los usuarios registrados podrán gestionar sus propios eventos" | Visitante: CU-16 "Crear evento LARI 1°" (usuario registrado) | CU-16: Actor "Usuario registrado" ✓ |
| | **Pero** Visitante también tiene: CU-01 a CU-07 (consultar, buscar, filtrar, ver detalle, registrarse, login, recuperar pass) | CUs 01-07: Actor "Usuario Visitante (No registrado)" ✓ |
| **Impacto** | Coherente en la práctica, pero redacción "Todos los usuarios registrados podrán gestionar" podría interpretarse como "solo registrados acceden a todo", contradiciendo que visitante consulta eventos. |

### INC-09: Precondición "Evento agotado" en devolución

| Documentación (RF-09) | CU-15 (Solicitar devolución) |
|----------------------|------------------------------|
| "permitir procesar devoluciones de entradas en **eventos agotados**" | Precondición: "El Usuario Registrado posee una entrada activa para el **evento agotado**" |
| **Pero** RF-09 dice "eventos agotados" como condición, mientras que en la práctica las devoluciones suelen permitirse antes también. | **Ambigüedad**: ¿solo eventos agotados? ¿o cualquier evento con entrada activa? |

### INC-10: Reportes históricos - Alcance MVP vs CUs generados

| Documentación (Sección 4.1 MVP) | CUs generados |
|--------------------------------|---------------|
| MVP incluye: "Registro de transacciones" | CU-24 (Generar reporte histórico) y CU-25 (Descargar reporte) son **complejos**: async, PDF/Excel, cola de tareas, storage |
| MVP **NO incluye**: reportes históricos avanzados (Sección 4.2 no los menciona explícitamente, pero RF-19 sí) | RF-19 está en funcionales, pero complejidad de CU-24/25 excede MVP básico |
| **Impacto** | CU-24/25 añaden: procesamiento asíncrono, generación PDF/Excel, almacenamiento de archivos, notificación al completar. ¿Están en MVP? |

### INC-11: Precios dinámicos - Disparador automático vs manual

| Documentación (RF-10) | CU-30 (Aplicar incremento dinámico) |
|----------------------|-----------------------------------|
| "incrementar **automáticamente** en 20% el precio base... cuando el aforo vendido supere el 80%" | Actor Principal: **Sistema**. Disparador: "El sistema detecta que el aforo vendido supera el 80%" ✓ Coherente |
| | **Pero** CU-30 es un CU de "Sistema" (automático), mientras que el resto de CUs son actor-humano. ¿Está bien modelado como CU separado? |

### INC-12: Consulta de eventos - Visitante ve solo aprobados

| Documentación (RF-23) | CUs Visitante (CU-01, CU-02, CU-03, CU-04) |
|----------------------|------------------------------------------|
| "impedir la visualización pública de eventos que no hayan sido aprobados" | CU-01, CU-02, CU-03, CU-04 precondiciones/flujo: filtran por estado "Aprobado" ✓ Coherente |
| **Impacto** | Coherente, pero verificar que CU-16 (crear evento) deja evento en "Pendiente" y no visible hasta aprobación (CU-32). |

---

## 2. Ambigüedades (Requieren clarificación)

### AMB-01: ¿Qué significa "gestionar sus propios eventos" sin admin global?
- Si no hay admin global (Sección 4.2), ¿quién aprueba los eventos "Pendiente de aprobación" (CU-16)?
- ¿Los usuarios se auto-aprueban? ¿Hay flujo de revisión automática?
- CU-21, CU-31-34 asumen admin externo que revisa.

### AMB-02: Devoluciones - ¿Solo eventos agotados o siempre?
- RF-09: "en eventos agotados"
- CU-15 precondición: "evento agotado"
- Pero regla de negocio real: ¿se puede devolver entrada de evento no agotado? ¿Cuál es la política real?

### AMB-03: Código único de entrada (RF-15) - ¿Formato? ¿QR? ¿Alfanumérico?
- No especificado en doc. CU-14 (Visualizar comprobante) menciona QR pero no el código único de validación.
- CU faltante para validación (INC-05) necesitaría esta definición.

### AMB-04: "Personal de control de acceso" - ¿Usuario del sistema o actor externo?
- Stakeholder en Sección 3.1, pero ¿tiene login? ¿Usa API? ¿Escanea QR con app móvil?
- Sin CU, no hay trazabilidad de este actor.

### AMB-05: Límite de compra - ¿Por transacción o por evento acumulado?
- RF-03: "por transacción" (1-4)
- CU-27: "límite máximo de entradas para el evento" (acumulado: activas + utilizadas)
- ¿Son dos límites distintos? ¿Uno por transacción (max 4) y otro acumulado por evento?

### AMB-06: Estados de evento - ¿Cuáles son exactamente?
- Documentación menciona: "Pendiente de aprobación", "Aprobado", "Rechazado", "Cancelado", "Finalizado", "Borrador" (dispersos en CUs)
- No hay tabla maestra de estados y transiciones válidas en doc.
- CU-21 (Cambiar estado) y CU-17 (Modificar evento) referencian estados distintos.

---

## 3. Elementos Desfazados / Obsoletos

### DES-01: CU-18 (Gestionar sectores) - Numeración
- TP original: CU-19 "Gestionar sectores" (saltaba CU-18)
- Generado como CU-18 (renumerado para llenar gap)
- **Riesgo**: Trazabilidad con TP original rota. Referencias externas a "CU-19" ahora apuntan a CU-18.

### DES-02: CU-20 - Gap numérico intencional
- CU-21 existe (Cambiar estado evento)
- CU-22 existe (Consultar estadísticas)
- CU-20 quedó vacío (era CU-21 en TP original)
- **Decisión**: ¿Mantener gap? ¿Renumerar CU-23→20, CU-24→21...? (Rompe CU-21, CU-22 existentes)

### DES-03: RFs 11-14 (Pagos) - Posiblemente obsoletos para MVP
- Si se confirma "solo efectivo, sin gateway" (Sección 4.2), estos RFs deberían moverse a "Futuro" o reescribirse como "simulación de pago interno".
- CU-10 los asume reales.

### DES-04: Notificaciones en CUs - Obsoletas si se respeta Out of Scope
- 5 CUs incluyen pasos de notificación (email/in-app)
- Si MVP excluye notificaciones, esos pasos deben quitarse o marcarse como "Futuro".

---

## 4. Matriz de Trazabilidad: RF → CU (Cobertura)

| RF | Descripción | CU(s) Asociado(s) | Estado |
|----|-------------|-------------------|--------|
| RF-01 | Crear evento | CU-16 | ✅ Cubierto |
| RF-02 | Consultar disponibilidad | CU-01, CU-02, CU-03, CU-04 | ✅ Cubierto |
| RF-03 | Compra 1-4 entradas | CU-10, CU-27 | ⚠️ Límite 4 no explícito en CU-27 |
| RF-04 | Rechazar por cupo | CU-26 | ✅ Cubierto |
| RF-05 | Reserva temporal 15 min | CU-29 | ❌ **Inconsistencia**: 10 min en CU |
| RF-06 | Cargar DNI por entrada | CU-10 | ✅ Cubierto |
| RF-07 | Validar DNI duplicados | CU-10, CU-28 | ✅ Cubierto |
| RF-08 | Registrar transacción | CU-10 | ✅ Cubierto |
| RF-09 | Devoluciones 80% | CU-15 | ✅ Cubierto |
| RF-10 | Precio dinámico +20% | CU-30 | ✅ Cubierto |
| RF-11 | Validar estado pago | CU-10 | ❌ **Conflicto** con Out of Scope |
| RF-12 | Cancelar reserva si no paga | CU-10, CU-29 | ❌ **Conflicto** con Out of Scope |
| RF-13 | Registrar resultado pago | CU-10 | ❌ **Conflicto** con Out of Scope |
| RF-14 | Generar comprobante | CU-14 | ❌ **Conflicto** (post-pago) |
| RF-15 | Validar entrada código único | **NINGUNO** | ❌ **FALTA CU** |
| RF-16 | Impedir reuso entrada | **NINGUNO** | ❌ **FALTA CU** |
| RF-17 | Registrar fecha validación | **NINGUNO** | ❌ **FALTA CU** |
| RF-18 | Reportes estadísticos | CU-22, CU-23 | ✅ Cubierto |
| RF-19 | Reportes históricos | CU-24, CU-25 | ⚠️ Complejidad excede MVP |
| RF-20 | Admin ver pendientes | CU-31 | ❌ **Conflicto** (no hay admin global) |
| RF-21 | Admin aprobar | CU-32 | ❌ **Conflicto** (no hay admin global) |
| RF-22 | Admin rechazar con motivo | CU-33 | ❌ **Conflicto** (no hay admin global) |
| RF-23 | Impedir visualizar no aprobados | CU-01, CU-02, CU-03, CU-04 | ✅ Cubierto |

**Resumen cobertura:** 14/23 RFs cubiertos sin conflictos, 5 con conflictos críticos, 3 faltantes, 1 con ambigüedad.

---

## 5. Decisiones Requeridas (Para el Analista)

| # | Decisión | Opciones | Recomendación Inicial |
|---|----------|----------|----------------------|
| 1 | **Sistema de pagos** | A) Simular pago interno (mock) B) Integrar gateway real C) Quitar RFs 11-14 del MVP | A para MVP, B para v2 |
| 2 | **Rol Administrador** | A) Mantener admin global (CUs 31-34) B) Quitar admin, auto-aprobación C) Admin solo para moderación, creadores auto-gestionan | A (más realista), documentar excepción a Sección 4.2 |
| 3 | **Notificaciones** | A) Quitar de CUs (respetar Out of Scope) B) Implementar básico (log/DB) C) Mover a v2 | A para MVP, registro en BD para auditoría |
| 4 | **Reserva temporal** | A) Cambiar CU-29 a 15 min B) Cambiar doc a 10 min | A (doc es fuente de verdad) |
| 5 | **Validación entradas (RF-15,16,17)** | A) Crear CU-35 "Validar entrada en acceso" B) Mover a v2 | A (core business, stakeholder existe) |
| 6 | **Límite compra** | A) Hardcodear 4 en CU-27 B) Configurable con default 4 | B (flexible) |
| 7 | **CU-24/25 Reportes históricos** | A) Mantener en MVP B) Mover a v2 (simplificar MVP) | B (MVP = registro transacciones, no reportes async) |
| 8 | **Numeración CUs** | A) Mantener gaps (CU-18, CU-20) B) Renumerar todo C) Documentar mapping | A (menos riesgo, documentar) |

---

## 6. Próximos Pasos Sugeridos

1. **Sesión de decisión** con el analista para resolver items 1-8 arriba
2. **Actualizar documentación de negocio** reflejando decisiones (versión 1.1)
3. **Ajustar CUs afectados** según decisiones (principalmente: CU-10, CU-14, CU-15, CU-21, CU-27, CU-29, CU-31 a CU-34)
4. **Crear CU faltante**: "Validar entrada en acceso" (RF-15,16,17) para stakeholder "Personal de control de acceso"
5. **Definir máquina de estados** de evento (tabla maestra transiciones)
6. **Actualizar matriz trazabilidad** RF↔CU final

---

## Archivos de Referencia

- Documentación negocio: `Documentos/Business/Documentación de Proyecto de Software.md`
- Casos de uso: `Documentos/TP/cu-*.md` (31 archivos)
- TP Original: `Documentos/TP/Casos_de_uso.md`
- Guía especificación: `Documentos/Ejemplos Oscar/GUIA-Especificacion-Casos-de-Uso.md`
- Ejemplo CU-02: `Documentos/Ejemplos Oscar/CU-02 Alta de Medico.md`
