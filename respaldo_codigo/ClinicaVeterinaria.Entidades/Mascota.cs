namespace ClinicaVeterinaria.Entidades;

public class Mascota
{
    public int IdMascota { get; set; }
    public string Nombre { get; set; } = "";
    public DateTime? FechaNacimiento { get; set; }
    public int IdEspecie { get; set; }
    public string NombreEspecie { get; set; } = "";
    public int IdPropietario { get; set; }
    public string NombrePropietario { get; set; } = "";
    public string RutPropietario { get; set; } = "";
}
