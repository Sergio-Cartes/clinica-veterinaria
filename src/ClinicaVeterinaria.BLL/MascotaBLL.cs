using System.Text.RegularExpressions;
using ClinicaVeterinaria.DAL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.BLL;

public static class MascotaBLL
{
    private static readonly Regex NombreRegex = new(@"^[\p{L}0-9 '\-\.]+$");

    public static List<Mascota> Listar(string? texto)
    {
        Sesion.Requerir();
        return MascotaDAL.Listar(string.IsNullOrWhiteSpace(texto) ? null : texto.Trim());
    }

    public static int Insertar(Mascota m)
    {
        var usuario = Sesion.Requerir();
        Validar(m);
        return MascotaDAL.Insertar(m, usuario.IdUsuario);
    }

    public static void Actualizar(Mascota m)
    {
        var usuario = Sesion.Requerir();
        if (m.IdMascota <= 0)
            throw new NegocioException("Seleccione una mascota para modificar.");
        Validar(m);
        MascotaDAL.Actualizar(m, usuario.IdUsuario);
    }

    public static void Eliminar(int idMascota)
    {
        // Solo el Administrador puede eliminar (RF-05). Se valida aquí y también en el SP.
        var usuario = Sesion.RequerirAdministrador();
        if (idMascota <= 0)
            throw new NegocioException("Seleccione una mascota para eliminar.");
        MascotaDAL.Eliminar(idMascota, usuario.IdUsuario);
    }

    private static void Validar(Mascota m)
    {
        m.Nombre = (m.Nombre ?? "").Trim();
        if (m.Nombre.Length == 0)
            throw new NegocioException("El nombre de la mascota es obligatorio.");
        if (m.Nombre.Length > 50)
            throw new NegocioException("El nombre no puede superar los 50 caracteres.");
        if (!NombreRegex.IsMatch(m.Nombre))
            throw new NegocioException("El nombre contiene caracteres no permitidos.");
        if (m.IdEspecie <= 0)
            throw new NegocioException("Seleccione una especie.");
        if (m.IdPropietario <= 0)
            throw new NegocioException("Seleccione un propietario.");
        if (m.FechaNacimiento.HasValue && m.FechaNacimiento.Value.Date > DateTime.Today)
            throw new NegocioException("La fecha de nacimiento no puede ser futura.");
    }
}
