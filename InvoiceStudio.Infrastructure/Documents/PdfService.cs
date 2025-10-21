using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;

namespace InvoiceStudio.Infrastructure.Documents;

public class PdfService : IPdfService
{
    private readonly ILogger _logger;

    public PdfService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.Information("Generating PDF for invoice {InvoiceNumber}", invoice.InvoiceNumber);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header()
                            .Text($"INVOICE {invoice.InvoiceNumber}")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(20);

                                // Invoice info and client details will go here
                                x.Item().Text("PDF generation successful!")
                                    .FontSize(16);
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating PDF for invoice {InvoiceNumber}", invoice.InvoiceNumber);
                throw;
            }
        });
    }

    public async Task<string> GenerateInvoicePdfFileAsync(Invoice invoice, string? outputPath = null)
    {
        var pdfBytes = await GenerateInvoicePdfAsync(invoice);

        outputPath ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "InvoiceStudio",
            "PDFs",
            $"Invoice_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllBytesAsync(outputPath, pdfBytes);

        _logger.Information("PDF saved to {OutputPath}", outputPath);
        return outputPath;
    }
}