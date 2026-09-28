using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.DAL;

public static class EspecieDAL
{
    public static List<Especie> Listar()
    {
        return Ejecutor.Consultar("dbo.sp_Especie_Listar", null,
            rd => new Especie
            {
                IdEspecie = (int)rd["IdEspecie"],
                Nombre = (string)rd["Nombre"]
            });
    }
}
