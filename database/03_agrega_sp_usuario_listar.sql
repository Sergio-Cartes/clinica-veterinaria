
USE ClinicaVeterinaria;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Listar
    @IdSolicitante INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario
                       WHERE IdUsuario = @IdSolicitante AND Rol = N'Administrador' AND Bloqueado = 0)
            THROW 50003, N'Solo un Administrador puede ver la lista de usuarios.', 1;

        SELECT IdUsuario, NombreUsuario, Rol
        FROM dbo.Usuario
        ORDER BY NombreUsuario;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO
