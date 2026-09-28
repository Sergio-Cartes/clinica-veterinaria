using ClinicaVeterinaria.BLL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.UI;

public class MascotasForm : Form
{
    private readonly TextBox txtBuscar = new();
    private readonly TextBox txtNombre = new();
    private readonly DateTimePicker dtpNacimiento = new();
    private readonly ComboBox cboEspecie = new();
    private readonly ComboBox cboPropietario = new();
    private readonly DataGridView dgv = new();
    private readonly Button btnEliminar;
    private int idSeleccionado = 0;

    public MascotasForm()
    {
        Text = "Mascotas";
        Font = Interfaz.Fuente;
        ClientSize = new Size(980, 620);
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterParent;

        // Búsqueda
        Controls.Add(Interfaz.Etiqueta("Buscar:", 15, 20));
        txtBuscar.SetBounds(75, 16, 320, 27);
        Controls.Add(txtBuscar);
        var btnBuscar = Interfaz.Boton("Buscar", 405, 13, 90);
        var btnTodas = Interfaz.Boton("Ver todas", 505, 13, 100);
        Controls.Add(btnBuscar);
        Controls.Add(btnTodas);
        AcceptButton = btnBuscar;

        // Tabla
        dgv.SetBounds(15, 60, 950, 290);
        dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Interfaz.EstiloGrid(dgv);
        dgv.Columns.Add(Interfaz.Columna("IdMascota", "ID", 40));
        dgv.Columns.Add(Interfaz.Columna("Nombre", "Nombre", 110));
        dgv.Columns.Add(Interfaz.Columna("NombreEspecie", "Especie", 80));
        dgv.Columns.Add(Interfaz.Columna("FechaNacimiento", "Nacimiento", 90, "dd-MM-yyyy"));
        dgv.Columns.Add(Interfaz.Columna("NombrePropietario", "Propietario", 160));
        dgv.Columns.Add(Interfaz.Columna("RutPropietario", "RUT propietario", 100));
        Controls.Add(dgv);

        // Datos de la mascota
        var grupo = new GroupBox
        {
            Text = "Datos de la mascota",
            Location = new Point(15, 365),
            Size = new Size(950, 165),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        grupo.Controls.Add(Interfaz.Etiqueta("Nombre *", 15, 30));
        txtNombre.SetBounds(15, 52, 260, 27);
        txtNombre.MaxLength = 50;
        grupo.Controls.Add(txtNombre);

        grupo.Controls.Add(Interfaz.Etiqueta("Fecha de nacimiento (opcional)", 295, 30));
        dtpNacimiento.SetBounds(295, 52, 220, 27);
        dtpNacimiento.Format = DateTimePickerFormat.Short;
        dtpNacimiento.ShowCheckBox = true;
        dtpNacimiento.MaxDate = DateTime.Today;
        dtpNacimiento.Checked = false;
        grupo.Controls.Add(dtpNacimiento);

        grupo.Controls.Add(Interfaz.Etiqueta("Especie *", 535, 30));
        cboEspecie.SetBounds(535, 52, 200, 27);
        cboEspecie.DropDownStyle = ComboBoxStyle.DropDownList;
        grupo.Controls.Add(cboEspecie);

        grupo.Controls.Add(Interfaz.Etiqueta("Propietario *", 15, 95));
        cboPropietario.SetBounds(15, 117, 500, 27);
        cboPropietario.DropDownStyle = ComboBoxStyle.DropDownList;
        grupo.Controls.Add(cboPropietario);

        Controls.Add(grupo);

        // Botones
        var btnNuevo = Interfaz.Boton("Nuevo", 15, 545, 110);
        var btnGuardar = Interfaz.Boton("Guardar", 135, 545, 110);
        btnEliminar = Interfaz.Boton("Eliminar", 255, 545, 110);
        btnEliminar.BackColor = Color.FromArgb(176, 58, 46);
        btnEliminar.Visible = Sesion.EsAdministrador; // solo el Administrador ve este botón
        foreach (var b in new[] { btnNuevo, btnGuardar, btnEliminar })
            b.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        Controls.Add(btnNuevo);
        Controls.Add(btnGuardar);
        Controls.Add(btnEliminar);

        // Eventos
        Load += (s, e) =>
        {
            CargarCombos();
            CargarGrid();
            Limpiar();
        };
        btnBuscar.Click += (s, e) => CargarGrid();
        btnTodas.Click += (s, e) => { txtBuscar.Clear(); CargarGrid(); Limpiar(); };
        btnNuevo.Click += (s, e) => Limpiar();
        btnGuardar.Click += (s, e) => Guardar();
        btnEliminar.Click += (s, e) => Eliminar();
        dgv.SelectionChanged += (s, e) => MostrarSeleccion();
    }

    private void CargarCombos()
    {
        try
        {
            cboEspecie.DisplayMember = "Nombre";
            cboEspecie.ValueMember = "IdEspecie";
            cboEspecie.DataSource = EspecieBLL.Listar();

            cboPropietario.DisplayMember = "Descripcion";
            cboPropietario.ValueMember = "IdPropietario";
            cboPropietario.DataSource = PropietarioBLL.Listar(null);
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }

    private void CargarGrid()
    {
        try
        {
            dgv.DataSource = MascotaBLL.Listar(txtBuscar.Text);
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }

    private void Limpiar()
    {
        idSeleccionado = 0;
        txtNombre.Clear();
        dtpNacimiento.Checked = false;
        cboEspecie.SelectedIndex = -1;
        cboPropietario.SelectedIndex = -1;
        dgv.ClearSelection();
        txtNombre.Focus();
    }

    private void MostrarSeleccion()
    {
        if (dgv.SelectedRows.Count == 0)
            return;
        if (dgv.SelectedRows[0].DataBoundItem is not Mascota m)
            return;

        idSeleccionado = m.IdMascota;
        txtNombre.Text = m.Nombre;
        if (m.FechaNacimiento.HasValue)
        {
            dtpNacimiento.Checked = true;
            dtpNacimiento.Value = m.FechaNacimiento.Value;
        }
        else
        {
            dtpNacimiento.Checked = false;
        }
        cboEspecie.SelectedValue = m.IdEspecie;
        cboPropietario.SelectedValue = m.IdPropietario;
    }

    private void Guardar()
    {
        try
        {
            var m = new Mascota
            {
                IdMascota = idSeleccionado,
                Nombre = txtNombre.Text,
                FechaNacimiento = dtpNacimiento.Checked ? dtpNacimiento.Value.Date : null,
                IdEspecie = cboEspecie.SelectedValue is int especie ? especie : 0,
                IdPropietario = cboPropietario.SelectedValue is int propietario ? propietario : 0
            };

            if (idSeleccionado == 0)
            {
                MascotaBLL.Insertar(m);
                Interfaz.Info("Mascota registrada correctamente.");
            }
            else
            {
                MascotaBLL.Actualizar(m);
                Interfaz.Info("Mascota actualizada correctamente.");
            }

            CargarGrid();
            Limpiar();
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }

    private void Eliminar()
    {
        if (idSeleccionado == 0)
        {
            Interfaz.Info("Seleccione una mascota de la tabla para eliminarla.");
            return;
        }
        if (!Interfaz.Confirmar("¿Seguro que desea eliminar esta mascota?\nLa eliminación queda registrada en el log."))
            return;

        try
        {
            MascotaBLL.Eliminar(idSeleccionado);
            Interfaz.Info("Mascota eliminada.");
            CargarGrid();
            Limpiar();
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }
}
