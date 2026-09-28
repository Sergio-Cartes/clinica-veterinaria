using System.Text.RegularExpressions;
using ClinicaVeterinaria.DAL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.BLL;

public static class PropietarioBLL
{
    private static readonly Regex RutRegex = new(@"^\d{7,8}-[\dK]$");
    private static readonly Regex TelefonoRegex = new(@"^[0-9+ ]{8,20}$");
    private static readonly Regex CorreoRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static List<Propietario> Listar(string? texto)
    {
        Sesion.Requerir();
        return PropietarioDAL.Listar(string.IsNullOrWhiteSpace(texto) ? null : texto.Trim());
    }

    public static int Insertar(Propietario p)
    {
        Sesion.Requerir();
        Validar(p, validarRut: true);
        return PropietarioDAL.Insertar(p);
    }

    public static void Actualizar(Propietario p)
    {
        Sesion.Requerir();
        if (p.IdPropietario <= 0)
            throw new NegocioException("Seleccione un propietario para modificar.");
        Validar(p, validarRut: false);
        PropietarioDAL.Actualizar(p);
    }

    private static void Validar(Propietario p, bool validarRut)
    {
        if (validarRut)
        {
            p.Rut = (p.Rut ?? "").Trim().Replace(".", "").ToUpperInvariant();
            if (!RutRegex.IsMatch(p.Rut))
                throw new NegocioException("El RUT no es válido. Use el formato 12345678-9 (sin puntos y con guion).");
        }

        p.Nombre = (p.Nombre ?? "").Trim();
        if (p.Nombre.Length == 0)
            throw new NegocioException("El nombre del propietario es obligatorio.");
        if (p.Nombre.Length > 100)
            throw new NegocioException("El nombre no puede superar los 100 caracteres.");

        p.Telefono = (p.Telefono ?? "").Trim();
        if (!TelefonoRegex.IsMatch(p.Telefono))
            throw new NegocioException("El teléfono no es válido. Use solo números, espacios y el signo +, con al menos 8 caracteres.");

        p.Correo = string.IsNullOrWhiteSpace(p.Correo) ? null : p.Correo.Trim();
        if (p.Correo != null && (p.Correo.Length > 100 || !CorreoRegex.IsMatch(p.Correo)))
            throw new NegocioException("El correo no tiene un formato válido.");
    }
}
