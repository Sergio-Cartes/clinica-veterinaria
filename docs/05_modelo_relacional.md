# Modelo Relacional y Normalización

Sistema de gestión para una clínica veterinaria

El modelo sale del diagrama de clases (`04_diagrama_clases.md`). Cada clase pasó a ser una tabla y las relaciones 1 a muchos pasaron a ser claves foráneas. El código completo está en `database/01_script_clinica_veterinaria.sql`.

## 1. Tablas

Especie (<u>IdEspecie</u>, Nombre)

Propietario (<u>IdPropietario</u>, Rut, Nombre, Telefono, Correo)

Mascota (<u>IdMascota</u>, Nombre, FechaNacimiento, IdEspecie, IdPropietario)

Usuario (<u>IdUsuario</u>, NombreUsuario, ClaveHash, Salt, Rol, IntentosFallidos, Bloqueado)

LogAuditoria (<u>IdLog</u>, IdUsuario, Tabla, Accion, IdRegistro, FechaHora, ValorAnterior, ValorNuevo)

Subrayado = clave primaria. Cursiva = clave foránea.

## 2. Claves foráneas

| Tabla | Columna | Referencia |
|-------|---------|------------|
| Mascota | IdEspecie | Especie(IdEspecie) |
| Mascota | IdPropietario | Propietario(IdPropietario) |
| LogAuditoria | IdUsuario | Usuario(IdUsuario) |

## 3. Restricciones de integridad

| Tabla | Restricción | Para qué sirve |
|-------|-------------|----------------|
| Especie | UNIQUE(Nombre) | No repetir especies |
| Propietario | UNIQUE(Rut) | Un RUT no se repite (RF-06) |
| Propietario | CHECK en Correo | El correo tiene formato válido |
| Propietario | CHECK en Telefono | Solo números, `+` y espacios, mínimo 8 caracteres |
| Usuario | UNIQUE(NombreUsuario) | No hay dos usuarios iguales |
| Usuario | CHECK en Rol | Solo Administrador o Recepcionista |
| Mascota | UNIQUE(IdPropietario, Nombre) | Un propietario no repite el nombre de una mascota (RF-02) |
| Mascota | CHECK en FechaNacimiento | La fecha no puede ser futura |
| LogAuditoria | CHECK en Accion | Solo INSERT, UPDATE o DELETE |

## 4. Normalización

Primera forma normal (1FN). Todas las columnas guardan un solo valor y no hay grupos repetidos. Por ejemplo, la especie no se escribe como texto dentro de Mascota, sino que se guarda su id, y cada mascota es una fila distinta.

Segunda forma normal (2FN). Todas las tablas tienen una clave primaria de una sola columna (un id), así que ningún atributo puede depender solo de una parte de la clave. Se cumple sin cambios.

Tercera forma normal (3FN). Ningún atributo depende de otro que no sea la clave. Los datos del dueño (nombre, teléfono, correo) están en Propietario y no en Mascota, y el nombre de la especie está en Especie. Si estuvieran dentro de Mascota, cambiar el teléfono de un dueño obligaría a modificar todas sus mascotas.

Un caso aparte es `LogAuditoria`, que guarda `ValorAnterior` y `ValorNuevo` como texto. Es intencional: el log es un historial y tiene que conservar cómo era el dato en ese momento, aunque después cambie o se elimine. Por eso no se trata como una columna derivada.

## 5. Cambios respecto al diagrama de clases

Al bajar al modelo relacional se agregaron columnas que el diagrama de clases no tenía, porque las pide un requerimiento:

| Tabla | Columna nueva | Motivo |
|-------|---------------|--------|
| Usuario | Salt | Guardar la clave con hash y salt (RNF-01) |
| Usuario | IntentosFallidos, Bloqueado | Bloquear la cuenta tras 3 intentos fallidos (RF-01) |
| LogAuditoria | IdRegistro | Saber qué mascota fue modificada |

## 6. Procedimientos almacenados

| Procedimiento | Qué hace | RF |
|---------------|----------|----|
| sp_Usuario_ObtenerParaLogin | Entrega el hash y el salt para validar la clave | RF-01 |
| sp_Usuario_RegistrarIntentoFallido | Suma un intento y bloquea al llegar a 3 | RF-01 |
| sp_Usuario_RegistrarLoginExitoso | Deja los intentos en 0 | RF-01 |
| sp_Mascota_Insertar | Registra una mascota | RF-02 |
| sp_Mascota_Listar | Busca por nombre, especie, propietario o RUT | RF-03 |
| sp_Mascota_ObtenerPorId | Trae una mascota para editarla | RF-03 |
| sp_Mascota_Actualizar | Modifica una mascota | RF-04 |
| sp_Mascota_Eliminar | Elimina una mascota (solo Administrador) | RF-05 |
| sp_Propietario_Insertar / Actualizar / ObtenerPorId / Listar | Mantenedor de propietarios | RF-06 |
| sp_Especie_Listar / Insertar | Catálogo de especies | RF-02 |
| sp_Log_Consultar | Consulta el log con filtros (solo Administrador) | RF-08, RF-09 |

## 7. Triggers

| Trigger | Tabla | Qué hace | RF |
|---------|-------|----------|----|
| trg_Mascota_Auditoria | Mascota | Después de un INSERT, UPDATE o DELETE guarda en LogAuditoria el usuario, la acción, la fecha y los valores | RF-07 |
| trg_Log_Inmutable | LogAuditoria | Bloquea cualquier UPDATE o DELETE sobre el log | RF-07 |

Cómo sabe el trigger quién hizo el cambio. Un trigger no recibe parámetros. Por eso los procedimientos de Mascota reciben `@IdUsuario` y lo dejan guardado en el contexto de la sesión (`sp_set_session_context`) justo antes de modificar la tabla. El trigger lo lee con `SESSION_CONTEXT`. Para que funcione, la aplicación debe modificar siempre por estos procedimientos y con la misma conexión abierta.

## 8. Cómo se guarda la clave

La clave nunca se guarda en texto plano. Se guarda `SHA-256(Salt + clave)`, donde el Salt es un valor aleatorio de 16 bytes propio de cada usuario. La aplicación C# tiene que calcular el hash exactamente igual para poder compararlo: `SHA256(Salt + Encoding.Unicode.GetBytes(clave))`.
