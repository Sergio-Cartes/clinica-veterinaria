using ClinicaVeterinaria.Entidades;
using Microsoft.Data.SqlClient;

namespace ClinicaVeterinaria.DAL;

public static class MascotaDAL
{
    public static int Insertar(Mascota m, int idUsuario)
    {
        return Ejecutor.EjecutarConSalida("dbo.sp_Mascota_Insertar", p =>
        {
            p.AddWithValue("@Nombre", m.Nombre);
            p.AddWithValue("@FechaNacimiento", Ejecutor.V(m.FechaNacimiento?.Date));
            p.AddWithValue("@IdEspecie", m.IdEspecie);
            p.AddWithValue("@IdPropietario", m.IdPropietario);
            p.AddWithValue("@IdUsuario", idUsuario);
        }, "@IdMascota");
    }

    public static void Actualizar(Mascota m, int idUsuario)
    {
        Ejecutor.Ejecutar("dbo.sp_Mascota_Actualizar", p =>
        {
            p.AddWithValue("@IdMascota", m.IdMascota);
            p.AddWithValue("@Nombre", m.Nombre);
            p.AddWithValue("@FechaNacimiento", Ejecutor.V(m.FechaNacimiento?.Date));
            p.AddWithValue("@IdEspecie", m.IdEspecie);
            p.AddWithValue("@IdPropietario", m.IdPropietario);
            p.AddWithValue("@IdUsuario", idUsuario);
        });
    }

    public static void Eliminar(int idMascota, int idUsuario)
    {
        Ejecutor.Ejecutar("dbo.sp_Mascota_Eliminar", p =>
        {
            p.AddWithValue("@IdMascota", idMascota);
            p.AddWithValue("@IdUsuario", idUsuario);
        });
    }

    public static List<Mascota> Listar(string? texto)
    {
        return Ejecutor.Consultar("dbo.sp_Mascota_Listar",
            p => p.AddWithValue("@Texto", Ejecutor.V(texto)),
            Mapear);
    }

    private static Mascota Mapear(SqlDataReader rd)
    {
        return new Mascota
        {
            IdMascota = (int)rd["IdMascota"],
            Nombre = (string)rd["Nombre"],
            FechaNacimiento = rd["FechaNacimiento"] as DateTime?,
            IdEspecie = (int)rd["IdEspecie"],
            NombreEspecie = (string)rd["Especie"],
            IdPropietario = (int)rd["IdPropietario"],
            NombrePropietario = (string)rd["Propietario"],
            RutPropietario = (string)rd["RutPropietario"]
        };
    }
}
