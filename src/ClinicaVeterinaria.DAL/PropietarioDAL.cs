using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.DAL;

public static class PropietarioDAL
{
    public static int Insertar(Propietario x)
    {
        return Ejecutor.EjecutarConSalida("dbo.sp_Propietario_Insertar", p =>
        {
            p.AddWithValue("@Rut", x.Rut);
            p.AddWithValue("@Nombre", x.Nombre);
            p.AddWithValue("@Telefono", x.Telefono);
            p.AddWithValue("@Correo", Ejecutor.V(x.Correo));
        }, "@IdPropietario");
    }

    public static void Actualizar(Propietario x)
    {
        Ejecutor.Ejecutar("dbo.sp_Propietario_Actualizar", p =>
        {
            p.AddWithValue("@IdPropietario", x.IdPropietario);
            p.AddWithValue("@Nombre", x.Nombre);
            p.AddWithValue("@Telefono", x.Telefono);
            p.AddWithValue("@Correo", Ejecutor.V(x.Correo));
        });
    }

    public static List<Propietario> Listar(string? texto)
    {
        return Ejecutor.Consultar("dbo.sp_Propietario_Listar",
            p => p.AddWithValue("@Texto", Ejecutor.V(texto)),
            rd => new Propietario
            {
                IdPropietario = (int)rd["IdPropietario"],
                Rut = (string)rd["Rut"],
                Nombre = (string)rd["Nombre"],
                Telefono = (string)rd["Telefono"],
                Correo = rd["Correo"] as string
            });
    }
}
