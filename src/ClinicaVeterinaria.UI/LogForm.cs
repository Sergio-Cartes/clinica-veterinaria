using System.Diagnostics;
using ClinicaVeterinaria.BLL;
using ClinicaVeterinaria.Entidades;

namespace ClinicaVeterinaria.UI;

public class LogForm : Form
{
    private readonly DateTimePicker dtpDesde = new();
    private readonly DateTimePicker dtpHasta = new();
    private readonly ComboBox cboUsuario = new();
    private readonly ComboBox cboAccion = new();
    private readonly DataGridView dgv = new();
    private readonly Label lblTotal = new();
    private List<LogRegistro> registros = new();

    public LogForm()
    {
        Text = "Log de auditoría";
        Font = Interfaz.Fuente;
        ClientSize = new Size(1100, 620);
        MinimumSize = new Size(1000, 560);
        StartPosition = FormStartPosition.CenterParent;

        // Filtros
        Controls.Add(Interfaz.Etiqueta("Desde", 15, 15));
        dtpDesde.SetBounds(15, 38, 150, 27);
        dtpDesde.Format = DateTimePickerFormat.Short;
        dtpDesde.ShowCheckBox = true;
        dtpDesde.Checked = false;
        Controls.Add(dtpDesde);

        Controls.Add(Interfaz.Etiqueta("Hasta", 180, 15));
        dtpHasta.SetBounds(180, 38, 150, 27);
        dtpHasta.Format = DateTimePickerFormat.Short;
        dtpHasta.ShowCheckBox = true;
        dtpHasta.Checked = false;
        Controls.Add(dtpHasta);

        Controls.Add(Interfaz.Etiqueta("Usuario", 345, 15));
        cboUsuario.SetBounds(345, 38, 170, 27);
        cboUsuario.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(cboUsuario);

        Controls.Add(Interfaz.Etiqueta("Acción", 530, 15));
        cboAccion.SetBounds(530, 38, 130, 27);
        cboAccion.DropDownStyle = ComboBoxStyle.DropDownList;
        cboAccion.Items.AddRange(new object[] { "(Todas)", "INSERT", "UPDATE", "DELETE" });
        cboAccion.SelectedIndex = 0;
        Controls.Add(cboAccion);

        var btnConsultar = Interfaz.Boton("Consultar", 680, 34, 110);
        var btnPdf = Interfaz.Boton("Generar PDF", 800, 34, 130);
        Controls.Add(btnConsultar);
        Controls.Add(btnPdf);
        AcceptButton = btnConsultar;

        // Tabla
        dgv.SetBounds(15, 85, 1070, 490);
        dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Interfaz.EstiloGrid(dgv);
        dgv.Columns.Add(Interfaz.Columna("IdLog", "ID", 35));
        dgv.Columns.Add(Interfaz.Columna("FechaHora", "Fecha y hora", 95, "dd-MM-yyyy HH:mm:ss"));
        dgv.Columns.Add(Interfaz.Columna("Usuario", "Usuario", 70));
        dgv.Columns.Add(Interfaz.Columna("Accion", "Acción", 60));
        dgv.Columns.Add(Interfaz.Columna("IdRegistro", "Reg.", 40));
        dgv.Columns.Add(Interfaz.Columna("ValorAnterior", "Valor anterior", 200));
        dgv.Columns.Add(Interfaz.Columna("ValorNuevo", "Valor nuevo", 200));
        Controls.Add(dgv);

        lblTotal.AutoSize = true;
        lblTotal.Location = new Point(15, 588);
        lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        Controls.Add(lblTotal);

        Load += (s, e) =>
        {
            CargarUsuarios();
            Consultar();
        };
        btnConsultar.Click += (s, e) => Consultar();
        btnPdf.Click += (s, e) => GenerarPdf();
    }

    private void CargarUsuarios()
    {
        try
        {
            var lista = UsuarioBLL.ListarParaFiltro();
            lista.Insert(0, new Usuario { IdUsuario = 0, NombreUsuario = "(Todos)" });
            cboUsuario.DisplayMember = "NombreUsuario";
            cboUsuario.ValueMember = "IdUsuario";
            cboUsuario.DataSource = lista;
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }

    private void Consultar()
    {
        try
        {
            DateTime? desde = dtpDesde.Checked ? dtpDesde.Value.Date : null;
            DateTime? hasta = dtpHasta.Checked ? dtpHasta.Value.Date : null;

            int? idUsuario = null;
            if (cboUsuario.SelectedValue is int id && id > 0)
                idUsuario = id;

            string? accion = null;
            if (cboAccion.SelectedIndex > 0)
                accion = cboAccion.SelectedItem?.ToString();

            registros = LogBLL.Consultar(desde, hasta, idUsuario, accion);
            dgv.DataSource = registros;
            lblTotal.Text = $"{registros.Count} registro(s) encontrado(s)";
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }

    private string TextoFiltros()
    {
        var desde = dtpDesde.Checked ? dtpDesde.Value.ToString("dd-MM-yyyy") : "sin límite";
        var hasta = dtpHasta.Checked ? dtpHasta.Value.ToString("dd-MM-yyyy") : "sin límite";
        var usuario = cboUsuario.SelectedIndex > 0 ? cboUsuario.Text : "todos";
        var accion = cboAccion.SelectedIndex > 0 ? cboAccion.Text : "todas";
        return $"Desde {desde} | Hasta {hasta} | Usuario: {usuario} | Acción: {accion}";
    }

    private void GenerarPdf()
    {
        if (registros.Count == 0)
        {
            Interfaz.Info("No hay registros para exportar. Primero consulte el log.");
            return;
        }

        using var dialogo = new SaveFileDialog
        {
            Title = "Guardar reporte PDF",
            Filter = "Archivo PDF (*.pdf)|*.pdf",
            FileName = $"LogAuditoria_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
        };
        if (dialogo.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            ReporteLogBLL.GenerarPdf(registros, dialogo.FileName, TextoFiltros());
            Interfaz.Info("El reporte PDF se generó correctamente.");
            Process.Start(new ProcessStartInfo(dialogo.FileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Interfaz.Manejar(ex);
        }
    }
}
