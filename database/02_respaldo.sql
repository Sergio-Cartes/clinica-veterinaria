/* Respaldo y restauración de la base ClinicaVeterinaria (RNF-06)
   Antes de ejecutar: crear la carpeta C:\Respaldos (o cambiar la ruta)*/

-- Respaldo completo
BACKUP DATABASE ClinicaVeterinaria
TO DISK = N'C:\Respaldos\ClinicaVeterinaria.bak'
WITH INIT, COMPRESSION, NAME = N'ClinicaVeterinaria - respaldo completo';
GO

-- Restauración (ejecutar solo si hay que recuperar la base)
/*
USE master;
GO
RESTORE DATABASE ClinicaVeterinaria
FROM DISK = N'C:\Respaldos\ClinicaVeterinaria.bak'
WITH REPLACE;
GO
*/
