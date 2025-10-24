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
            .Include(i => i.Company)                 // <-- ensure Company is loaded
            .Include(i => i.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Company)                 // <-- include Company
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Company)                 // <-- include Company
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Company)                 // <-- include Company
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Company)                 // <-- include Company
            .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate)
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Company)                 // <-- include Company (bank details, address, etc.)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Client)
            .Include(i => i.Company)                 // <-- include Company
            .OrderByDescending(i => i.IssueDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteInvoiceLinesAsync(Guid invoiceId)
    {
        var lines = await _dbSet
            .Where(i => i.Id == invoiceId)
            .SelectMany(i => i.Lines)
            .ToListAsync();

        foreach (var line in lines)
        {
            _context.InvoiceLines.Remove(line);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateInvoiceBasicAsync(Guid invoiceId, string? notes, string? terms)
    {
        var invoice = await _dbSet.FindAsync(invoiceId);
        if (invoice == null) return false;

        invoice.UpdateNotes(notes, terms);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Invoice> CreateInvoiceWithCompanyAsync(string invoiceNumber, Guid clientId, DateTime issueDate, DateTime dueDate, string currency = "EUR", CancellationToken cancellationToken = default)
    {
        // Get the first (default) company
        var company = await _context.Companies.FirstAsync(cancellationToken);

        // Create invoice with company reference
        var invoice = new Invoice(invoiceNumber, clientId, company.Id, issueDate, dueDate, currency);

        // Optional: attach navigation (helps in-memory use before reloading)
        invoice = _context.Attach(invoice).Entity;
        // If you want to save immediately, uncomment:
        // await _context.SaveChangesAsync(cancellationToken);

        return invoice;
    }
}
