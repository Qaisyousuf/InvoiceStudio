using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }

    public AttachmentType Type { get; private set; }
    public string? Description { get; private set; }

    private Attachment() { } // EF Core

    public Attachment(Guid invoiceId, string fileName, string filePath, string contentType, long fileSize, AttachmentType type)
    {
        InvoiceId = invoiceId;
        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        Type = type;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        MarkAsUpdated();
    }
}

public enum AttachmentType
{
    Receipt = 0,
    PurchaseOrder = 1,
    Contract = 2,
    Timesheet = 3,
    Other = 99
}