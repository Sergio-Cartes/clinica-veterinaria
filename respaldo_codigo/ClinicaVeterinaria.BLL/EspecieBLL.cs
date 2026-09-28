using ClinicaVeterinaria.DAL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.BLL;

public static class EspecieBLL
{
    public static List<Especie> Listar()
    {
        Sesion.Requerir();
        return EspecieDAL.Listar();
    }
}
