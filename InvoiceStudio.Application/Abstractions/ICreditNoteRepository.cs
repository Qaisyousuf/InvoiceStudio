using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface ICreditNoteRepository : IRepository<CreditNote>
{
    Task<CreditNote?> GetByCreditNoteNumberAsync(string creditNoteNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditNote>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditNote>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditNote>> GetByStatusAsync(CreditNoteStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditNote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}