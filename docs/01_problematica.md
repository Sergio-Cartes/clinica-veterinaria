Descripción del Proyecto

Contexto:
En una clínica veterinaria poblacional se registran los datos de los pacientes (mascotas) y propietarios en cuadernos y plantillas sueltas. Esto provoca duplicación de registros, pérdida de información, imposibilidad de saber quién modificó una ficha y exposición de datos personales de los clientes.

Problema:
No existe un sistema (base de datos) que permita administrar las fichas de las mascotas con control de acceso, validación de datos y trazabilidad de los cambios realizados.

Solución Propuesta:
Una aplicación de escritorio desarrollada en Windows Forms integrada con una base de datos SQL Server, la cual permitirá gestionar (crear, consultar, modificar y eliminar) las mascotas y sus propietarios, restringir el acceso según el rol del usuario, registrar automáticamente cada modificación en un historial de auditoría (log) y generar reportes en formato PDF de dicho historial.

Alcance:
Incluye: Mantenedor de mascotas y propietarios, módulo de autenticación (login) con roles, y registro de auditoría con exportación a PDF.
Excluye: Agenda de citas, facturación e inventario de medicamentos.

Usuarios:
Administrador: Acceso total al sistema y visualización/exportación del registro de auditoría.
Recepcionista: Gestión de fichas de mascotas y propietarios, sin acceso al registro de auditoría.