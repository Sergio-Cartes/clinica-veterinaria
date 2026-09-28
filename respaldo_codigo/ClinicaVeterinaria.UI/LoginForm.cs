using ClinicaVeterinaria.BLL;

namespace ClinicaVeterinaria.UI;

public class LoginForm : Form
{
    private readonly TextBox txtUsuario = new();
    private readonly TextBox txtClave = new();

    public LoginForm()
    {
        Text = "Iniciar sesión";
        Font = Interfaz.Fuente;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(360, 290);

        Controls.Add(new Label
        {
            Text = "Clínica Veterinaria",
            Font = new Font("Segoe UI Semibold", 17F),
            ForeColor = Interfaz.Acento,
            AutoSize = true,
            Location = new Point(24, 22)
        });
        Controls.Add(new Label
        {
            Text = "Ingrese con su usuario y su clave",
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(26, 60)
        });

        Controls.Add(Interfaz.Etiqueta("Usuario", 26, 100));
        txtUsuario.SetBounds(26, 122, 308, 27);
        txtUsuario.MaxLength = 50;
        Controls.Add(txtUsuario);

        Controls.Add(Interfaz.Etiqueta("Clave", 26, 160));
        txtClave.SetBounds(26, 182, 308, 27);
        txtClave.UseSystemPasswordChar = true;
        txtClave.MaxLength = 100;
        Controls.Add(txtClave);

        var btnIngresar = Interfaz.Boton("Ingresar", 26, 228, 308);
        btnIngresar.Click += Ingresar;
        Controls.Add(btnIngresar);
        AcceptButton = btnIngresar;
    }

    private void Ingresar(object? sender, EventArgs e)
    {
        try
        {
            UsuarioBLL.Login(txtUsuario.Text, txtClave.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
            txtClave.Clear();
            txtClave.Focus();
        }
    }
}
