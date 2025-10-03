using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}