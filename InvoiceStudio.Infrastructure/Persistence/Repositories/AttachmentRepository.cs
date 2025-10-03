using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceStudio.Infrastructure.Persistence.Repositories;

public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(InvoiceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Attachment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.InvoiceId == invoiceId)
            .OrderBy(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Attachment>> GetByTypeAsync(AttachmentType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Type == type)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Attachment?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.FilePath == filePath, cancellationToken);
    }

    public override async Task<IReadOnlyList<Attachment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}