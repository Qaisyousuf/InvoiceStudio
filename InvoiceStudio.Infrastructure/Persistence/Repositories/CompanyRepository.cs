using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<Company?> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Country == country, cancellationToken);
    }

    public async Task<Company?> GetBySiretAsync(string siret, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Siret == siret, cancellationToken);
    }

    public async Task<Company?> GetByCvrNumberAsync(string cvrNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CvrNumber == cvrNumber, cancellationToken);
    }

    public async Task<Company?> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Company?> GetFirstForUpdateAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cancellationToken); // No AsNoTracking = tracked entity
    }
}