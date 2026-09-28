using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.BLL;

/// <summary>
/// Guarda quién inició sesión y valida permisos por rol (RNF-03).
/// </summary>
public static class Sesion
{
    public static Usuario? UsuarioActual { get; private set; }

    public static bool EsAdministrador => UsuarioActual?.Rol == Roles.Administrador;

    public static void Iniciar(Usuario usuario)
    {
        // Se guarda una copia sin el hash ni el salt
        UsuarioActual = new Usuario
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol
        };
    }

    public static void Cerrar()
    {
        UsuarioActual = null;
    }

    public static Usuario Requerir()
    {
        return UsuarioActual ?? throw new NegocioException("Debe iniciar sesión para continuar.");
    }

    public static Usuario RequerirAdministrador()
    {
        var usuario = Requerir();
        if (usuario.Rol != Roles.Administrador)
            throw new NegocioException("No tiene permisos para realizar esta acción.");
        return usuario;
    }
}
