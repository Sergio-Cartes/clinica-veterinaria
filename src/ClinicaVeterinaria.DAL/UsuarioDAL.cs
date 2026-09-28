using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.DAL;

public static class UsuarioDAL
{
    public static Usuario? ObtenerParaLogin(string nombreUsuario)
    {
        var lista = Ejecutor.Consultar("dbo.sp_Usuario_ObtenerParaLogin",
            p => p.AddWithValue("@NombreUsuario", nombreUsuario),
            rd => new Usuario
            {
                IdUsuario = (int)rd["IdUsuario"],
                NombreUsuario = (string)rd["NombreUsuario"],
                ClaveHash = (byte[])rd["ClaveHash"],
                Salt = (byte[])rd["Salt"],
                Rol = (string)rd["Rol"],
                IntentosFallidos = (int)rd["IntentosFallidos"],
                Bloqueado = (bool)rd["Bloqueado"]
            });
        return lista.FirstOrDefault();
    }

    public static (int Intentos, bool Bloqueado) RegistrarIntentoFallido(string nombreUsuario)
    {
        var lista = Ejecutor.Consultar("dbo.sp_Usuario_RegistrarIntentoFallido",
            p => p.AddWithValue("@NombreUsuario", nombreUsuario),
            rd => ((int)rd["IntentosFallidos"], (bool)rd["Bloqueado"]));
        return lista.FirstOrDefault();
    }

    public static void RegistrarLoginExitoso(int idUsuario)
    {
        Ejecutor.Ejecutar("dbo.sp_Usuario_RegistrarLoginExitoso",
            p => p.AddWithValue("@IdUsuario", idUsuario));
    }

    public static List<Usuario> Listar(int idSolicitante)
    {
        return Ejecutor.Consultar("dbo.sp_Usuario_Listar",
            p => p.AddWithValue("@IdSolicitante", idSolicitante),
            rd => new Usuario
            {
                IdUsuario = (int)rd["IdUsuario"],
                NombreUsuario = (string)rd["NombreUsuario"],
                Rol = (string)rd["Rol"]
            });
    }
}
