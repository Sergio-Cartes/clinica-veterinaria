# Especificación de Requerimientos

Sistema de gestión para una clínica veterinaria

- Asignatura: PRO203 / PRO205
- Alumno: Sergio Eduardo Cartes Troncoso
- Hito 1: Análisis y Diseño de Software
- Estándares usados: IEEE 830 e ISO/IEC 27001:2022

## 1. Introducción

### 1.1 Propósito

Este documento describe qué debe hacer el sistema de la clínica veterinaria y qué medidas de seguridad debe cumplir. Es la base para los diagramas, la base de datos y la aplicación que se desarrollan en el Hito 2.

### 1.2 Alcance

El sistema permite administrar las mascotas de la clínica y sus propietarios. Cada persona del personal entra con su usuario y clave, y según su rol puede hacer más o menos cosas. Además, cada cambio que se hace sobre una mascota queda guardado en un registro (log) y ese registro se puede sacar en un reporte PDF.

No incluye agenda de citas, facturación ni control de medicamentos.

### 1.3 Definiciones

| Término | Significado |
|---------|-------------|
| RF | Requerimiento funcional |
| RNF | Requerimiento no funcional |
| SP | Procedimiento almacenado (Stored Procedure) |
| Trigger | Código que se ejecuta solo en la base de datos cuando se inserta, modifica o elimina un registro |
| CRUD | Crear, consultar, modificar y eliminar |
| Log | Registro de los cambios hechos en el sistema |
| PII | Datos personales que permiten identificar a una persona |

## 2. Descripción general

### 2.1 Perspectiva del producto

Es una aplicación de escritorio hecha en C# con Windows Forms, conectada a una base de datos en SQL Server. Todo el acceso a los datos se hace con procedimientos almacenados y el código está separado en cuatro capas: interfaz, lógica de negocio, acceso a datos y entidades.

### 2.2 Usuarios

**Administrador.** Es quien está a cargo de la clínica. Puede hacer todo lo que hace la recepcionista y además eliminar mascotas, revisar el log y generar el reporte PDF.

**Recepcionista.** Atiende a los clientes. Puede registrar, buscar y modificar mascotas y propietarios, pero no puede eliminar mascotas ni ver el log.

### 2.3 Restricciones

- Desarrollo en C# con .NET Windows Forms.
- Base de datos en SQL Server, con acceso solo por procedimientos almacenados.
- Arquitectura en 4 capas.
- Documentación y código versionados en GitHub.

### 2.4 Supuestos

- El sistema se usa dentro de la red de la clínica.
- Cada funcionario tiene su propio usuario y no lo comparte.

## 3. Requerimientos funcionales

Cada requerimiento indica quién lo usa (rol), qué necesita hacer y cómo se comprueba que está bien hecho, incluyendo la parte de seguridad. En la última columna va el control de la norma ISO 27001 que se aplica.

| ID | Rol | Necesidad / acción | Criterio de aceptación / seguridad | ISO 27001 |
|----|-----|--------------------|------------------------------------|-----------|
| RF-01 | Usuario del sistema | Iniciar sesión con su usuario y clave | Solo entra si las credenciales son correctas. La clave se compara con su hash. Después de 3 intentos fallidos la cuenta se bloquea. | A.5.16, A.8.5 |

| RF-02 | Recepcionista, Administrador | Registrar una mascota con su propietario y su especie | Nombre, especie y propietario son obligatorios. No se puede repetir una mascota con el mismo nombre para el mismo propietario. El sistema avisa cuando se guardó. | A.8.28 |

| RF-03 | Recepcionista, Administrador | Buscar y ver mascotas por nombre, especie o propietario | Solo lo puede hacer un usuario que ya inició sesión. La búsqueda usa parámetros, no arma el SQL con texto. | A.8.3, A.5.34 |

| RF-04 | Recepcionista, Administrador | Modificar los datos de una mascota | Los datos se validan antes de guardar. El cambio queda en el log con el valor anterior y el nuevo. | A.8.15, A.8.32 |

| RF-05 | Administrador | Eliminar una mascota | Solo el Administrador puede hacerlo. El sistema pide confirmación. La eliminación queda en el log. | A.5.18, A.8.2, A.8.15 |

| RF-06 | Recepcionista, Administrador | Registrar, ver y modificar propietarios | El RUT no se puede repetir. El correo y el teléfono deben tener un formato válido. | A.5.34, A.8.28 |

| RF-07 | Sistema (trigger) | Guardar en el log cada INSERT, UPDATE y DELETE sobre Mascota | Se guarda quién lo hizo, qué acción fue, cuándo y qué datos cambiaron. Desde la aplicación no se puede editar ni borrar el log. | A.8.15 |

| RF-08 | Administrador | Consultar el log filtrando por fecha, usuario y acción | La Recepcionista no tiene acceso a esta pantalla. | A.8.3, A.8.16 |

| RF-09 | Administrador | Generar un reporte PDF con el log filtrado | El PDF muestra la fecha en que se emitió y quién lo generó. Solo lo puede generar el Administrador. | A.8.15, A.5.34 |

## 4. Requerimientos no funcionales

| ID | Tipo | Descripción | ISO 27001 |
|----|------|-------------|-----------|
| RNF-01 | Seguridad | Las claves se guardan con hash y salt, nunca en texto plano. | A.8.24, A.5.17 |

| RNF-02 | Seguridad | Todo acceso a la base de datos se hace con procedimientos almacenados y parámetros, para evitar inyección SQL. | A.8.28 |

| RNF-03 | Seguridad | Los permisos por rol se revisan en la capa de negocio, no solo ocultando botones en la interfaz. | A.5.15, A.5.18 |

| RNF-04 | Integridad | La base de datos tiene claves primarias y foráneas, restricciones (UNIQUE, NOT NULL, CHECK) y está normalizada hasta 3FN. | A.8.28 |

| RNF-05 | Trazabilidad | Todo cambio en Mascota queda en la tabla LogAuditoria con el usuario y la fecha y hora. | A.8.15 |

| RNF-06 | Disponibilidad | Se incluye un script de respaldo de la base de datos, con instrucciones. | A.8.13 |

| RNF-07 | Mantenibilidad | El código se separa en UI, BLL, DAL y Entidades. Los formularios no tienen lógica de negocio. | A.8.28 |

| RNF-08 | Usabilidad | Los mensajes de error son claros y no muestran detalles técnicos de la base de datos. | A.8.28 |

| RNF-09 | Rendimiento | Las consultas y el CRUD responden en menos de 2 segundos con hasta 10.000 registros. | (ninguno) |

## 5. Controles ISO 27001 usados

Los códigos corresponden al Anexo A de la versión 2022 de la norma.

| Control | Nombre |
|---------|--------|
| A.5.15 | Control de acceso |
| A.5.16 | Gestión de identidad |
| A.5.17 | Información de autenticación |
| A.5.18 | Derechos de acceso |
| A.5.34 | Privacidad y protección de datos personales |
| A.8.2 | Derechos de acceso privilegiados |
| A.8.3 | Restricción de acceso a la información |
| A.8.5 | Autenticación segura |
| A.8.13 | Respaldo de la información |
| A.8.15 | Registro de eventos (logging) |
| A.8.16 | Actividades de seguimiento |
| A.8.24 | Uso de criptografía |
| A.8.28 | Codificación segura |
| A.8.32 | Gestión de cambios |
