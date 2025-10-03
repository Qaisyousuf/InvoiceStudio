using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPaymentsByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}