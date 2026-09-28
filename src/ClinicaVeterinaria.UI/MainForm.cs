using ClinicaVeterinaria.BLL;

namespace ClinicaVeterinaria.UI;

public class MainForm : Form
{
    public bool CerroSesion { get; private set; }

    public MainForm()
    {
        var usuario = Sesion.UsuarioActual!;

        Text = "Clínica Veterinaria";
        Font = Interfaz.Fuente;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(500, 340);

        Controls.Add(new Label
        {
            Text = "Clínica Veterinaria",
            Font = new Font("Segoe UI Semibold", 17F),
            ForeColor = Interfaz.Acento,
            AutoSize = true,
            Location = new Point(28, 20)
        });
        Controls.Add(new Label
        {
            Text = $"Sesión: {usuario.NombreUsuario} ({usuario.Rol})",
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(30, 58)
        });

        var btnMascotas = Interfaz.Boton("Mascotas", 30, 100, 440);
        btnMascotas.Height = 44;
        btnMascotas.Click += (s, e) => { using var f = new MascotasForm(); f.ShowDialog(this); };
        Controls.Add(btnMascotas);

        var btnPropietarios = Interfaz.Boton("Propietarios", 30, 156, 440);
        btnPropietarios.Height = 44;
        btnPropietarios.Click += (s, e) => { using var f = new PropietariosForm(); f.ShowDialog(this); };
        Controls.Add(btnPropietarios);

        var siguienteY = 212;
        if (Sesion.EsAdministrador)
        {
            var btnLog = Interfaz.Boton("Log de auditoría y reporte PDF", 30, siguienteY, 440);
            btnLog.Height = 44;
            btnLog.Click += (s, e) => { using var f = new LogForm(); f.ShowDialog(this); };
            Controls.Add(btnLog);
            siguienteY += 56;
        }

        var btnSalir = Interfaz.Boton("Cerrar sesión", 30, siguienteY, 440);
        btnSalir.BackColor = Color.FromArgb(120, 130, 130);
        btnSalir.Click += (s, e) =>
        {
            CerroSesion = true;
            Sesion.Cerrar();
            Close();
        };
        Controls.Add(btnSalir);
    }
}
