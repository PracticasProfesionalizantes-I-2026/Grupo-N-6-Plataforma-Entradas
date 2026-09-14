# Documentación de Proyecto de Software

**Nombre del proyecto:** EntradApp

**Integrantes:** Lara Glaría, Paula Quevedo

**Año:** 2026

**Facultad / Carrera:** Instituto Cooperativo de Enseñanza Superior (ICES), Centro Universitario Sunchales (CUS) — Técnico Superior en Desarrollo de Software.

---

## 1. Requerimientos del Negocio

### 1.1 Situación actual o Propósito

Actualmente, la venta de entradas se realiza a través de múltiples canales, como boleterías físicas, plataformas digitales poco integradas y sistemas manuales, lo que genera falta de control sobre la disponibilidad en tiempo real, derivando en sobreventa e inconsistencias en el aforo.

Además, la ausencia de validaciones adecuadas dificulta la trazabilidad de las entradas y favorece prácticas fraudulentas, mientras que la falta de límites en la cantidad de compra afecta la equidad en el acceso. A su vez, no existen procesos automatizados para la gestión de devoluciones y reventas oficiales.

En este contexto, surge la necesidad de desarrollar una plataforma de venta de entradas que permita gestionar eficientemente el stock, prevenir fraudes y garantizar un control preciso del aforo y de las transacciones.

### 1.2 Oportunidad del negocio

El desarrollo de una plataforma de venta de entradas permitirá centralizar y automatizar la gestión del stock, garantizando control en tiempo real del aforo, validación de identidad de los compradores y aplicación estricta de reglas de negocio para prevenir fraudes y sobreventa.

La oportunidad de negocio radica en mejorar la transparencia y eficiencia del proceso de compra, optimizando la experiencia del usuario y reduciendo pérdidas operativas. El sistema operará en un entorno digital, accesible mediante APIs, facilitando su integración con aplicaciones web o móviles.

Se descartan soluciones existentes del mercado debido a que no contemplan requerimientos específicos como:

- Control de concurrencia.
- Trazabilidad mediante DNIs.
- Limitación de entradas por transacción.
- Reventa oficial automatizada.
- Implementación de precios dinámicos según la demanda.

### 1.3 Riesgos

| # | Riesgo | Severidad | Mitigación |
|---|--------|-----------|------------|
| 1 | Fallos en el control de concurrencia que permitan la sobreventa de entradas | Alta | Implementar mecanismos de bloqueo y testing exhaustivo de escenarios simultáneos. |
| 2 | Errores en la validación de datos (por ejemplo, DNIs duplicados) que afecten la trazabilidad | Media | Validaciones estrictas a nivel backend. |
| 3 | Resistencia al uso del sistema por parte de usuarios o administradores acostumbrados a métodos tradicionales | Media | Diseño intuitivo y documentación clara. |
| 4 | Caídas del sistema o problemas de infraestructura que afecten la disponibilidad durante picos de demanda | Alta | Uso de infraestructura escalable y monitoreo continuo. |
| 5 | Retrasos en el desarrollo debido a la complejidad de las reglas de negocio (concurrencia, precios dinámicos, etc.) | Alta | Planificación por etapas y testing incremental. |

---

## 2. Visión de la Solución

### 2.1 Funciones principales

- Gestión de eventos y sectores (creación, configuración de aforo y precios base por sector).
- Módulo de venta de entradas con control de disponibilidad en tiempo real y validación de reglas de negocio.
- Gestión de carrito de compras con reserva temporal de entradas (bloqueo de stock por tiempo limitado).
- Validación de identidad de asistentes mediante carga de DNIs para garantizar trazabilidad y evitar duplicados.
- Control de límites de compra por usuario para asegurar una distribución equitativa de entradas.
- Módulo de devoluciones y reventa oficial con actualización automática del stock disponible.
- Sistema de precios dinámicos basado en el nivel de ocupación de cada sector.
- Registro y trazabilidad de transacciones para auditoría y control.
- Módulo de reportes y estadísticas de ventas (entradas vendidas, ingresos generados y nivel de ocupación por evento y sector).
- Generación de reportes históricos para análisis de demanda, comportamiento de compra y rendimiento de eventos.

---

## 3. Contexto del Negocio

### 3.1 Perfil de los interesados (Stakeholders)

| Stakeholder | Beneficio y valor percibido | Actitudes | Funciones de interés mayor | Restricciones |
|-------------|------------------------------|-----------|------------------------------|----------------|
| Organizador del evento | Maximizar ventas y controlar el aforo sin errores. | Muy interesado, enfocado en resultados y control. | Gestión de eventos, reportes de ventas, control de stock. | Necesita alta confiabilidad, no tolera sobreventa ni fallos. |
| Usuarios compradores | Comprar entradas de forma rápida, segura y transparente. | Alta expectativa de inmediatez y facilidad de uso. | Compra de entradas, selección de sector, carga de datos. | Debe ser simple y rápido. |
| Administrador | Garantizar la calidad y legitimidad de los eventos publicados. | Responsable, requiere claridad y control. | Gestión de eventos. | Necesita una interfaz clara y datos consistentes. |
| Sistema externo de pagos | Procesar pagos de forma segura y confiable. | Enfocado en seguridad y validación de transacciones. | Procesar pagos, validación de operaciones. | Cumplimiento de normas de tiempos de respuesta. |
| Personal de control de acceso | Validar entradas de forma rápida y sin errores. | Necesita rapidez y confiabilidad. | Escaneo y validación de entradas. | El sistema debe ser ágil y funcionar incluso con alta concurrencia. |

---

## 4. Alcance y Limitaciones

### 4.1 Alcance inicial (MVP - Minimum Viable Product)

La versión 1.0 del sistema (MVP) incluirá:

- Gestión básica de eventos y sectores (configuración de aforo y precios base).
- Lógica principal de venta de entradas con control de disponibilidad en tiempo real y validación de reglas de negocio (límite de entradas por compra y verificación de cupo).
- Gestión de carrito de compras con reserva temporal de stock por **15 minutos**.
- Carga obligatoria de DNIs por entrada, con validación de duplicados para garantizar la trazabilidad.
- Registro de transacciones.

### 4.2 Limitaciones y exclusiones (Out of Scope)

En esta primera versión del sistema **NO** se incluirá:

- Desarrollo de una interfaz de usuario completa (frontend); el sistema se limitará a su uso mediante API y herramientas de prueba o cliente básico.
- Implementación de modelos de machine learning o validaciones automatizadas avanzadas sobre identidad.
- Sistema de notificaciones (email, SMS u otros).
- Integración con sistemas externos de pago; el proceso se limitará a pagos en efectivo.
- Gestión de administradores globales de la plataforma. Todos los usuarios registrados podrán gestionar sus propios eventos y consultar la información asociada a ellos.

---

## 5. Requerimientos

### 5.1 Requerimientos Funcionales

| Código | Descripción |
|--------|--------------|
| RF-01 | El sistema debe permitir crear un nuevo evento con estado inicial "Pendiente de aprobación", indicando nombre, fecha, ubicación, sectores disponibles, capacidad máxima y precio base por sector. |
| RF-02 | El sistema debe permitir consultar la disponibilidad de entradas por evento y sector en tiempo real. |
| RF-03 | El sistema debe permitir a un usuario solicitar la compra de entre 1 y 4 entradas por transacción. |
| RF-04 | El sistema debe rechazar la solicitud de compra cuando la cantidad solicitada supere el cupo remanente del sector. |
| RF-05 | El sistema debe bloquear temporalmente el stock solicitado en el carrito durante 15 minutos antes de liberarlo automáticamente. |
| RF-06 | El sistema debe requerir la carga de un DNI por cada entrada solicitada. |
| RF-07 | El sistema debe validar que no existan DNIs duplicados para el mismo evento, rechazando la operación en caso de repetición. |
| RF-08 | El sistema debe registrar cada transacción realizada, almacenando usuario, evento, sector, cantidad de entradas y DNIs asociados. |
| RF-09 | El sistema debe permitir procesar devoluciones de entradas en eventos agotados, aplicando un reembolso del 80% y reponiendo automáticamente el stock disponible. |
| RF-10 | El sistema debe incrementar automáticamente en 20% el precio base de un sector cuando el aforo vendido supere el 80% de su capacidad. |
| RF-11 | El sistema debe validar el estado del pago antes de confirmar la compra de entradas. |
| RF-12 | El sistema debe cancelar la reserva de entradas si el pago no se completa dentro del tiempo establecido. |
| RF-13 | El sistema debe registrar el resultado de cada transacción de pago (aprobado, rechazado o pendiente). |
| RF-14 | El sistema debe generar un comprobante de compra una vez confirmado el pago. |
| RF-15 | El sistema debe permitir validar entradas mediante un código único asociado a cada ticket. |
| RF-16 | El sistema debe impedir el uso de una entrada que ya haya sido validada previamente. |
| RF-17 | El sistema debe registrar la fecha y hora de validación de cada entrada. |
| RF-18 | El sistema debe permitir generar reportes estadísticos de ventas por evento y sector, mostrando la cantidad de entradas vendidas, disponibles y el porcentaje de ocupación. |
| RF-19 | El sistema debe permitir consultar reportes históricos de eventos, incluyendo ingresos generados, cantidad de entradas vendidas y nivel de ocupación alcanzado. |
| RF-20 | El sistema debe permitir a un administrador visualizar los eventos pendientes de aprobación. |
| RF-21 | El administrador debe poder aprobar un evento para que sea publicado en la plataforma. |
| RF-22 | El administrador debe poder rechazar un evento indicando el motivo del rechazo. |
| RF-23 | El sistema debe impedir la visualización pública de eventos que no hayan sido aprobados. |

> **Nota:** Se corrigió la numeración duplicada del RF-16 original (había dos requerimientos con ese código); a partir de allí se renumeraron correlativamente hasta RF-23.

### 5.2 Requerimientos No Funcionales

| Código | Categoría | Descripción |
|--------|-----------|--------------|
| RNF-01 | Seguridad | Los datos sensibles de los usuarios deben ser protegidos mediante mecanismos de autenticación y autorización, evitando accesos no autorizados. |
| RNF-02 | Seguridad | La información de los DNIs debe almacenarse de forma segura, garantizando la confidencialidad y el cumplimiento de buenas prácticas de protección de datos. |
| RNF-03 | Rendimiento | Las consultas de disponibilidad y compra de entradas deben responder en un tiempo menor a 500 ms en condiciones normales. |
| RNF-04 | Concurrencia | El sistema debe ser capaz de manejar múltiples solicitudes simultáneas sin generar inconsistencias en el stock de entradas. |
| RNF-05 | Disponibilidad | El sistema debe estar disponible durante todo el tiempo de operación del evento, minimizando caídas en momentos de alta demanda. |
| RNF-06 | Escalabilidad | El sistema debe poder escalar para soportar picos de alta demanda en eventos masivos. |
| RNF-07 | Arquitectura | El backend debe desarrollarse como una API REST, siguiendo una arquitectura en capas que favorezca la mantenibilidad y escalabilidad. |
| RNF-08 | Usabilidad | La API debe estar documentada mediante herramientas como Swagger/OpenAPI para facilitar su uso. |
| RNF-09 | Mantenibilidad | El código debe estar estructurado de forma clara y modular, permitiendo futuras modificaciones o ampliaciones sin afectar el sistema completo. |
| RNF-10 | Integridad de datos | El sistema debe garantizar la consistencia de la información en todas las operaciones, especialmente en la gestión de stock y transacciones. |
| RNF-11 | Rendimiento | El sistema debe validar entradas en un tiempo menor a 300 ms para garantizar fluidez en accesos masivos. |
| RNF-12 | Disponibilidad | El sistema debe garantizar alta disponibilidad durante eventos, especialmente en procesos de validación de entradas. |