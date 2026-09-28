using Microsoft.Data.SqlClient;

namespace ClinicaVeterinaria.DAL;

public static class ConexionBD
{
    // Si cambias de computador, solo hay que cambiar el nombre del servidor.
    private const string Cadena =
        @"Server=DESKTOP-1BLTFF8\SQLEXPRESS;Database=ClinicaVeterinaria;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection Crear() => new SqlConnection(Cadena);
}
