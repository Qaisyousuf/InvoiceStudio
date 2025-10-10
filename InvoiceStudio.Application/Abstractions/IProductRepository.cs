using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCurrencyAsync(string currency, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<bool> TestConnectionAsync();
    Task<int> GetCountAsync();
}