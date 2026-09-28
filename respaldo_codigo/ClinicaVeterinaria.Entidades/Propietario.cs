namespace ClinicaVeterinaria.Entidades;

public class Propietario
{
    public int IdPropietario { get; set; }
    public string Rut { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string? Correo { get; set; }

    // Texto que se muestra en las listas desplegables
    public string Descripcion => $"{Nombre} ({Rut})";
}
