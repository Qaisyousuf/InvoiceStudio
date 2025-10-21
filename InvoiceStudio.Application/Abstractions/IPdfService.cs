using InvoiceStudio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceStudio.Application.Abstractions
{
    public interface IPdfService
    {
        Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);
        Task<string> GenerateInvoicePdfFileAsync(Invoice invoice, string? outputPath = null);
    }
}
