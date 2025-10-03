using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class CreditNoteRepository : Repository<CreditNote>, ICreditNoteRepository
{
    public CreditNoteRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<CreditNote?> GetByCreditNoteNumberAsync(string creditNoteNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Invoice)
            .Include(c => c.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CreditNoteNumber == creditNoteNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<CreditNote>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Client)
            .Where(c => c.InvoiceId == invoiceId)
            .OrderByDescending(c => c.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CreditNote>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Invoice)
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CreditNote>> GetByStatusAsync(CreditNoteStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Invoice)
            .Include(c => c.Client)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CreditNote>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Invoice)
            .Include(c => c.Client)
            .Where(c => c.IssueDate >= startDate && c.IssueDate <= endDate)
            .OrderByDescending(c => c.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<CreditNote>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Invoice)
            .Include(c => c.Client)
            .OrderByDescending(c => c.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}