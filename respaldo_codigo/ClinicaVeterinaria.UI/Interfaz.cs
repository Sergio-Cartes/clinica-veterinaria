using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.UI;

/// <summary>
/// Utilidades visuales compartidas por todos los formularios.
/// </summary>
internal static class Interfaz
{
    public static readonly Font Fuente = new("Segoe UI", 10F);
    public static readonly Color Acento = Color.FromArgb(23, 120, 122);

    public static Button Boton(string texto, int x, int y, int ancho = 110)
    {
        var boton = new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(ancho, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Acento,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderSize = 0;
        return boton;
    }

    public static Label Etiqueta(string texto, int x, int y)
    {
        return new Label { Text = texto, Location = new Point(x, y), AutoSize = true };
    }

    public static void EstiloGrid(DataGridView g)
    {
        g.AutoGenerateColumns = false;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.ReadOnly = true;
        g.MultiSelect = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.RowHeadersVisible = false;
        g.BackgroundColor = Color.White;
        g.BorderStyle = BorderStyle.None;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 34;
        g.ColumnHeadersDefaultCellStyle.BackColor = Acento;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 246, 245);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 228, 227);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
    }

    public static DataGridViewTextBoxColumn Columna(string propiedad, string titulo, float peso = 100, string? formato = null)
    {
        var columna = new DataGridViewTextBoxColumn
        {
            Name = propiedad,
            DataPropertyName = propiedad,
            HeaderText = titulo,
            FillWeight = peso
        };
        if (formato != null)
            columna.DefaultCellStyle.Format = formato;
        return columna;
    }

    public static void Info(string mensaje)
    {
        MessageBox.Show(mensaje, "Clínica Veterinaria", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void MostrarError(string mensaje)
    {
        MessageBox.Show(mensaje, "Clínica Veterinaria", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static bool Confirmar(string mensaje)
    {
        return MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    // Los errores de negocio se muestran tal cual; cualquier otro error, con un mensaje genérico (RNF-08)
    public static void Manejar(Exception ex)
    {
        if (ex is NegocioException)
            MostrarError(ex.Message);
        else
            MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
    }
}
