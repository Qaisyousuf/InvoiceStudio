using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCurrencyAsync(string currency, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Currency == currency && p.IsActive)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(searchTerm) && p.IsActive)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            // Test if we can connect and count records
            var count = await _context.Database.CanConnectAsync();
            return count;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _dbSet.CountAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Database query failed: {ex.Message}", ex);
        }
    }
}