using ClosedXML.Excel;
using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERPContable.API.Services;

public sealed class ReportesExportService
{
    private readonly IReportesContablesService _reportes;

    public ReportesExportService(IReportesContablesService reportes) => _reportes = reportes;

    public async Task<ExportFile> ExcelAsync(int empresaId, string reporte, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(NombreReporte(reporte));
        var titulo = NombreReporte(reporte);
        sheet.Cell(1, 1).Value = titulo;
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;

        switch (reporte.ToLowerInvariant())
        {
            case "libro-diario":
                var diario = await _reportes.ObtenerLibroDiarioAsync(empresaId, desde, hasta, ct);
                EscribirEncabezado(sheet, 3, "Fecha", "Asiento", "Glosa", "Cuenta", "Nombre", "Debe", "Haber");
                var filaDiario = 4;
                foreach (var x in diario.Lineas) { sheet.Cell(filaDiario, 1).Value = x.Fecha; sheet.Cell(filaDiario, 2).Value = x.AsientoId; sheet.Cell(filaDiario, 3).Value = x.Glosa; sheet.Cell(filaDiario, 4).Value = x.CodigoCuenta; sheet.Cell(filaDiario, 5).Value = x.NombreCuenta; sheet.Cell(filaDiario, 6).Value = x.Debe; sheet.Cell(filaDiario, 7).Value = x.Haber; filaDiario++; }
                EscribirTotales(sheet, filaDiario, diario.TotalDebe, diario.TotalHaber, 6, 7);
                break;
            case "libro-mayor":
                var mayor = await _reportes.ObtenerLibroMayorAsync(empresaId, desde, hasta, ct);
                EscribirEncabezado(sheet, 3, "Cuenta", "Nombre", "Tipo", "Debe", "Haber", "Saldo deudor", "Saldo acreedor");
                var filaMayor = 4;
                foreach (var x in mayor) { sheet.Cell(filaMayor, 1).Value = x.Codigo; sheet.Cell(filaMayor, 2).Value = x.Nombre; sheet.Cell(filaMayor, 3).Value = x.Tipo; sheet.Cell(filaMayor, 4).Value = x.TotalDebe; sheet.Cell(filaMayor, 5).Value = x.TotalHaber; sheet.Cell(filaMayor, 6).Value = x.SaldoDeudor; sheet.Cell(filaMayor, 7).Value = x.SaldoAcreedor; filaMayor++; }
                break;
            case "balance-comprobacion":
                var comprobacion = await _reportes.ObtenerBalanceComprobacionAsync(empresaId, desde, hasta, ct);
                EscribirEncabezado(sheet, 3, "Cuenta", "Nombre", "Tipo", "Debe", "Haber", "Saldo deudor", "Saldo acreedor");
                var filaComprobacion = 4;
                foreach (var x in comprobacion.Lineas) { sheet.Cell(filaComprobacion, 1).Value = x.Codigo; sheet.Cell(filaComprobacion, 2).Value = x.Nombre; sheet.Cell(filaComprobacion, 3).Value = x.Tipo; sheet.Cell(filaComprobacion, 4).Value = x.Debe; sheet.Cell(filaComprobacion, 5).Value = x.Haber; sheet.Cell(filaComprobacion, 6).Value = x.SaldoDeudor; sheet.Cell(filaComprobacion, 7).Value = x.SaldoAcreedor; filaComprobacion++; }
                EscribirTotales(sheet, filaComprobacion, comprobacion.TotalDebe, comprobacion.TotalHaber, 4, 5);
                break;
            case "estado-resultados":
                var resultados = await _reportes.ObtenerEstadoResultadosAsync(empresaId, desde, hasta, ct);
                EscribirResumen(sheet, ("Ingresos", resultados.Ingresos), ("Gastos", resultados.Gastos), ("Resultado", resultados.Resultado));
                break;
            case "balance-general":
                var balance = await _reportes.ObtenerBalanceGeneralAsync(empresaId, hasta, ct);
                EscribirResumen(sheet, ("Activos", balance.Activos), ("Pasivos", balance.Pasivos), ("Patrimonio", balance.Patrimonio), ("Resultado del período", balance.ResultadoDelPeriodo), ("Pasivo + patrimonio", balance.TotalPasivoPatrimonio));
                break;
            case "flujo-caja":
                var flujo = await _reportes.ObtenerFlujoCajaAsync(empresaId, desde, hasta, ct);
                EscribirEncabezado(sheet, 3, "Fecha", "Asiento", "Glosa", "Cuenta", "Nombre", "Entrada", "Salida");
                var filaFlujo = 4;
                foreach (var x in flujo.Movimientos) { sheet.Cell(filaFlujo, 1).Value = x.Fecha; sheet.Cell(filaFlujo, 2).Value = x.AsientoId; sheet.Cell(filaFlujo, 3).Value = x.Glosa; sheet.Cell(filaFlujo, 4).Value = x.CodigoCuenta; sheet.Cell(filaFlujo, 5).Value = x.NombreCuenta; sheet.Cell(filaFlujo, 6).Value = x.Entrada; sheet.Cell(filaFlujo, 7).Value = x.Salida; filaFlujo++; }
                EscribirTotales(sheet, filaFlujo, flujo.Entradas, flujo.Salidas, 6, 7);
                break;
            default: throw new ArgumentException("Reporte no soportado.");
        }

        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ExportFile(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reporte}.xlsx");
    }

    public async Task<ExportFile> PdfAsync(int empresaId, string reporte, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var model = await ObtenerModeloAsync(empresaId, reporte, desde, hasta, ct);
        var document = Document.Create(container => container.Page(page =>
        {
            page.Margin(28);
            page.Header().Text(NombreReporte(reporte)).FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
            page.Content().PaddingTop(16).Column(column =>
            {
                column.Item().Text($"Empresa: {empresaId} | Período: {desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}").FontSize(9);
                column.Item().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(2); columns.RelativeColumn(3); columns.RelativeColumn(2); });
                    table.Header(header => { header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Concepto").FontColor(Colors.White).Bold(); header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Descripción").FontColor(Colors.White).Bold(); header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Importe").FontColor(Colors.White).Bold(); });
                    foreach (var row in model) { table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.Concepto); table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.Descripcion); table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(row.Importe.ToString("N2")); }
                });
            });
            page.Footer().AlignCenter().Text(text => { text.Span("ERP Contable · "); text.CurrentPageNumber(); });
        }));
        return new ExportFile(document.GeneratePdf(), "application/pdf", $"{reporte}.pdf");
    }

    private async Task<List<PdfRow>> ObtenerModeloAsync(int empresaId, string reporte, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        switch (reporte.ToLowerInvariant())
        {
            case "libro-diario":
                return (await _reportes.ObtenerLibroDiarioAsync(empresaId, desde, hasta, ct)).Lineas.Select(x => new PdfRow(x.CodigoCuenta, x.NombreCuenta, x.Debe - x.Haber)).ToList();
            case "libro-mayor":
                return (await _reportes.ObtenerLibroMayorAsync(empresaId, desde, hasta, ct)).Select(x => new PdfRow(x.Codigo, x.Nombre, x.SaldoDeudor - x.SaldoAcreedor)).ToList();
            case "balance-comprobacion":
                return (await _reportes.ObtenerBalanceComprobacionAsync(empresaId, desde, hasta, ct)).Lineas.Select(x => new PdfRow(x.Codigo, x.Nombre, x.SaldoDeudor - x.SaldoAcreedor)).ToList();
            case "estado-resultados":
                var resultados = await _reportes.ObtenerEstadoResultadosAsync(empresaId, desde, hasta, ct);
                return [new PdfRow("Ingresos", "Ingresos del período", resultados.Ingresos), new PdfRow("Gastos", "Gastos del período", resultados.Gastos), new PdfRow("Resultado", "Resultado del período", resultados.Resultado)];
            case "balance-general":
                var balance = await _reportes.ObtenerBalanceGeneralAsync(empresaId, hasta, ct);
                return [new PdfRow("Activos", "Total activos", balance.Activos), new PdfRow("Pasivos", "Total pasivos", balance.Pasivos), new PdfRow("Patrimonio", "Patrimonio", balance.Patrimonio), new PdfRow("Resultado", "Resultado del período", balance.ResultadoDelPeriodo), new PdfRow("Comprobación", "Pasivo + patrimonio", balance.TotalPasivoPatrimonio)];
            case "flujo-caja":
                return (await _reportes.ObtenerFlujoCajaAsync(empresaId, desde, hasta, ct)).Movimientos.Select(x => new PdfRow(x.CodigoCuenta, x.NombreCuenta, x.Entrada - x.Salida)).ToList();
            default:
                throw new ArgumentException("Reporte no soportado.");
        }
    }

    private static void EscribirEncabezado(IXLWorksheet sheet, int fila, params string[] valores)
    {
        for (var i = 0; i < valores.Length; i++) { var cell = sheet.Cell(fila, i + 1); cell.Value = valores[i]; cell.Style.Font.Bold = true; cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0D2B55"); cell.Style.Font.FontColor = XLColor.White; }
    }

    private static void EscribirTotales(IXLWorksheet sheet, int fila, decimal primero, decimal segundo, int columnaPrimero, int columnaSegundo)
    {
        sheet.Cell(fila, columnaPrimero - 1).Value = "TOTAL"; sheet.Cell(fila, columnaPrimero - 1).Style.Font.Bold = true; sheet.Cell(fila, columnaPrimero).Value = primero; sheet.Cell(fila, columnaSegundo).Value = segundo; sheet.Range(fila, columnaPrimero - 1, fila, columnaSegundo).Style.Font.Bold = true;
    }

    private static void EscribirResumen(IXLWorksheet sheet, params (string Nombre, decimal Importe)[] filas)
    {
        EscribirEncabezado(sheet, 3, "Concepto", "Importe");
        for (var i = 0; i < filas.Length; i++) { sheet.Cell(i + 4, 1).Value = filas[i].Nombre; sheet.Cell(i + 4, 2).Value = filas[i].Importe; }
    }

    private static string NombreReporte(string reporte) => reporte.ToLowerInvariant() switch
    {
        "libro-diario" => "Libro Diario", "libro-mayor" => "Libro Mayor", "balance-comprobacion" => "Balance Comprobación",
        "estado-resultados" => "Estado de Resultados", "balance-general" => "Balance General", "flujo-caja" => "Flujo de Caja", _ => throw new ArgumentException("Reporte no soportado.")
    };

    private sealed record PdfRow(string Concepto, string Descripcion, decimal Importe);
}

public sealed record ExportFile(byte[] Content, string ContentType, string FileName);
