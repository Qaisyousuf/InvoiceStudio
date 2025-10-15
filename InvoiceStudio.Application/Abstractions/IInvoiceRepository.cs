using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteInvoiceLinesAsync(Guid invoiceId);
    Task<bool> UpdateInvoiceBasicAsync(Guid invoiceId, string? notes, string? terms);
}