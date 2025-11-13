using System.Globalization;
using System.IO;
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
                        page.Margin(20, Unit.Millimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // ===== HEADER =====
                        page.Header().Column(header =>
                        {
                            header.Item().Row(r =>
                            {
                                r.RelativeItem().Text("INVOICE").FontSize(28).Bold();
                                r.ConstantItem(250).Column(meta =>
                                {
                                    meta.Item().AlignRight().Text($"#{invoice.InvoiceNumber}").FontSize(14).Bold();
                                    meta.Item().AlignRight().Text($"Status: {GetStatusText(invoice.Status)}").FontSize(11);
                                    meta.Item().PaddingTop(6).AlignRight().Text($"Issue Date: {invoice.IssueDate:dd/MM/yyyy}");
                                    meta.Item().AlignRight().Text($"Due Date: {invoice.DueDate:dd/MM/yyyy}");
                                    if (!string.IsNullOrWhiteSpace(invoice.Currency))
                                        meta.Item().AlignRight().Text($"Currency: {invoice.Currency}");
                                });
                            });

                            header.Item().PaddingTop(12).LineHorizontal(1);
                        });

                        // ===== MAIN CONTENT =====
                        page.Content().Column(content =>
                        {
                            content.Spacing(18);

                            // ===== COMPANY (LEFT) | CLIENT (RIGHT) =====
                            content.Item().Row(row =>
                            {
                                // ---------- LEFT: COMPANY ----------
                                row.RelativeItem().Column(company =>
                                {
                                    company.Item().Row(logoRow =>
                                    {
                                        logoRow.RelativeItem().Text("FROM").FontSize(12).Bold().AlignLeft();
                                        logoRow.RelativeItem().AlignRight().Text("");
                                    });

                                    company.Item().PaddingTop(5).LineHorizontal(1);

                                    company.Item().PaddingTop(8).Row(lr =>
                                    {
                                        lr.ConstantItem(80).Column(logoBox =>
                                        {
                                            logoBox.Item().Element(c => DrawLogo(c, invoice.Company?.LogoPath, 50, 50));
                                        });

                                        lr.RelativeItem().Column(info =>
                                        {
                                            info.Item().Text(invoice.Company?.Name ?? "Company Name").FontSize(14).Bold();

                                            if (!string.IsNullOrWhiteSpace(invoice.Company?.LegalName))
                                                info.Item().Text($"Legal Name: {invoice.Company.LegalName}").FontSize(9);
                                            if (!string.IsNullOrWhiteSpace(invoice.Company?.LegalForm))
                                                info.Item().Text($"Legal Form: {invoice.Company.LegalForm}").FontSize(9);
                                        });
                                    });

                                    if (!IsAllEmpty(invoice.Company?.Street,
                                                    invoice.Company?.PostalCode,
                                                    invoice.Company?.City,
                                                    invoice.Company?.CountryName))
                                    {
                                        company.Item().PaddingTop(8).Text("ADDRESS").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Street))
                                            company.Item().Text(invoice.Company.Street);
                                        var cityLine = BuildCityLine(invoice.Company?.PostalCode, invoice.Company?.City);
                                        if (!string.IsNullOrWhiteSpace(cityLine))
                                            company.Item().Text(cityLine);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.CountryName))
                                            company.Item().Text(invoice.Company.CountryName);
                                    }

                                    if (!IsAllEmpty(invoice.Company?.Email, invoice.Company?.Phone, invoice.Company?.Website))
                                    {
                                        company.Item().PaddingTop(8).Text("CONTACT").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Email))
                                            company.Item().Text($"Email: {invoice.Company.Email}");
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Phone))
                                            company.Item().Text($"Phone: {invoice.Company.Phone}");
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Website))
                                            company.Item().Text($"Website: {invoice.Company.Website}");
                                    }

                                    var hasRegistrationInfo =
                                        !string.IsNullOrWhiteSpace(invoice.Company?.Siret) ||
                                        !string.IsNullOrWhiteSpace(invoice.Company?.Siren) ||
                                        !string.IsNullOrWhiteSpace(invoice.Company?.ApeCode) ||
                                        !string.IsNullOrWhiteSpace(invoice.Company?.VatNumber) ||
                                        !string.IsNullOrWhiteSpace(invoice.Company?.CvrNumber) ||
                                        !string.IsNullOrWhiteSpace(invoice.Company?.TaxId) ||
                                        !string.IsNullOrWhiteSpace(invoice.Company?.RcsNumber);

                                    if (hasRegistrationInfo)
                                    {
                                        company.Item().PaddingTop(8).Text("REGISTRATION").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Siret))
                                            company.Item().Text($"SIRET: {invoice.Company.Siret}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Siren))
                                            company.Item().Text($"SIREN: {invoice.Company.Siren}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.ApeCode))
                                            company.Item().Text($"APE Code: {invoice.Company.ApeCode}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.VatNumber))
                                            company.Item().Text($"VAT: {invoice.Company.VatNumber}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.CvrNumber))
                                            company.Item().Text($"CVR: {invoice.Company.CvrNumber}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.TaxId))
                                            company.Item().Text($"Tax ID: {invoice.Company.TaxId}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.RcsNumber))
                                            company.Item().Text($"RCS: {invoice.Company.RcsNumber}").FontSize(9);
                                    }

                                    var hasBankInfo =
     !string.IsNullOrWhiteSpace(invoice.Company?.BankName) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.Iban) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.Swift) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.FrenchBankCode) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.FrenchBranchCode) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.FrenchAccountNumber) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.FrenchRibKey) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.DanishRegistrationNumber) ||
     !string.IsNullOrWhiteSpace(invoice.Company?.DanishAccountNumber);

                                    if (hasBankInfo)
                                    {
                                        company.Item().PaddingTop(8).Text("BANK DETAILS").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.BankName))
                                            company.Item().Text($"Bank: {invoice.Company.BankName}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Iban))
                                            company.Item().Text($"IBAN: {FormatIban(invoice.Company.Iban)}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.Swift))
                                            company.Item().Text($"SWIFT/BIC: {invoice.Company.Swift}").FontSize(9);

                                        // French RIB - Display each component separately
                                        var hasFrenchRib =
                                            !string.IsNullOrWhiteSpace(invoice.Company?.FrenchBankCode) ||
                                            !string.IsNullOrWhiteSpace(invoice.Company?.FrenchBranchCode) ||
                                            !string.IsNullOrWhiteSpace(invoice.Company?.FrenchAccountNumber) ||
                                            !string.IsNullOrWhiteSpace(invoice.Company?.FrenchRibKey);

                                        if (hasFrenchRib)
                                        {
                                            if (!string.IsNullOrWhiteSpace(invoice.Company?.FrenchBankCode))
                                                company.Item().Text($"Code Banque: {invoice.Company.FrenchBankCode}").FontSize(9);
                                            if (!string.IsNullOrWhiteSpace(invoice.Company?.FrenchBranchCode))
                                                company.Item().Text($"Code Guichet: {invoice.Company.FrenchBranchCode}").FontSize(9);
                                            if (!string.IsNullOrWhiteSpace(invoice.Company?.FrenchAccountNumber))
                                                company.Item().Text($"Numéro de Compte: {invoice.Company.FrenchAccountNumber}").FontSize(9);
                                            if (!string.IsNullOrWhiteSpace(invoice.Company?.FrenchRibKey))
                                                company.Item().Text($"Clé RIB: {invoice.Company.FrenchRibKey}").FontSize(9);
                                        }

                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.DanishRegistrationNumber))
                                            company.Item().Text($"Reg Nr: {invoice.Company.DanishRegistrationNumber}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Company?.DanishAccountNumber))
                                            company.Item().Text($"Account Nr: {invoice.Company.DanishAccountNumber}").FontSize(9);
                                    }

                                    if (invoice.Company?.IsVatExempt == true && !string.IsNullOrWhiteSpace(invoice.Company.FrenchVatExemptionMention))
                                    {
                                        company.Item().PaddingTop(8).Text("VAT EXEMPTION").FontSize(9).Bold();
                                        company.Item().Text(invoice.Company.FrenchVatExemptionMention).FontSize(9).Italic();
                                    }
                                });

                                row.ConstantItem(40); // Spacer

                                // ---------- RIGHT: CLIENT ----------
                                row.RelativeItem().Column(client =>
                                {
                                    client.Item().Row(logoRow =>
                                    {
                                        logoRow.RelativeItem().Text("BILL TO").FontSize(12).Bold().AlignLeft();
                                        logoRow.RelativeItem().AlignRight().Text("");
                                    });

                                    client.Item().PaddingTop(5).LineHorizontal(1);

                                    client.Item().PaddingTop(8).Row(lr =>
                                    {
                                        lr.ConstantItem(80).Column(logoBox =>
                                        {
                                            logoBox.Item().Element(c => DrawLogo(c, invoice.Client?.LogoPath, 50, 50));
                                        });

                                        lr.RelativeItem().Column(info =>
                                        {
                                            info.Item().Text(invoice.Client?.Name ?? "Client Name").FontSize(14).Bold();
                                            if (!string.IsNullOrWhiteSpace(invoice.Client?.LegalName))
                                                info.Item().Text($"Legal Name: {invoice.Client.LegalName}").FontSize(9);
                                            info.Item().Text($"Type: {invoice.Client?.Type.ToString() ?? "Individual"}").FontSize(9);
                                            if (!string.IsNullOrWhiteSpace(invoice.Client?.Category))
                                                info.Item().Text($"Category: {invoice.Client.Category}").FontSize(9);
                                        });
                                    });

                                    if (!IsAllEmpty(invoice.Client?.Street,
                                                    invoice.Client?.PostalCode,
                                                    invoice.Client?.City,
                                                    invoice.Client?.CountryName))
                                    {
                                        client.Item().PaddingTop(8).Text("ADDRESS").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.Street))
                                            client.Item().Text(invoice.Client.Street);
                                        var clientCityLine = BuildCityLine(invoice.Client?.PostalCode, invoice.Client?.City);
                                        if (!string.IsNullOrWhiteSpace(clientCityLine))
                                            client.Item().Text(clientCityLine);
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.CountryName))
                                            client.Item().Text(invoice.Client.CountryName);
                                    }

                                    if (!IsAllEmpty(invoice.Client?.Email, invoice.Client?.Phone, invoice.Client?.Website))
                                    {
                                        client.Item().PaddingTop(8).Text("CONTACT").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.Email))
                                            client.Item().Text($"Email: {invoice.Client.Email}");
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.Phone))
                                            client.Item().Text($"Phone: {invoice.Client.Phone}");
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.Website))
                                            client.Item().Text($"Website: {invoice.Client.Website}");
                                    }

                                    var hasClientBusinessInfo =
                                        !string.IsNullOrWhiteSpace(invoice.Client?.TaxId) ||
                                        !string.IsNullOrWhiteSpace(invoice.Client?.Siret) ||
                                        !string.IsNullOrWhiteSpace(invoice.Client?.Siren) ||
                                        !string.IsNullOrWhiteSpace(invoice.Client?.CvrNumber);

                                    if (hasClientBusinessInfo)
                                    {
                                        client.Item().PaddingTop(8).Text("BUSINESS INFO").FontSize(9).Bold();
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.TaxId))
                                            client.Item().Text($"Tax ID: {invoice.Client.TaxId}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.Siret))
                                            client.Item().Text($"SIRET: {invoice.Client.Siret}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.Siren))
                                            client.Item().Text($"SIREN: {invoice.Client.Siren}").FontSize(9);
                                        if (!string.IsNullOrWhiteSpace(invoice.Client?.CvrNumber))
                                            client.Item().Text($"CVR: {invoice.Client.CvrNumber}").FontSize(9);
                                    }

                                    client.Item().PaddingTop(8).Text("PAYMENT SETTINGS").FontSize(9).Bold();
                                    client.Item().Text($"Payment Terms: {invoice.Client?.PaymentTermDays ?? 30} days").FontSize(9);
                                    client.Item().Text($"Preferred Currency: {invoice.Client?.PreferredCurrency ?? "EUR"}").FontSize(9);
                                    client.Item().Text($"Priority: {invoice.Client?.Priority.ToString() ?? "Normal"}").FontSize(9);
                                    if (invoice.Client?.DefaultDiscountPercent.HasValue == true)
                                        client.Item().Text($"Default Discount: {invoice.Client.DefaultDiscountPercent:P1}").FontSize(9);
                                });
                            });

                            // ===== INVOICE DETAILS TABLE =====
                            content.Item().PaddingTop(20).Column(invoiceDetails =>
                            {
                                invoiceDetails.Item().Text("INVOICE DETAILS").FontSize(14).Bold();
                                invoiceDetails.Item().PaddingTop(5).LineHorizontal(1);

                                invoiceDetails.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(4);
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(100);
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(110);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("#").Bold().FontSize(9);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Description").Bold().FontSize(9);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignCenter().Text("Qty").Bold().FontSize(9);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text("Unit Price").Bold().FontSize(9);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignCenter().Text("Tax %").Bold().FontSize(9);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text("Total").Bold().FontSize(9);
                                    });

                                    if (invoice.Lines != null && invoice.Lines.Any())
                                    {
                                        int lineNumber = 1;
                                        foreach (var line in invoice.Lines)
                                        {
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(lineNumber.ToString()).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(line.Description ?? "Service").FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{line.Quantity:N2}").FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatMoney(line.UnitPrice, invoice.Currency)).FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{line.TaxRate}").FontSize(9);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatMoney(line.Total, invoice.Currency)).FontSize(9);
                                            lineNumber++;
                                        }
                                    }
                                });
                            });

                            // ===== TOTALS =====
                            content.Item().PaddingTop(16).Row(totalsRow =>
                            {
                                totalsRow.RelativeItem();
                                totalsRow.ConstantItem(320).Table(totalsTable =>
                                {
                                    totalsTable.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(140);
                                    });

                                    totalsTable.Cell().Padding(3).Text("Subtotal:").FontSize(10);
                                    totalsTable.Cell().Padding(3).AlignRight().Text(FormatMoney(invoice.SubTotal, invoice.Currency)).FontSize(10);

                                    if (invoice.DiscountAmount > 0)
                                    {
                                        totalsTable.Cell().Padding(3).Text("Discount:").FontSize(10);
                                        totalsTable.Cell().Padding(3).AlignRight().Text("-" + FormatMoney(invoice.DiscountAmount, invoice.Currency)).FontSize(10);
                                    }

                                    totalsTable.Cell().Padding(3).Text("Tax:").FontSize(10);
                                    totalsTable.Cell().Padding(3).AlignRight().Text(FormatMoney(invoice.TaxAmount, invoice.Currency)).FontSize(10);

                                    totalsTable.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("TOTAL:").Bold().FontSize(11);
                                    totalsTable.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text(FormatMoney(invoice.TotalAmount, invoice.Currency)).Bold().FontSize(11);

                                    if (invoice.PaidAmount > 0)
                                    {
                                        totalsTable.Cell().Padding(3).Text("Paid:").FontSize(10);
                                        totalsTable.Cell().Padding(3).AlignRight().Text(FormatMoney(invoice.PaidAmount, invoice.Currency)).FontSize(10);

                                        var due = invoice.TotalAmount - invoice.PaidAmount;
                                        totalsTable.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("AMOUNT DUE:").Bold().FontSize(11);
                                        totalsTable.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text(FormatMoney(due, invoice.Currency)).Bold().FontSize(11);
                                    }
                                });
                            });

                            // ===== NOTES / TERMS / LEGAL ===== (restored)
                            var hasNotes = !string.IsNullOrWhiteSpace(invoice.Notes);
                            var hasLegal = !string.IsNullOrWhiteSpace(invoice.LegalMentions);
                            var hasPaymentTerms = !string.IsNullOrWhiteSpace(invoice.PaymentTerms);

                            if (hasNotes || hasLegal || hasPaymentTerms)
                            {
                                content.Item().PaddingTop(24).Column(additional =>
                                {
                                    additional.Item().Text("ADDITIONAL INFORMATION").FontSize(14).Bold();
                                    additional.Item().PaddingTop(5).LineHorizontal(1);

                                    if (hasPaymentTerms)
                                    {
                                        additional.Item().PaddingTop(8).Text("PAYMENT TERMS").FontSize(10).Bold();
                                        additional.Item().PaddingTop(4).Text(invoice.PaymentTerms!).FontSize(9);
                                    }

                                    if (hasNotes)
                                    {
                                        additional.Item().PaddingTop(8).Text("NOTES").FontSize(10).Bold();
                                        additional.Item().PaddingTop(4).Text(invoice.Notes!).FontSize(9);
                                    }

                                    if (hasLegal)
                                    {
                                        additional.Item().PaddingTop(8).Text("LEGAL MENTIONS").FontSize(10).Bold();
                                        additional.Item().PaddingTop(4).Text(invoice.LegalMentions!).FontSize(9);
                                    }
                                });
                            }
                        });

                        // ===== FOOTER =====
                        page.Footer().PaddingTop(10).Row(footer =>
                        {
                            footer.RelativeItem()
                                .Text($"Generated on {DateTime.Now:dd/MM/yyyy HH:mm} by AI Invoice Studio")
                                .FontSize(8)
                                .Italic()
                                .FontColor(Colors.Grey.Medium);
                            footer.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Page ");
                                t.CurrentPageNumber();
                                t.Span(" of ");
                                t.TotalPages();
                            });
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
            "InvoiceStudio", "Invoices",
            $"Invoice_{invoice.InvoiceNumber?.Replace("/", "-")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        );

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(outputPath, pdfBytes);

        _logger.Information("PDF invoice saved to {OutputPath}", outputPath);
        return outputPath;
    }

    // ===== Helper Methods =====

    // helper uses IContainer only (no .Item()); callers pass via .Element(...)
    private static void DrawLogo(IContainer c, string? path, float width, float height)
    {
        c.Height(height)
         .Width(width)
         .Border(0.5f)
         .BorderColor(Colors.Grey.Lighten3)
         .Padding(2)
         .AlignMiddle()
         .AlignCenter()
         .Element(box =>
         {
             if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
             {
                 try
                 {
                     box.Image(path, ImageScaling.FitArea);
                 }
                 catch
                 {
                     box.Text("[LOGO]").FontSize(9);
                 }
             }
             else
             {
                 box.Text("[LOGO]").FontSize(9);
             }
         });
    }

    private static string BuildCityLine(string? postalCode, string? city)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(postalCode)) parts.Add(postalCode!.Trim());
        if (!string.IsNullOrWhiteSpace(city)) parts.Add(city!.Trim());
        return string.Join(" ", parts);
    }

    private static bool IsAllEmpty(params string?[] values) =>
        values.All(v => string.IsNullOrWhiteSpace(v));

    private static string FormatIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return "";
        var cleaned = iban.Replace(" ", "");
        var chunks = new List<string>();
        for (int i = 0; i < cleaned.Length; i += 4)
            chunks.Add(cleaned.Substring(i, Math.Min(4, cleaned.Length - i)));
        return string.Join(" ", chunks);
    }

    private static string FormatMoney(decimal amount, string? currencyCode)
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        nfi.NumberDecimalDigits = 2;
        return $"{amount.ToString("N", nfi)} {(string.IsNullOrWhiteSpace(currencyCode) ? "" : currencyCode)}".TrimEnd();
    }

    private static string GetStatusText(InvoiceStatus status) =>
        status switch
        {
            InvoiceStatus.Draft => "DRAFT",
            InvoiceStatus.Approved => "APPROVED",
            InvoiceStatus.Issued => "ISSUED",
            InvoiceStatus.Sent => "SENT",
            InvoiceStatus.Paid => "PAID",
            InvoiceStatus.Overdue => "OVERDUE",
            InvoiceStatus.Cancelled => "CANCELLED",
            _ => "UNKNOWN"
        };
}
