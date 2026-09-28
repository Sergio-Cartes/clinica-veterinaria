namespace ClinicaVeterinaria.Entidades;

/// <summary>
/// Error con un mensaje pensado para mostrarse al usuario (sin detalles técnicos).
/// </summary>
public class NegocioException : Exception
{
    public NegocioException(string mensaje) : base(mensaje) { }
}
