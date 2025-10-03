using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Client?> GetBySiretAsync(string siret, CancellationToken cancellationToken = default);
    Task<Client?> GetByCvrNumberAsync(string cvrNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetByTypeAsync(ClientType type, CancellationToken cancellationToken = default);
}