using InvoiceStudio.Domain.Entities;

namespace InvoiceStudio.Application.Abstractions;

public interface IAttachmentRepository : IRepository<Attachment>
{
    Task<IReadOnlyList<Attachment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attachment>> GetByTypeAsync(AttachmentType type, CancellationToken cancellationToken = default);
    Task<Attachment?> GetByFilePathAsync(string filePath, CancellationToken cancellationToken = default);
}