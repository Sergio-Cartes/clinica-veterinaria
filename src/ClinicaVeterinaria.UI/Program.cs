namespace ClinicaVeterinaria.UI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Mientras el usuario cierre sesión (y no la ventana), vuelve al login
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK)
                break;

            using var principal = new MainForm();
            Application.Run(principal);

            if (!principal.CerroSesion)
                break;
        }
    }
}
