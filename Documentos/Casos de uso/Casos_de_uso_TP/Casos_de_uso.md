---
title: "[]{#_9pqjywnyakzy .anchor}Casos de uso"
---

## Proyecto: EntradApp

## 

## 

## 

## 

## 

## 

## 

## 

## 

## 

## 

## 

Integrantes: Lara Glaría, Paula Quevedo

Fecha: 11/07/2026

## **Usuario Visitante (No registrado)**

Casos de uso:

-   CU - 1: Consultar eventos

-   CU - 2: Buscar eventos

-   CU - 3: Filtrar eventos

-   CU - 4:Ver detalle de evento

-   CU - 5: Registrarse

-   CU - 6: Iniciar sesión

-   CU - 7: Recuperar contraseña

### **Usuario Registrado**

#### Casos de uso:

-   CU - 8: Modificar datos personales

-   CU - 9: Modificar datos de seguridad

```{=html}
<!-- -->
```
-   [CU - 10: Comprar entradas PAU 1°]{.mark}

-   CU - 11: Consultar entradas activas

-   CU - 12: Consultar entradas utilizadas

-   CU - 13: Consultar entradas devueltas

-   CU - 14: Visualizar comprobante

-   [CU - 15: Solicitar devolución PAU 2°]{.mark}

```{=html}
<!-- -->
```
-   [CU - 16: Crear evento LARI 1°]{.mark}

-   CU - 17: Modificar evento

-   CU - 19: Gestionar sectores

-   CU - 20: Consultar eventos creados

-   [CU - 21: Cambiar estado de evento LARI 2°]{.mark}

```{=html}
<!-- -->
```
-   [CU - 22: Consultar estadísticas de ventas LARI 3°]{.mark}

-   CU - 23: Consultar porcentaje de ocupación

-   CU - 24: Generar reporte histórico

-   CU - 25: Descargar reporte

### **Sistema**

-   CU - 26: Validar disponibilidad de entradas

-   CU - 27: Validar límite máximo de compra

-   CU - 28: Validar DNIs duplicados

-   CU - 29: Habilitar reserva temporal de entradas

-   [CU - 30: Aplicar incremento dinámico de precios PAU 3°]{.mark}

**Administrador**

-   Revisar eventos pendientes

-   Aprobar evento

-   Rechazar evento

-   Consultar historial de eventos aprobados/rechazados

[CREAR EVENTO]{.mark}

+-----------------------------------+-----------------------------------+
| **ID / NOMBRE:**                  | CU - 16: CREAR EVENTO             |
+===================================+===================================+
| **DESCRIPCIÓN:**                  | Permite a un usuario registrado   |
|                                   | crear un evento ingresando la     |
|                                   | información requerida para su     |
|                                   | posterior revisión y aprobación.  |
+-----------------------------------+-----------------------------------+
| **ACTOR PRINCIPAL:**              | Usuario registrado                |
+-----------------------------------+-----------------------------------+
| **ACTORES SECUNDARIOS:**          | Sistema, Administrador.           |
+-----------------------------------+-----------------------------------+
| **DISPARADOR (TRIGGER):**         | El usuario registrado selecciona  |
|                                   | la opción \"Crear Evento\" desde  |
|                                   | la plataforma.                    |
+-----------------------------------+-----------------------------------+
| **PRECONDICIONES:**               | El usuario inició sesión en la    |
|                                   | plataforma.                       |
+-----------------------------------+-----------------------------------+
| **FLUJO PRINCIPAL:**              | 1.  El usuario registrado         |
|                                   |     selecciona la opción \"Crear  |
|                                   |     Evento\".                     |
|                                   |                                   |
|                                   | 2.  El sistema muestra el         |
|                                   |     formulario de creación de     |
|                                   |     eventos.                      |
|                                   |                                   |
|                                   | 3.  El usuario completa la        |
|                                   |     información requerida del     |
|                                   |     evento.                       |
|                                   |                                   |
|                                   | 4.  El usuario configura los      |
|                                   |     sectores del evento,          |
|                                   |     indicando capacidad y precio  |
|                                   |     base de cada uno.             |
|                                   |                                   |
|                                   | 5.  El usuario confirma el        |
|                                   |     registro del evento.          |
|                                   |                                   |
|                                   | 6.  El sistema valida que los     |
|                                   |     datos ingresados sean         |
|                                   |     correctos y estén completos.  |
|                                   |                                   |
|                                   | 7.  El sistema registra el evento |
|                                   |     con estado \"Pendiente de     |
|                                   |     aprobación\" y lo asocia al   |
|                                   |     usuario creador.              |
|                                   |                                   |
|                                   | 8.  El sistema informa que el     |
|                                   |     evento fue registrado         |
|                                   |     correctamente y que quedará   |
|                                   |     sujeto a aprobación por parte |
|                                   |     de un administrador.          |
+-----------------------------------+-----------------------------------+
| **FLUJOS ALTERNATIVOS:**          | 1.  **Campos obligatorios         |
|                                   |     incompletos** (El usuario     |
|                                   |     intenta registrar el evento   |
|                                   |     sin completar alguno de los   |
|                                   |     campos obligatorios, el       |
|                                   |     sistema informa el error,     |
|                                   |     resalta los campos            |
|                                   |     correspondientes y solicita   |
|                                   |     completar la información. El  |
|                                   |     caso de uso retorna al paso 3 |
|                                   |     del flujo principal).         |
|                                   |                                   |
|                                   | 2.  **Datos inválidos** (El       |
|                                   |     usuario ingresa información   |
|                                   |     con un formato incorrecto,    |
|                                   |     por ejemplo, una fecha        |
|                                   |     pasada, capacidad negativa o  |
|                                   |     precio inválido, el sistema   |
|                                   |     muestra un mensaje de error y |
|                                   |     solicita corregir los datos.  |
|                                   |     El caso de uso retorna al     |
|                                   |     paso 3 del flujo principal).  |
|                                   |                                   |
|                                   | 3.  **Cancelación del registro**  |
|                                   |     (El usuario decide cancelar   |
|                                   |     la creación del evento antes  |
|                                   |     de confirmarlo, el sistema    |
|                                   |     descarta la información       |
|                                   |     ingresada y regresa al        |
|                                   |     listado de eventos del        |
|                                   |     usuario. El caso de uso       |
|                                   |     finaliza).                    |
|                                   |                                   |
|                                   | 4.  **Error al registrar el       |
|                                   |     evento** (Ocurre un           |
|                                   |     inconveniente durante el      |
|                                   |     almacenamiento de la          |
|                                   |     información, el sistema       |
|                                   |     informa que no fue posible    |
|                                   |     registrar el evento y         |
|                                   |     solicita intentar nuevamente  |
|                                   |     más tarde. El caso de uso     |
|                                   |     finaliza.                     |
+-----------------------------------+-----------------------------------+
| **POSTCONDICIONES:**              | El evento queda registrado en el  |
|                                   | sistema.                          |
|                                   |                                   |
|                                   | El evento queda asociado al       |
|                                   | usuario que lo creó.              |
|                                   |                                   |
|                                   | El evento se almacena con estado  |
|                                   | **\"Pendiente de aprobación\"**.  |
|                                   |                                   |
|                                   | El evento queda disponible para   |
|                                   | su revisión por parte de un       |
|                                   | administrador.                    |
+-----------------------------------+-----------------------------------+
| **REGLAS DE NEGOCIO:**            | Todo evento registrado deberá     |
|                                   | quedar con estado \"Pendiente de  |
|                                   | aprobación\" hasta ser revisado   |
|                                   | por un administrador.             |
|                                   |                                   |
|                                   | Solo los eventos con estado       |
|                                   | \"Aprobado\" podrán ser visibles  |
|                                   | para los usuarios visitantes y    |
|                                   | estar disponibles para la venta   |
|                                   | de entradas.                      |
+-----------------------------------+-----------------------------------+

[CAMBIAR ESTADO DE UN EVENTO]{.mark}

+-----------------------------------+-----------------------------------+
| **ID / NOMBRE:**                  | CU-21: CAMBIAR ESTADO DE UN       |
|                                   | EVENTO                            |
+===================================+===================================+
| **DESCRIPCIÓN:**                  | Permite al administrador revisar  |
|                                   | un evento y modificar su estado   |
|                                   | de acuerdo con las reglas de      |
|                                   | negocio establecidas, habilitando |
|                                   | o rechazando su publicación en la |
|                                   | plataforma.                       |
+-----------------------------------+-----------------------------------+
| **ACTOR PRINCIPAL:**              | Administrador.                    |
+-----------------------------------+-----------------------------------+
| **ACTORES SECUNDARIOS:**          | Sistema, usuario registrado.      |
+-----------------------------------+-----------------------------------+
| **DISPARADOR (TRIGGER):**         | El Administrador selecciona la    |
|                                   | opción para modificar el estado   |
|                                   | de un evento.                     |
+-----------------------------------+-----------------------------------+
| **PRECONDICIONES:**               | El evento debe existir.           |
|                                   |                                   |
|                                   | El actor debe tener permisos para |
|                                   | modificar el estado del evento.   |
|                                   |                                   |
|                                   | El evento debe encontrarse en un  |
|                                   | estado que permita la transición  |
|                                   | solicitada.                       |
+-----------------------------------+-----------------------------------+
| **FLUJO PRINCIPAL:**              | 1.  El administrador accede al    |
|                                   |     listado de eventos pendientes |
|                                   |     de revisión.                  |
|                                   |                                   |
|                                   | 2.  El sistema muestra los        |
|                                   |     eventos con estado            |
|                                   |     \"Pendiente de aprobación\".  |
|                                   |                                   |
|                                   | 3.  El administrador selecciona   |
|                                   |     un evento para revisar.       |
|                                   |                                   |
|                                   | 4.  El sistema muestra el detalle |
|                                   |     completo del evento.          |
|                                   |                                   |
|                                   | 5.  El administrador selecciona   |
|                                   |     la opción \"Aprobar\" o       |
|                                   |     \"Rechazar\".                 |
|                                   |                                   |
|                                   | 6.  El sistema valida la          |
|                                   |     transición de estado y        |
|                                   |     actualiza el estado del       |
|                                   |     evento.                       |
|                                   |                                   |
|                                   | 7.  El sistema registra el cambio |
|                                   |     de estado y notifica el       |
|                                   |     resultado al usuario creador  |
|                                   |     del evento.                   |
+-----------------------------------+-----------------------------------+
| **FLUJOS ALTERNATIVOS:**          | 1.  **El evento ya no se          |
|                                   |     encuentra en estado           |
|                                   |     \"Pendiente de aprobación\"** |
|                                   |     (El sistema informa que el    |
|                                   |     evento ya fue procesado y no  |
|                                   |     permite modificar su estado). |
|                                   |                                   |
|                                   | 2.  **Ocurre un error al          |
|                                   |     actualizar el estado** (El    |
|                                   |     sistema informa que no fue    |
|                                   |     posible completar la          |
|                                   |     operación y solicita          |
|                                   |     intentarlo nuevamente).       |
+-----------------------------------+-----------------------------------+
| **POSTCONDICIONES:**              | El estado del evento queda        |
|                                   | actualizado según la decisión del |
|                                   | administrador.                    |
|                                   |                                   |
|                                   | El cambio queda registrado en el  |
|                                   | sistema.                          |
|                                   |                                   |
|                                   | Los eventos aprobados quedan      |
|                                   | publicados y disponibles para los |
|                                   | usuarios de la plataforma; los    |
|                                   | eventos rechazados permanecen     |
|                                   | ocultos y no pueden comercializar |
|                                   | entradas.                         |
+-----------------------------------+-----------------------------------+
| **REGLAS DE NEGOCIO:**            | Solo un administrador podrá       |
|                                   | aprobar o rechazar eventos.       |
|                                   |                                   |
|                                   | Solo los eventos con estado       |
|                                   | \"Aprobado\" serán visibles para  |
|                                   | los usuarios visitantes y podrán  |
|                                   | habilitar la venta de entradas.   |
|                                   |                                   |
|                                   | Una vez rechazado un evento, no   |
|                                   | podrá publicarse sin una nueva    |
|                                   | revisión administrativa.          |
+-----------------------------------+-----------------------------------+

[CONSULTAR ESTADÍSTICAS DE VENTAS]{.mark}

+-----------------------------------+-----------------------------------+
| **ID / NOMBRE:**                  | CU-22: CONSULTAR ESTADÍSTICAS DE  |
|                                   | VENTAS                            |
+===================================+===================================+
| **DESCRIPCIÓN:**                  | Permite al usuario registrado     |
|                                   | consultar información estadística |
|                                   | sobre las ventas de sus eventos.  |
+-----------------------------------+-----------------------------------+
| **ACTOR PRINCIPAL:**              | Usuario Registrado.               |
+-----------------------------------+-----------------------------------+
| **ACTORES SECUNDARIOS:**          | Sistema.                          |
+-----------------------------------+-----------------------------------+
| **DISPARADOR (TRIGGER):**         | El usuario registrado selecciona  |
|                                   | la opción \"Estadísticas\" de uno |
|                                   | de sus eventos.                   |
+-----------------------------------+-----------------------------------+
| **PRECONDICIONES:**               | El usuario debe estar autenticado |
|                                   | en la plataforma.                 |
|                                   |                                   |
|                                   | El usuario debe ser propietario   |
|                                   | del evento.                       |
|                                   |                                   |
|                                   | El evento debe existir.           |
+-----------------------------------+-----------------------------------+
| **FLUJO PRINCIPAL:**              | 1.  El usuario accede al listado  |
|                                   |     de sus eventos.               |
|                                   |                                   |
|                                   | 2.  El usuario selecciona el      |
|                                   |     evento del cual desea         |
|                                   |     consultar las estadísticas.   |
|                                   |                                   |
|                                   | 3.  El sistema obtiene la         |
|                                   |     información correspondiente   |
|                                   |     al evento seleccionado.       |
|                                   |                                   |
|                                   | 4.  El sistema calcula las        |
|                                   |     estadísticas de ventas y      |
|                                   |     ocupación.                    |
|                                   |                                   |
|                                   | 5.  El sistema muestra al usuario |
|                                   |     la información estadística    |
|                                   |     del evento.                   |
+-----------------------------------+-----------------------------------+
| **FLUJOS ALTERNATIVOS:**          | 1.  **Ocurre un error al          |
|                                   |     recuperar la información**    |
|                                   |     (El sistema informa que no    |
|                                   |     fue posible obtener las       |
|                                   |     estadísticas y solicita       |
|                                   |     intentar nuevamente más       |
|                                   |     tarde).                       |
+-----------------------------------+-----------------------------------+
| **POSTCONDICIONES:**              | Las estadísticas del evento son   |
|                                   | visualizadas por el usuario.      |
+-----------------------------------+-----------------------------------+
| **REGLAS DE NEGOCIO:**            | Solo el propietario del evento    |
|                                   | podrá consultar las estadísticas  |
|                                   | correspondientes a dicho evento.  |
|                                   |                                   |
|                                   | El sistema deberá mostrar         |
|                                   | información actualizada al        |
|                                   | momento de realizar la consulta.  |
+-----------------------------------+-----------------------------------+

[COMPRAR ENTRADAS]{.mark}

+-----------------------------------+-----------------------------------+
| **ID / NOMBRE:**                  | CU - 10: COMPRAR ENTRADAS         |
+===================================+===================================+
| **DESCRIPCIÓN:**                  | Permite a un Usuario Registrado   |
|                                   | comprar entradas para un evento   |
|                                   | disponible.                       |
+-----------------------------------+-----------------------------------+
| **ACTOR PRINCIPAL:**              | Usuario Registrado                |
+-----------------------------------+-----------------------------------+
| **ACTORES SECUNDARIOS:**          | No aplica                         |
+-----------------------------------+-----------------------------------+
| **DISPARADOR (TRIGGER):**         | El Usuario Registrado selecciona  |
|                                   | la opción "Comprar entradas" para |
|                                   | un evento disponible.             |
+-----------------------------------+-----------------------------------+
| **PRECONDICIONES:**               | El evento se encuentra aprobado y |
|                                   | dispone de entradas disponibles.  |
+-----------------------------------+-----------------------------------+
| **FLUJO PRINCIPAL**               | 1\. El Usuario Registrado         |
|                                   | selecciona un evento.             |
|                                   |                                   |
|                                   | 2\. El Sistema muestra los        |
|                                   | sectores disponibles              |
|                                   |                                   |
|                                   | 3\. El Usuario Registrado         |
|                                   | selecciona el sector y la         |
|                                   | cantidad de entradas.             |
|                                   |                                   |
|                                   | 4\. El Sistema valida la          |
|                                   | disponibilidad de entradas.       |
|                                   |                                   |
|                                   | 5\. El Sistema reserva            |
|                                   | temporalmente las entradas        |
|                                   | seleccionadas.                    |
|                                   |                                   |
|                                   | 6\. El Usuario Registrado ingresa |
|                                   | un DNI por cada entrada.          |
|                                   |                                   |
|                                   | 7\. El Sistema registra la compra |
|                                   | y actualiza el stock.             |
|                                   |                                   |
|                                   | 8\. El Sistema genera el          |
|                                   | comprobante de la compra.         |
+-----------------------------------+-----------------------------------+
| **FLUJO ALTERNATIVO**             | **5a. DNI duplicado.**            |
|                                   |                                   |
|                                   | 1\. El Sistema detecta que el DNI |
|                                   | ingresado ya se encuentra         |
|                                   | registrado para el mismo evento.  |
|                                   |                                   |
|                                   | 2\. El Sistema muestra el mensaje |
|                                   | " El DNI ingresado ya posee una   |
|                                   | entrada para este evento" y       |
|                                   | regresa al paso 6 del flujo       |
|                                   | principal.                        |
+-----------------------------------+-----------------------------------+
| **POSTCONDICIONES:**              | La compra queda registrada, las   |
|                                   | entradas quedan asociadas a los   |
|                                   | DNIs ingresados y el stock del    |
|                                   | evento se actualiza.              |
+-----------------------------------+-----------------------------------+
| **REGLAS DE NEGOCIO:**            | RN-01: No se permitirá registrar  |
|                                   | DNIs duplicados para un mismo     |
|                                   | evento.                           |
+-----------------------------------+-----------------------------------+

[SOLICITAR DEVOLUCIÓN]{.mark}

+-----------------------------------+-----------------------------------+
| **ID / NOMBRE:**                  | CU - 15 SOLICITAR DEVOLUCIÓN      |
+===================================+===================================+
| **DESCRIPCIÓN:**                  | Permite a un Usuario Registrado   |
|                                   | solicitar la devolución de una    |
|                                   | entrada adquirida.                |
+-----------------------------------+-----------------------------------+
| **ACTOR PRINCIPAL:**              | Usuario Registrado                |
+-----------------------------------+-----------------------------------+
| **ACTORES SECUNDARIOS:**          | No aplica                         |
+-----------------------------------+-----------------------------------+
| **DISPARADOR (TRIGGER):**         | El Usuario Registrado selecciona  |
|                                   | la opción "Solicitar devolución"  |
|                                   | de una entrada adquirida.         |
+-----------------------------------+-----------------------------------+
| **PRECONDICIONES:**               | El Usuario Registrado posee una   |
|                                   | entrada activa para el evento     |
|                                   | agotado.                          |
+-----------------------------------+-----------------------------------+
| **FLUJO PRINCIPAL**               | 1\. El Usuario Registrado         |
|                                   | selecciona la entrada que desea   |
|                                   | devolver.                         |
|                                   |                                   |
|                                   | 2\. El Sistema muestra la         |
|                                   | información de la entrada.        |
|                                   |                                   |
|                                   | 3\. El Usuario Registrado         |
|                                   | confirma la solicitud de          |
|                                   | devolución.                       |
|                                   |                                   |
|                                   | 4\. El Sistema registra la        |
|                                   | devolución.                       |
|                                   |                                   |
|                                   | 6\. El Sistema aplica el          |
|                                   | reembolso correspondiente.        |
|                                   |                                   |
|                                   | 7\. El Sistema repone la entrada  |
|                                   | al stock disponible.              |
+-----------------------------------+-----------------------------------+
| **FLUJO ALTERNATIVO**             | **3A. Entrada no válida para      |
|                                   | devolución.**                     |
|                                   |                                   |
|                                   | 1\. El Sistema detecta que la     |
|                                   | entrada no cumple las condiciones |
|                                   | para ser devuelta.                |
|                                   |                                   |
|                                   | 2\. El Sistema muestra el mensaje |
|                                   | "La devolución no puede           |
|                                   | realizarse para esta entrada" y   |
|                                   | regresa al paso 2 del flujo       |
|                                   | principal.                        |
+-----------------------------------+-----------------------------------+
| **POSTCONDICIONES:**              | La devolución queda registrada,   |
|                                   | se aplica un reembolso del 80% y  |
|                                   | la entrada vuelve al stock        |
|                                   | disponible.                       |
+-----------------------------------+-----------------------------------+
| **REGLAS DE NEGOCIO:**            | RN-02: El sistema aplicará un     |
|                                   | reembolso del 80% y repondrá      |
|                                   | automáticamente la entrada al     |
|                                   | stock disponible.                 |
+-----------------------------------+-----------------------------------+

[APLICAR INCREMENTO DINÁMICO DE PRECIOS]{.mark}

+-----------------------------------+-----------------------------------+
| **ID / NOMBRE:**                  | CU - 30 APLICAR INCREMENTO        |
|                                   | DINÁMICO DE PRECIOS               |
+===================================+===================================+
| **DESCRIPCIÓN:**                  | Permite aplicar automáticamente   |
|                                   | un incremento al precio de las    |
|                                   | entradas según el nivel de        |
|                                   | ocupación del sector.             |
+-----------------------------------+-----------------------------------+
| **ACTOR PRINCIPAL:**              | Sistema.                          |
+-----------------------------------+-----------------------------------+
| **ACTORES SECUNDARIOS:**          | No aplica.                        |
+-----------------------------------+-----------------------------------+
| **DISPARADOR (TRIGGER):**         | El sistema detecta que el aforo   |
|                                   | vendido supera el 80% de la       |
|                                   | capacidad del sector.             |
+-----------------------------------+-----------------------------------+
| **PRECONDICIONES:**               | El evento se encuentra activo y   |
|                                   | posee sectores con entradas       |
|                                   | disponibles.                      |
+-----------------------------------+-----------------------------------+
| **FLUJO PRINCIPAL**               | 1\. El Sistema monitorea el nivel |
|                                   | de ocupación del sector.          |
|                                   |                                   |
|                                   | 2\. El Sistema detecta que el     |
|                                   | aforo vendido supera el 80% de la |
|                                   | capacidad.                        |
|                                   |                                   |
|                                   | 3\. El Sistema aplica un          |
|                                   | incremento del 20% sobre el       |
|                                   | precio base.                      |
|                                   |                                   |
|                                   | 4\. El Sistema actualiza el       |
|                                   | precio de las entradas del        |
|                                   | sector.                           |
+-----------------------------------+-----------------------------------+
| **FLUJO ALTERNATIVO**             | **2A. No se alcanza el nivel de   |
|                                   | ocupación**                       |
|                                   |                                   |
|                                   | 1\. El Sistema detecta que el     |
|                                   | aforo vendido no supera el 80%    |
|                                   |                                   |
|                                   | 2\. El Sistema mantiene el precio |
|                                   | base de las entradas.             |
+-----------------------------------+-----------------------------------+
| **POSTCONDICIONES:**              | El precio de las entradas queda   |
|                                   | actualizado cuando corresponda.   |
+-----------------------------------+-----------------------------------+
| **REGLAS DE NEGOCIO:**            | RN - 03: El Sistema incrementará  |
|                                   | automáticamente en un 20% el      |
|                                   | precio base de un sector cuando   |
|                                   | el aforo vendido supere el 80% de |
|                                   | su capacidad.                     |
+-----------------------------------+-----------------------------------+
