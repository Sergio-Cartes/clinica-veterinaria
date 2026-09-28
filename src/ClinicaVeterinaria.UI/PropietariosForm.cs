using ClinicaVeterinaria.BLL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.UI;

public class PropietariosForm : Form
{
    private readonly TextBox txtBuscar = new();
    private readonly TextBox txtRut = new();
    private readonly TextBox txtNombre = new();
    private readonly TextBox txtTelefono = new();
    private readonly TextBox txtCorreo = new();
    private readonly DataGridView dgv = new();
    private int idSeleccionado = 0;

    public PropietariosForm()
    {
        Text = "Propietarios";
        Font = Interfaz.Fuente;
        ClientSize = new Size(980, 620);
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterParent;

        // Búsqueda
        Controls.Add(Interfaz.Etiqueta("Buscar:", 15, 20));
        txtBuscar.SetBounds(75, 16, 320, 27);
        Controls.Add(txtBuscar);
        var btnBuscar = Interfaz.Boton("Buscar", 405, 13, 90);
        var btnTodos = Interfaz.Boton("Ver todos", 505, 13, 100);
        Controls.Add(btnBuscar);
        Controls.Add(btnTodos);
        AcceptButton = btnBuscar;

        // Tabla
        dgv.SetBounds(15, 60, 950, 290);
        dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Interfaz.EstiloGrid(dgv);
        dgv.Columns.Add(Interfaz.Columna("IdPropietario", "ID", 40));
        dgv.Columns.Add(Interfaz.Columna("Rut", "RUT", 90));
        dgv.Columns.Add(Interfaz.Columna("Nombre", "Nombre", 160));
        dgv.Columns.Add(Interfaz.Columna("Telefono", "Teléfono", 100));
        dgv.Columns.Add(Interfaz.Columna("Correo", "Correo", 160));
        Controls.Add(dgv);

        // Datos del propietario
        var grupo = new GroupBox
        {
            Text = "Datos del propietario",
            Location = new Point(15, 365),
            Size = new Size(950, 165),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        grupo.Controls.Add(Interfaz.Etiqueta("RUT * (ej: 12345678-9)", 15, 30));
        txtRut.SetBounds(15, 52, 200, 27);
        txtRut.MaxLength = 12;
        grupo.Controls.Add(txtRut);

        grupo.Controls.Add(Interfaz.Etiqueta("Nombre *", 240, 30));
        txtNombre.SetBounds(240, 52, 380, 27);
        txtNombre.MaxLength = 100;
        grupo.Controls.Add(txtNombre);

        grupo.Controls.Add(Interfaz.Etiqueta("Teléfono *", 15, 95));
        txtTelefono.SetBounds(15, 117, 200, 27);
        txtTelefono.MaxLength = 20;
        grupo.Controls.Add(txtTelefono);

        grupo.Controls.Add(Interfaz.Etiqueta("Correo (opcional)", 240, 95));
        txtCorreo.SetBounds(240, 117, 380, 27);
        txtCorreo.MaxLength = 100;
        grupo.Controls.Add(txtCorreo);

        Controls.Add(grupo);

        // Botones
        var btnNuevo = Interfaz.Boton("Nuevo", 15, 545, 110);
        var btnGuardar = Interfaz.Boton("Guardar", 135, 545, 110);
        btnNuevo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        Controls.Add(btnNuevo);
        Controls.Add(btnGuardar);

        // Eventos
        Load += (s, e) =>
        {
            CargarGrid();
            Limpiar();
        };
        btnBuscar.Click += (s, e) => CargarGrid();
        btnTodos.Click += (s, e) => { txtBuscar.Clear(); CargarGrid(); Limpiar(); };
        btnNuevo.Click += (s, e) => Limpiar();
        btnGuardar.Click += (s, e) => Guardar();
        dgv.SelectionChanged += (s, e) => MostrarSeleccion();
    }

    private void CargarGrid()
    {
        try
        {
            dgv.DataSource = PropietarioBLL.Listar(txtBuscar.Text);
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }

    private void Limpiar()
    {
        idSeleccionado = 0;
        txtRut.Clear();
        txtNombre.Clear();
        txtTelefono.Clear();
        txtCorreo.Clear();
        txtRut.Enabled = true; // el RUT solo se puede escribir al registrar
        dgv.ClearSelection();
        txtRut.Focus();
    }

    private void MostrarSeleccion()
    {
        if (dgv.SelectedRows.Count == 0)
            return;
        if (dgv.SelectedRows[0].DataBoundItem is not Propietario p)
            return;

        idSeleccionado = p.IdPropietario;
        txtRut.Text = p.Rut;
        txtRut.Enabled = false;
        txtNombre.Text = p.Nombre;
        txtTelefono.Text = p.Telefono;
        txtCorreo.Text = p.Correo ?? "";
    }

    private void Guardar()
    {
        try
        {
            var p = new Propietario
            {
                IdPropietario = idSeleccionado,
                Rut = txtRut.Text,
                Nombre = txtNombre.Text,
                Telefono = txtTelefono.Text,
                Correo = txtCorreo.Text
            };

            if (idSeleccionado == 0)
            {
                PropietarioBLL.Insertar(p);
                Interfaz.Info("Propietario registrado correctamente.");
            }
            else
            {
                PropietarioBLL.Actualizar(p);
                Interfaz.Info("Propietario actualizado correctamente.");
            }

            CargarGrid();
            Limpiar();
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }
}
