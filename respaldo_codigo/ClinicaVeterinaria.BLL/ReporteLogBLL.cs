using ClinicaVeterinaria.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicaVeterinaria.BLL;

public static class ReporteLogBLL
{
    public static void GenerarPdf(IList<LogRegistro> registros, string ruta, string filtros)
    {
        // Solo el Administrador puede emitir el reporte (RF-09)
        var admin = Sesion.RequerirAdministrador();

        if (registros == null || registros.Count == 0)
            throw new NegocioException("No hay registros para incluir en el reporte.");
        if (string.IsNullOrWhiteSpace(ruta))
            throw new NegocioException("Indique dónde guardar el archivo.");

        QuestPDF.Settings.License = LicenseType.Community;

        var emitidoPor = admin.NombreUsuario;
        var fechaEmision = DateTime.Now;

        try
        {
            Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4.Landscape());
                    pagina.Margin(28);
                    pagina.DefaultTextStyle(x => x.FontSize(9));

                    pagina.Header().Column(col =>
                    {
                        col.Item().Text("Reporte de auditoría").FontSize(18).Bold().FontColor(Colors.Teal.Darken2);
                        col.Item().Text("Sistema de Gestión de Clínica Veterinaria").FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(4).Text($"Emitido el {fechaEmision:dd-MM-yyyy HH:mm} por {emitidoPor}");
                        col.Item().Text($"Filtros: {filtros}");
                        col.Item().PaddingBottom(8).Text($"Total de registros: {registros.Count}");
                    });

                    pagina.Content().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(34);   // ID
                            c.ConstantColumn(92);   // Fecha y hora
                            c.ConstantColumn(68);   // Usuario
                            c.ConstantColumn(50);   // Acción
                            c.ConstantColumn(40);   // Registro
                            c.RelativeColumn();     // Valor anterior
                            c.RelativeColumn();     // Valor nuevo
                        });

                        tabla.Header(h =>
                        {
                            h.Cell().Element(Cabecera).Text("ID").Bold().FontColor(Colors.White);
                            h.Cell().Element(Cabecera).Text("Fecha y hora").Bold().FontColor(Colors.White);
                            h.Cell().Element(Cabecera).Text("Usuario").Bold().FontColor(Colors.White);
                            h.Cell().Element(Cabecera).Text("Acción").Bold().FontColor(Colors.White);
                            h.Cell().Element(Cabecera).Text("Reg.").Bold().FontColor(Colors.White);
                            h.Cell().Element(Cabecera).Text("Valor anterior").Bold().FontColor(Colors.White);
                            h.Cell().Element(Cabecera).Text("Valor nuevo").Bold().FontColor(Colors.White);
                        });

                        foreach (var r in registros)
                        {
                            tabla.Cell().Element(Celda).Text(r.IdLog.ToString());
                            tabla.Cell().Element(Celda).Text(r.FechaHora.ToString("dd-MM-yyyy HH:mm:ss"));
                            tabla.Cell().Element(Celda).Text(r.Usuario ?? "(sin usuario)");
                            tabla.Cell().Element(Celda).Text(r.Accion);
                            tabla.Cell().Element(Celda).Text(r.IdRegistro.ToString());
                            tabla.Cell().Element(Celda).Text(r.ValorAnterior ?? "");
                            tabla.Cell().Element(Celda).Text(r.ValorNuevo ?? "");
                        }
                    });

                    pagina.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                });
            }).GeneratePdf(ruta);
        }
        catch (IOException)
        {
            throw new NegocioException("No se pudo guardar el archivo. Verifique que no esté abierto en otro programa.");
        }
        catch (UnauthorizedAccessException)
        {
            throw new NegocioException("No tiene permiso para guardar en esa carpeta. Elija otra ubicación.");
        }
    }

    private static IContainer Cabecera(IContainer contenedor)
    {
        return contenedor.Background(Colors.Teal.Darken2).Padding(4);
    }

    private static IContainer Celda(IContainer contenedor)
    {
        return contenedor.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3);
    }
}
