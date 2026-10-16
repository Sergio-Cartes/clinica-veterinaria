/* =====================================================================
   Carga los usuarios de prueba SIN usar CRYPT_GEN_RANDOM.

   Por qué existe este script: CRYPT_GEN_RANDOM puede fallar en
   computadores donde el usuario de SQL Server tiene permisos
   restringidos (por ejemplo, en los PC de un instituto). Si esa
   función falla, la tabla Usuario queda vacía y el login nunca
   funciona, aunque la clave esté bien escrita.

   Este script no genera el salt al azar: usa un salt fijo, ya
   calculado, y su hash correspondiente (SHA-256 de Salt + clave en
   UTF-16), calculado con el mismo método que usa la aplicación.
   Así no depende de ninguna función especial de SQL Server.

   Usar este script en vez de la sección 9 de 01_script...sql
   cuando CRYPT_GEN_RANDOM dé error de permisos.
   ===================================================================== */

USE ClinicaVeterinaria;
GO

-- Si ya existen usuarios con estos nombres, los reemplaza
DELETE FROM dbo.Usuario WHERE NombreUsuario IN (N'admin', N'recepcion');
GO

INSERT INTO dbo.Usuario (NombreUsuario, ClaveHash, Salt, Rol)
VALUES
    (N'admin',
     0x08C8BA5032246738F852AE37AC2ACD4E206B511F85E4CFDD6862B36026267EB4,
     0xA9A077D0F308CB53130DDB4A55649223,
     N'Administrador'),
    (N'recepcion',
     0x4E8B286C1BAF145129D5596AC98EC9A705C5617BF49F4F88F0D9F8D4EE6E176C,
     0x82FE851045B0B6FD87E1D84FAB193F78,
     N'Recepcionista');
GO

-- Comprobación: deben aparecer las 2 filas
SELECT IdUsuario, NombreUsuario, Rol, Bloqueado, IntentosFallidos FROM dbo.Usuario;
GO

/* Usuarios de prueba que quedan disponibles:
       admin      / Admin123!   (Administrador)
       recepcion  / Recep123!   (Recepcionista)
*/
