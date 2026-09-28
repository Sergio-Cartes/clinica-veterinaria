using System.Security.Cryptography;
using System.Text;
using ClinicaVeterinaria.DAL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.BLL;

public static class UsuarioBLL
{
    private const int MaxIntentos = 3;

    public static void Login(string nombreUsuario, string clave)
    {
        nombreUsuario = (nombreUsuario ?? "").Trim();
        if (nombreUsuario.Length == 0 || string.IsNullOrEmpty(clave))
            throw new NegocioException("Ingrese su usuario y su clave.");

        var usuario = UsuarioDAL.ObtenerParaLogin(nombreUsuario);

        // Mismo mensaje si el usuario no existe o la clave está mal (no revela cuál falló)
        if (usuario == null)
            throw new NegocioException("Usuario o clave incorrectos.");

        if (usuario.Bloqueado)
            throw new NegocioException("La cuenta está bloqueada por intentos fallidos. Contacte al administrador.");

        var hashIngresado = CalcularHash(usuario.Salt, clave);
        if (!CryptographicOperations.FixedTimeEquals(hashIngresado, usuario.ClaveHash))
        {
            var (intentos, bloqueado) = UsuarioDAL.RegistrarIntentoFallido(nombreUsuario);
            if (bloqueado)
                throw new NegocioException("Superó los 3 intentos. La cuenta quedó bloqueada.");
            throw new NegocioException($"Usuario o clave incorrectos. Intentos restantes: {MaxIntentos - intentos}.");
        }

        UsuarioDAL.RegistrarLoginExitoso(usuario.IdUsuario);
        Sesion.Iniciar(usuario);
    }

    public static List<Usuario> ListarParaFiltro()
    {
        var admin = Sesion.RequerirAdministrador();
        return UsuarioDAL.Listar(admin.IdUsuario);
    }

    // SHA-256 de (Salt + clave en UTF-16). Es el mismo cálculo que usa el script SQL.
    private static byte[] CalcularHash(byte[] salt, string clave)
    {
        var claveBytes = Encoding.Unicode.GetBytes(clave);
        var todo = new byte[salt.Length + claveBytes.Length];
        Buffer.BlockCopy(salt, 0, todo, 0, salt.Length);
        Buffer.BlockCopy(claveBytes, 0, todo, salt.Length, claveBytes.Length);
        return SHA256.HashData(todo);
    }
}
