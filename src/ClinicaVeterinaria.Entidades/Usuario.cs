namespace ClinicaVeterinaria.Entidades;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public byte[] ClaveHash { get; set; } = Array.Empty<byte>();
    public byte[] Salt { get; set; } = Array.Empty<byte>();
    public string Rol { get; set; } = "";
    public int IntentosFallidos { get; set; }
    public bool Bloqueado { get; set; }
}
