namespace ClinicaVeterinaria.Entidades;

public class LogRegistro
{
    public long IdLog { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Usuario { get; set; }
    public string Tabla { get; set; } = "";
    public string Accion { get; set; } = "";
    public int IdRegistro { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
}
