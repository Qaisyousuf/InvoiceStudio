using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class TaxRepository : Repository<Tax>, ITaxRepository
{
    public TaxRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<Tax?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Tax>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Country == country)
            .OrderBy(t => t.Rate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tax>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.Country)
            .ThenBy(t => t.Rate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Tax?> GetDefaultByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Country == country && t.IsDefault && t.IsActive, cancellationToken);
    }

    public override async Task<IReadOnlyList<Tax>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(t => t.Country)
            .ThenBy(t => t.Rate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}