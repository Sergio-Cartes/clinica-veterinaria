using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.DAL;

public static class LogDAL
{
    public static List<LogRegistro> Consultar(int idSolicitante, DateTime? desde, DateTime? hasta,
        int? idUsuarioFiltro, string? accion)
    {
        return Ejecutor.Consultar("dbo.sp_Log_Consultar", p =>
        {
            p.AddWithValue("@IdSolicitante", idSolicitante);
            p.AddWithValue("@Desde", Ejecutor.V(desde?.Date));
            p.AddWithValue("@Hasta", Ejecutor.V(hasta?.Date));
            p.AddWithValue("@IdUsuarioFiltro", Ejecutor.V(idUsuarioFiltro));
            p.AddWithValue("@Accion", Ejecutor.V(accion));
        },
        rd => new LogRegistro
        {
            IdLog = (long)rd["IdLog"],
            FechaHora = (DateTime)rd["FechaHora"],
            Usuario = rd["NombreUsuario"] as string,
            Tabla = (string)rd["Tabla"],
            Accion = (string)rd["Accion"],
            IdRegistro = (int)rd["IdRegistro"],
            ValorAnterior = rd["ValorAnterior"] as string,
            ValorNuevo = rd["ValorNuevo"] as string
        });
    }
}
