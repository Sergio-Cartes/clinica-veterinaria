using System.Data;
using ClinicaVeterinaria.Entidades;
using Microsoft.Data.SqlClient;

namespace ClinicaVeterinaria.DAL;

/// <summary>
/// Ejecuta procedimientos almacenados y traduce los errores de SQL Server
/// a mensajes claros para el usuario (sin mostrar detalles técnicos).
/// </summary>
internal static class Ejecutor
{
    // Convierte null en DBNull para poder enviarlo como parámetro
    public static object V(object? valor) => valor ?? DBNull.Value;

    public static List<T> Consultar<T>(string procedimiento,
        Action<SqlParameterCollection>? parametros,
        Func<SqlDataReader, T> mapear)
    {
        try
        {
            using var cn = ConexionBD.Crear();
            using var cmd = new SqlCommand(procedimiento, cn) { CommandType = CommandType.StoredProcedure };
            parametros?.Invoke(cmd.Parameters);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            var lista = new List<T>();
            while (rd.Read())
                lista.Add(mapear(rd));
            return lista;
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    public static void Ejecutar(string procedimiento, Action<SqlParameterCollection> parametros)
    {
        try
        {
            using var cn = ConexionBD.Crear();
            using var cmd = new SqlCommand(procedimiento, cn) { CommandType = CommandType.StoredProcedure };
            parametros(cmd.Parameters);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    public static int EjecutarConSalida(string procedimiento,
        Action<SqlParameterCollection> parametros, string nombreSalida)
    {
        try
        {
            using var cn = ConexionBD.Crear();
            using var cmd = new SqlCommand(procedimiento, cn) { CommandType = CommandType.StoredProcedure };
            parametros(cmd.Parameters);
            var salida = new SqlParameter(nombreSalida, SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(salida);
            cn.Open();
            cmd.ExecuteNonQuery();
            return (int)salida.Value;
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    private static NegocioException Traducir(SqlException ex)
    {
        switch (ex.Number)
        {
            // Errores propios que lanzan los procedimientos almacenados (THROW 500xx)
            case 50001:
            case 50002:
            case 50003:
            case 50004:
            case 50005:
            case 50010:
                return new NegocioException(ex.Message);

            // Duplicados (UNIQUE)
            case 2627:
            case 2601:
                return new NegocioException("Ya existe un registro con esos datos.");

            // Restricciones (CHECK / FOREIGN KEY)
            case 547:
                return new NegocioException("Algún dato no es válido o el registro está relacionado con otros datos.");

            // No se pudo conectar
            case -1:
            case 2:
            case 53:
            case 4060:
            case 18456:
                return new NegocioException("No se pudo conectar con la base de datos. Revise que SQL Server esté encendido.");

            default:
                return new NegocioException("Ocurrió un error en la base de datos. Intente nuevamente.");
        }
    }
}
