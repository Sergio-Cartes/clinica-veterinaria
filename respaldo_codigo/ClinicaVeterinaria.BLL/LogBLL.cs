using ClinicaVeterinaria.DAL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.BLL;

public static class LogBLL
{
    private static readonly string[] AccionesValidas = { "INSERT", "UPDATE", "DELETE" };

    public static List<LogRegistro> Consultar(DateTime? desde, DateTime? hasta, int? idUsuario, string? accion)
    {
        var admin = Sesion.RequerirAdministrador();

        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            throw new NegocioException("La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.");

        if (!string.IsNullOrEmpty(accion) && !AccionesValidas.Contains(accion))
            throw new NegocioException("La acción indicada no es válida.");

        return LogDAL.Consultar(admin.IdUsuario, desde, hasta, idUsuario, accion);
    }
}
