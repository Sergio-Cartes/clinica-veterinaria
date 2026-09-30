# Arquitectura de la Aplicación

Sistema de gestión para una clínica veterinaria

La aplicación está organizada en 4 capas. Cada una es un proyecto distinto de la solución, y cada capa solo puede llamar a la que tiene justo debajo.

```
[ UI ]  Windows Forms
   ▼
[ BLL ]  Lógica de negocio
   ▼
[ DAL ]  Acceso a datos
   ▼
[ Base de datos ]  Procedimientos almacenados y triggers

[ Entidades ]  clases de datos que usan UI, BLL y DAL
```

## Qué hace cada capa

| Capa | Proyecto | Qué hace |
|------|----------|----------|
| Entidades | `ClinicaVeterinaria.Entidades` | Clases de datos: Mascota, Propietario, Especie, Usuario, LogRegistro. Sin lógica. |
| DAL | `ClinicaVeterinaria.DAL` | Llama a los procedimientos almacenados y convierte el resultado en objetos. Nunca arma SQL con texto. |
| BLL | `ClinicaVeterinaria.BLL` | Valida los datos, controla los permisos por rol, calcula el hash de la clave y genera el PDF. |
| UI | `ClinicaVeterinaria.UI` | Formularios Windows Forms. Solo muestra datos y los envía a la BLL; no valida ni accede a la base. |

La UI nunca llama directo al DAL, siempre pasa por la BLL. Así ninguna regla de negocio se puede saltar desde un formulario.

## Flujo de datos: registrar una mascota (RF-02)

1. **UI:** el formulario `MascotasForm` arma un objeto `Mascota` y llama a `MascotaBLL.Insertar()`.
2. **BLL:** valida los datos (nombre obligatorio, especie y propietario elegidos, fecha no futura). Si algo falla, lanza un error y no sigue.
3. **BLL → DAL:** si todo está bien, llama a `MascotaDAL.Insertar()`.
4. **DAL:** ejecuta el procedimiento `sp_Mascota_Insertar` con los datos como parámetros.
5. **Base de datos:** valida de nuevo, hace el `INSERT` y el trigger `trg_Mascota_Auditoria` guarda el cambio en `LogAuditoria` (RF-07).
6. **UI:** recibe la confirmación y actualiza la tabla en pantalla.

Si algo falla en cualquier paso, el error sube por las mismas capas hasta la UI, que muestra un mensaje simple, sin detalles técnicos (RNF-08).

## Seguridad en más de una capa

| Regla | Dónde se aplica |
|-------|------------------|
| Solo el Administrador elimina mascotas | BLL y también el procedimiento `sp_Mascota_Eliminar` |
| Clave con hash y salt | Se calcula en la BLL antes de enviarla; nunca se guarda en texto plano |
| El log no se puede modificar | Trigger `trg_Log_Inmutable` |

Se repite en dos capas para que la regla se cumpla aunque alguien acceda directo a la base de datos, sin pasar por la aplicación.
