
IF DB_ID(N'ClinicaVeterinaria') IS NULL
    CREATE DATABASE ClinicaVeterinaria;
GO

USE ClinicaVeterinaria;
GO

/* ---------------------------------------------------------------------
   1. Limpieza (para poder volver a ejecutar el script)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.LogAuditoria', N'U') IS NOT NULL DROP TABLE dbo.LogAuditoria;
IF OBJECT_ID(N'dbo.Mascota',      N'U') IS NOT NULL DROP TABLE dbo.Mascota;
IF OBJECT_ID(N'dbo.Usuario',      N'U') IS NOT NULL DROP TABLE dbo.Usuario;
IF OBJECT_ID(N'dbo.Propietario',  N'U') IS NOT NULL DROP TABLE dbo.Propietario;
IF OBJECT_ID(N'dbo.Especie',      N'U') IS NOT NULL DROP TABLE dbo.Especie;
GO

/* ---------------------------------------------------------------------
   2. Tablas
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Especie (
    IdEspecie   INT IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Especie        PRIMARY KEY (IdEspecie),
    CONSTRAINT UQ_Especie_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE dbo.Propietario (
    IdPropietario INT IDENTITY(1,1) NOT NULL,
    Rut           NVARCHAR(12)      NOT NULL,
    Nombre        NVARCHAR(100)     NOT NULL,
    Telefono      NVARCHAR(20)      NOT NULL,
    Correo        NVARCHAR(100)     NULL,
    CONSTRAINT PK_Propietario        PRIMARY KEY (IdPropietario),
    CONSTRAINT UQ_Propietario_Rut    UNIQUE (Rut),
    CONSTRAINT CK_Propietario_Correo CHECK (Correo IS NULL OR Correo LIKE N'_%@_%._%'),
    CONSTRAINT CK_Propietario_Fono   CHECK (LEN(Telefono) >= 8 AND Telefono NOT LIKE N'%[^0-9+ ]%')
);
GO

CREATE TABLE dbo.Usuario (
    IdUsuario        INT IDENTITY(1,1) NOT NULL,
    NombreUsuario    NVARCHAR(50)      NOT NULL,
    ClaveHash        VARBINARY(32)     NOT NULL,   
    Salt             VARBINARY(16)     NOT NULL,
    Rol              NVARCHAR(20)      NOT NULL,
    IntentosFallidos INT               NOT NULL CONSTRAINT DF_Usuario_Intentos  DEFAULT 0,
    Bloqueado        BIT               NOT NULL CONSTRAINT DF_Usuario_Bloqueado DEFAULT 0,
    CONSTRAINT PK_Usuario        PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuario_Nombre UNIQUE (NombreUsuario),
    CONSTRAINT CK_Usuario_Rol    CHECK (Rol IN (N'Administrador', N'Recepcionista'))
);
GO

CREATE TABLE dbo.Mascota (
    IdMascota       INT IDENTITY(1,1) NOT NULL,
    Nombre          NVARCHAR(50)      NOT NULL,
    FechaNacimiento DATE              NULL,
    IdEspecie       INT               NOT NULL,
    IdPropietario   INT               NOT NULL,
    CONSTRAINT PK_Mascota             PRIMARY KEY (IdMascota),
    CONSTRAINT FK_Mascota_Especie     FOREIGN KEY (IdEspecie)     REFERENCES dbo.Especie (IdEspecie),
    CONSTRAINT FK_Mascota_Propietario FOREIGN KEY (IdPropietario) REFERENCES dbo.Propietario (IdPropietario),
    CONSTRAINT UQ_Mascota_Prop_Nombre UNIQUE (IdPropietario, Nombre),
    CONSTRAINT CK_Mascota_Nacimiento  CHECK (FechaNacimiento IS NULL OR FechaNacimiento <= CAST(GETDATE() AS DATE))
);
GO

CREATE TABLE dbo.LogAuditoria (
    IdLog         BIGINT IDENTITY(1,1) NOT NULL,
    IdUsuario     INT                  NULL,
    Tabla         NVARCHAR(50)         NOT NULL,
    Accion        NVARCHAR(10)         NOT NULL,
    IdRegistro    INT                  NOT NULL,
    FechaHora     DATETIME2(0)         NOT NULL CONSTRAINT DF_Log_Fecha DEFAULT SYSDATETIME(),
    ValorAnterior NVARCHAR(500)        NULL,
    ValorNuevo    NVARCHAR(500)        NULL,
    CONSTRAINT PK_LogAuditoria PRIMARY KEY (IdLog),
    CONSTRAINT FK_Log_Usuario  FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario),
    CONSTRAINT CK_Log_Accion   CHECK (Accion IN (N'INSERT', N'UPDATE', N'DELETE'))
);
GO

CREATE INDEX IX_Log_FechaHora ON dbo.LogAuditoria (FechaHora);
CREATE INDEX IX_Mascota_Nombre ON dbo.Mascota (Nombre);
GO


CREATE OR ALTER TRIGGER dbo.trg_Mascota_Auditoria
ON dbo.Mascota
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdUsuario INT = TRY_CAST(SESSION_CONTEXT(N'IdUsuario') AS INT);

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        -- UPDATE
        INSERT INTO dbo.LogAuditoria (IdUsuario, Tabla, Accion, IdRegistro, ValorAnterior, ValorNuevo)
        SELECT @IdUsuario, N'Mascota', N'UPDATE', i.IdMascota,
               CONCAT(N'Nombre=', d.Nombre,
                      N'; Nacimiento=', CONVERT(NVARCHAR(10), d.FechaNacimiento, 23),
                      N'; Especie=', (SELECT e.Nombre FROM dbo.Especie e WHERE e.IdEspecie = d.IdEspecie),
                      N'; Propietario=', (SELECT p.Nombre FROM dbo.Propietario p WHERE p.IdPropietario = d.IdPropietario)),
               CONCAT(N'Nombre=', i.Nombre,
                      N'; Nacimiento=', CONVERT(NVARCHAR(10), i.FechaNacimiento, 23),
                      N'; Especie=', (SELECT e.Nombre FROM dbo.Especie e WHERE e.IdEspecie = i.IdEspecie),
                      N'; Propietario=', (SELECT p.Nombre FROM dbo.Propietario p WHERE p.IdPropietario = i.IdPropietario))
        FROM inserted i
        INNER JOIN deleted d ON d.IdMascota = i.IdMascota;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        -- INSERT
        INSERT INTO dbo.LogAuditoria (IdUsuario, Tabla, Accion, IdRegistro, ValorAnterior, ValorNuevo)
        SELECT @IdUsuario, N'Mascota', N'INSERT', i.IdMascota,
               NULL,
               CONCAT(N'Nombre=', i.Nombre,
                      N'; Nacimiento=', CONVERT(NVARCHAR(10), i.FechaNacimiento, 23),
                      N'; Especie=', (SELECT e.Nombre FROM dbo.Especie e WHERE e.IdEspecie = i.IdEspecie),
                      N'; Propietario=', (SELECT p.Nombre FROM dbo.Propietario p WHERE p.IdPropietario = i.IdPropietario))
        FROM inserted i;
    END
    ELSE IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        -- DELETE
        INSERT INTO dbo.LogAuditoria (IdUsuario, Tabla, Accion, IdRegistro, ValorAnterior, ValorNuevo)
        SELECT @IdUsuario, N'Mascota', N'DELETE', d.IdMascota,
               CONCAT(N'Nombre=', d.Nombre,
                      N'; Nacimiento=', CONVERT(NVARCHAR(10), d.FechaNacimiento, 23),
                      N'; Especie=', (SELECT e.Nombre FROM dbo.Especie e WHERE e.IdEspecie = d.IdEspecie),
                      N'; Propietario=', (SELECT p.Nombre FROM dbo.Propietario p WHERE p.IdPropietario = d.IdPropietario)),
               NULL
        FROM deleted d;
    END
END;
GO


CREATE OR ALTER TRIGGER dbo.trg_Log_Inmutable
ON dbo.LogAuditoria
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50010, N'El registro de auditoría no se puede modificar ni eliminar.', 1;
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_Especie_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdEspecie, Nombre
    FROM dbo.Especie
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Especie_Insertar
    @Nombre    NVARCHAR(50),
    @IdEspecie INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF LTRIM(RTRIM(ISNULL(@Nombre, N''))) = N''
            THROW 50001, N'El nombre de la especie es obligatorio.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Especie WHERE Nombre = LTRIM(RTRIM(@Nombre)))
            THROW 50002, N'Esa especie ya existe.', 1;

        INSERT INTO dbo.Especie (Nombre) VALUES (LTRIM(RTRIM(@Nombre)));
        SET @IdEspecie = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_Propietario_Insertar
    @Rut           NVARCHAR(12),
    @Nombre        NVARCHAR(100),
    @Telefono      NVARCHAR(20),
    @Correo        NVARCHAR(100) = NULL,
    @IdPropietario INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF LTRIM(RTRIM(ISNULL(@Rut, N''))) = N''
            THROW 50001, N'El RUT es obligatorio.', 1;
        IF LTRIM(RTRIM(ISNULL(@Nombre, N''))) = N''
            THROW 50001, N'El nombre del propietario es obligatorio.', 1;
        IF LTRIM(RTRIM(ISNULL(@Telefono, N''))) = N''
            THROW 50001, N'El teléfono es obligatorio.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Propietario WHERE Rut = LTRIM(RTRIM(@Rut)))
            THROW 50005, N'Ya existe un propietario con ese RUT.', 1;

        INSERT INTO dbo.Propietario (Rut, Nombre, Telefono, Correo)
        VALUES (LTRIM(RTRIM(@Rut)), LTRIM(RTRIM(@Nombre)), LTRIM(RTRIM(@Telefono)),
                NULLIF(LTRIM(RTRIM(@Correo)), N''));

        SET @IdPropietario = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Propietario_Actualizar
    @IdPropietario INT,
    @Nombre        NVARCHAR(100),
    @Telefono      NVARCHAR(20),
    @Correo        NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Propietario WHERE IdPropietario = @IdPropietario)
            THROW 50004, N'El propietario no existe.', 1;
        IF LTRIM(RTRIM(ISNULL(@Nombre, N''))) = N''
            THROW 50001, N'El nombre del propietario es obligatorio.', 1;
        IF LTRIM(RTRIM(ISNULL(@Telefono, N''))) = N''
            THROW 50001, N'El teléfono es obligatorio.', 1;

        UPDATE dbo.Propietario
        SET Nombre   = LTRIM(RTRIM(@Nombre)),
            Telefono = LTRIM(RTRIM(@Telefono)),
            Correo   = NULLIF(LTRIM(RTRIM(@Correo)), N'')
        WHERE IdPropietario = @IdPropietario;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Propietario_ObtenerPorId
    @IdPropietario INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdPropietario, Rut, Nombre, Telefono, Correo
    FROM dbo.Propietario
    WHERE IdPropietario = @IdPropietario;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Propietario_Listar
    @Texto NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdPropietario, Rut, Nombre, Telefono, Correo
    FROM dbo.Propietario
    WHERE @Texto IS NULL
       OR Rut    LIKE N'%' + @Texto + N'%'
       OR Nombre LIKE N'%' + @Texto + N'%'
    ORDER BY Nombre;
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_Mascota_Insertar
    @Nombre          NVARCHAR(50),
    @FechaNacimiento DATE = NULL,
    @IdEspecie       INT,
    @IdPropietario   INT,
    @IdUsuario       INT,
    @IdMascota       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE IdUsuario = @IdUsuario AND Bloqueado = 0)
            THROW 50003, N'Usuario no válido o bloqueado.', 1;
        IF LTRIM(RTRIM(ISNULL(@Nombre, N''))) = N''
            THROW 50001, N'El nombre de la mascota es obligatorio.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Especie WHERE IdEspecie = @IdEspecie)
            THROW 50004, N'La especie indicada no existe.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Propietario WHERE IdPropietario = @IdPropietario)
            THROW 50004, N'El propietario indicado no existe.', 1;
        IF EXISTS (SELECT 1 FROM dbo.Mascota
                   WHERE IdPropietario = @IdPropietario AND Nombre = LTRIM(RTRIM(@Nombre)))
            THROW 50002, N'Ese propietario ya tiene una mascota con ese nombre.', 1;

        
        EXEC sys.sp_set_session_context @key = N'IdUsuario', @value = @IdUsuario;

        INSERT INTO dbo.Mascota (Nombre, FechaNacimiento, IdEspecie, IdPropietario)
        VALUES (LTRIM(RTRIM(@Nombre)), @FechaNacimiento, @IdEspecie, @IdPropietario);

        SET @IdMascota = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Mascota_Actualizar
    @IdMascota       INT,
    @Nombre          NVARCHAR(50),
    @FechaNacimiento DATE = NULL,
    @IdEspecie       INT,
    @IdPropietario   INT,
    @IdUsuario       INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE IdUsuario = @IdUsuario AND Bloqueado = 0)
            THROW 50003, N'Usuario no válido o bloqueado.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Mascota WHERE IdMascota = @IdMascota)
            THROW 50004, N'La mascota no existe.', 1;
        IF LTRIM(RTRIM(ISNULL(@Nombre, N''))) = N''
            THROW 50001, N'El nombre de la mascota es obligatorio.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Especie WHERE IdEspecie = @IdEspecie)
            THROW 50004, N'La especie indicada no existe.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Propietario WHERE IdPropietario = @IdPropietario)
            THROW 50004, N'El propietario indicado no existe.', 1;
        IF EXISTS (SELECT 1 FROM dbo.Mascota
                   WHERE IdPropietario = @IdPropietario
                     AND Nombre = LTRIM(RTRIM(@Nombre))
                     AND IdMascota <> @IdMascota)
            THROW 50002, N'Ese propietario ya tiene una mascota con ese nombre.', 1;

        EXEC sys.sp_set_session_context @key = N'IdUsuario', @value = @IdUsuario;

        UPDATE dbo.Mascota
        SET Nombre          = LTRIM(RTRIM(@Nombre)),
            FechaNacimiento = @FechaNacimiento,
            IdEspecie       = @IdEspecie,
            IdPropietario   = @IdPropietario
        WHERE IdMascota = @IdMascota;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Mascota_Eliminar
    @IdMascota INT,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario
                       WHERE IdUsuario = @IdUsuario AND Rol = N'Administrador' AND Bloqueado = 0)
            THROW 50003, N'Solo un Administrador puede eliminar mascotas.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Mascota WHERE IdMascota = @IdMascota)
            THROW 50004, N'La mascota no existe.', 1;

        EXEC sys.sp_set_session_context @key = N'IdUsuario', @value = @IdUsuario;

        DELETE FROM dbo.Mascota WHERE IdMascota = @IdMascota;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Mascota_ObtenerPorId
    @IdMascota INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.IdMascota, m.Nombre, m.FechaNacimiento,
           m.IdEspecie, e.Nombre AS Especie,
           m.IdPropietario, p.Nombre AS Propietario, p.Rut AS RutPropietario
    FROM dbo.Mascota m
    INNER JOIN dbo.Especie e     ON e.IdEspecie = m.IdEspecie
    INNER JOIN dbo.Propietario p ON p.IdPropietario = m.IdPropietario
    WHERE m.IdMascota = @IdMascota;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Mascota_Listar
    @Texto NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.IdMascota, m.Nombre, m.FechaNacimiento,
           m.IdEspecie, e.Nombre AS Especie,
           m.IdPropietario, p.Nombre AS Propietario, p.Rut AS RutPropietario
    FROM dbo.Mascota m
    INNER JOIN dbo.Especie e     ON e.IdEspecie = m.IdEspecie
    INNER JOIN dbo.Propietario p ON p.IdPropietario = m.IdPropietario
    WHERE @Texto IS NULL
       OR m.Nombre LIKE N'%' + @Texto + N'%'
       OR e.Nombre LIKE N'%' + @Texto + N'%'
       OR p.Nombre LIKE N'%' + @Texto + N'%'
       OR p.Rut    LIKE N'%' + @Texto + N'%'
    ORDER BY m.Nombre;
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerParaLogin
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT IdUsuario, NombreUsuario, ClaveHash, Salt, Rol, IntentosFallidos, Bloqueado
    FROM dbo.Usuario
    WHERE NombreUsuario = @NombreUsuario;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RegistrarIntentoFallido
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Usuario
    SET IntentosFallidos = IntentosFallidos + 1,
        Bloqueado = CASE WHEN IntentosFallidos + 1 >= 3 THEN 1 ELSE Bloqueado END
    WHERE NombreUsuario = @NombreUsuario;

    SELECT IntentosFallidos, Bloqueado
    FROM dbo.Usuario
    WHERE NombreUsuario = @NombreUsuario;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_RegistrarLoginExitoso
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuario
    SET IntentosFallidos = 0
    WHERE IdUsuario = @IdUsuario;
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_Log_Consultar
    @IdSolicitante   INT,
    @Desde           DATE = NULL,
    @Hasta           DATE = NULL,
    @IdUsuarioFiltro INT = NULL,
    @Accion          NVARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario
                       WHERE IdUsuario = @IdSolicitante AND Rol = N'Administrador' AND Bloqueado = 0)
            THROW 50003, N'Solo un Administrador puede consultar el log.', 1;

        SELECT l.IdLog, l.FechaHora, u.NombreUsuario, l.Tabla, l.Accion,
               l.IdRegistro, l.ValorAnterior, l.ValorNuevo
        FROM dbo.LogAuditoria l
        LEFT JOIN dbo.Usuario u ON u.IdUsuario = l.IdUsuario
        WHERE (@Desde IS NULL OR l.FechaHora >= @Desde)
          AND (@Hasta IS NULL OR l.FechaHora <  DATEADD(DAY, 1, @Hasta))
          AND (@IdUsuarioFiltro IS NULL OR l.IdUsuario = @IdUsuarioFiltro)
          AND (@Accion IS NULL OR l.Accion = @Accion)
        ORDER BY l.FechaHora DESC, l.IdLog DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


DECLARE @salt1 VARBINARY(16) = CRYPT_GEN_RANDOM(16);
DECLARE @salt2 VARBINARY(16) = CRYPT_GEN_RANDOM(16);

INSERT INTO dbo.Usuario (NombreUsuario, ClaveHash, Salt, Rol)
VALUES (N'admin',
        HASHBYTES('SHA2_256', @salt1 + CAST(N'Admin123!' AS VARBINARY(200))),
        @salt1, N'Administrador'),
       (N'recepcion',
        HASHBYTES('SHA2_256', @salt2 + CAST(N'Recep123!' AS VARBINARY(200))),
        @salt2, N'Recepcionista');
GO

INSERT INTO dbo.Especie (Nombre)
VALUES (N'Perro'), (N'Gato'), (N'Ave'), (N'Conejo'), (N'Hámster'), (N'Reptil');
GO

INSERT INTO dbo.Propietario (Rut, Nombre, Telefono, Correo)
VALUES (N'12345678-5', N'María González', N'+56912345678', N'maria.gonzalez@correo.cl'),
       (N'98765432-1', N'Pedro Soto',     N'+56987654321', NULL);
GO


DECLARE @id INT;
EXEC dbo.sp_Mascota_Insertar @Nombre = N'Firulais', @FechaNacimiento = '2020-03-15',
     @IdEspecie = 1, @IdPropietario = 1, @IdUsuario = 1, @IdMascota = @id OUTPUT;
EXEC dbo.sp_Mascota_Insertar @Nombre = N'Michi', @FechaNacimiento = '2021-07-02',
     @IdEspecie = 2, @IdPropietario = 1, @IdUsuario = 1, @IdMascota = @id OUTPUT;
EXEC dbo.sp_Mascota_Insertar @Nombre = N'Rocky', @FechaNacimiento = '2019-11-20',
     @IdEspecie = 1, @IdPropietario = 2, @IdUsuario = 2, @IdMascota = @id OUTPUT;
GO
