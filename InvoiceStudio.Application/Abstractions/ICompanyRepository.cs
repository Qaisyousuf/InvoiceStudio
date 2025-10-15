using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<Company?> GetBySiretAsync(string siret, CancellationToken cancellationToken = default);
    Task<Company?> GetByCvrNumberAsync(string cvrNumber, CancellationToken cancellationToken = default);
    Task<Company?> GetFirstAsync(CancellationToken cancellationToken = default);
    Task<Company?> GetFirstForUpdateAsync(CancellationToken cancellationToken = default); // ← ADD THIS LINE
}