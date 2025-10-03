using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface ITaxRepository : IRepository<Tax>
{
    Task<Tax?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tax>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tax>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Tax?> GetDefaultByCountryAsync(string country, CancellationToken cancellationToken = default);
}