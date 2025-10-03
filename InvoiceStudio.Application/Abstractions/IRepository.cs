namespace InvoiceStudio.Application.Abstractions;

public interface IRepository<T> where T : class
{
    // Read operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    // Write operations
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    // Persistence
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}