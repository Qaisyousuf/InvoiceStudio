using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.InvoiceId == invoiceId)
            .OrderBy(p => p.PaymentDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Method == method)
            .OrderByDescending(p => p.PaymentDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .OrderByDescending(p => p.PaymentDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPaymentsByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.InvoiceId == invoiceId)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public override async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(p => p.PaymentDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}