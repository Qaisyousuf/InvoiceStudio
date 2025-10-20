using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Serilog;
using System.Collections.ObjectModel;


namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class EditInvoiceViewModel : ObservableObject
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger _logger;

    private Guid _invoiceId;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _invoiceNumber = string.Empty;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _terms;

    [ObservableProperty]
    private Client? _selectedClient;

    public ObservableCollection<Client> AvailableClients { get; } = new();
    public ObservableCollection<EditLineItem> Lines { get; } = new();

    public EditInvoiceViewModel(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task LoadInvoiceAsync(Guid invoiceId)
    {
        try
        {
            IsBusy = true;
            _invoiceId = invoiceId;

            // Load clients
            var clients = await _clientRepository.GetActiveClientsAsync();
            AvailableClients.Clear();
            foreach (var client in clients)
            {
                AvailableClients.Add(client);
            }

            // Load invoice
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
            if (invoice == null) return;

            InvoiceNumber = invoice.InvoiceNumber;
            Notes = invoice.Notes;
            Terms = invoice.Terms;
            SelectedClient = AvailableClients.FirstOrDefault(c => c.Id == invoice.ClientId);

            // Load lines
            Lines.Clear();
            foreach (var line in invoice.Lines.OrderBy(l => l.LineOrder))
            {
                Lines.Add(new EditLineItem
                {
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxRate = line.TaxRate
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading invoice for edit");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddLine()
    {
        Lines.Add(new EditLineItem());
    }

    [RelayCommand]
    private void RemoveLine(EditLineItem line)
    {
        Lines.Remove(line);
    }

    [RelayCommand]
    public async Task<bool> SaveAsync()
    {
        try
        {
            IsBusy = true;

            // Use raw SQL to avoid EF tracking issues
            using var connection = new Microsoft.Data.SqlClient.SqlConnection("Server=SEO-PC;Database=InvoiceStudioDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");
            await connection.OpenAsync();

            // Update basic invoice info
            var updateInvoiceSql = "UPDATE Invoices SET Notes = @notes, Terms = @terms WHERE Id = @id";
            using var updateCmd = new Microsoft.Data.SqlClient.SqlCommand(updateInvoiceSql, connection);
            updateCmd.Parameters.AddWithValue("@notes", Notes ?? (object)DBNull.Value);
            updateCmd.Parameters.AddWithValue("@terms", Terms ?? (object)DBNull.Value);
            updateCmd.Parameters.AddWithValue("@id", _invoiceId);
            await updateCmd.ExecuteNonQueryAsync();

            // Delete existing lines
            var deleteSql = "DELETE FROM InvoiceLines WHERE InvoiceId = @invoiceId";
            using var deleteCmd = new Microsoft.Data.SqlClient.SqlCommand(deleteSql, connection);
            deleteCmd.Parameters.AddWithValue("@invoiceId", _invoiceId);
            await deleteCmd.ExecuteNonQueryAsync();

            // Insert new lines
            var insertSql = @"INSERT INTO InvoiceLines (Id, InvoiceId, Description, Quantity, UnitPrice, TaxRate, SubTotal, TaxAmount, Total, Unit, LineOrder, CreatedAt)
                              VALUES (@id, @invoiceId, @description, @quantity, @unitPrice, @taxRate, @subTotal, @taxAmount, @total, @unit, @lineOrder, @createdAt)";

            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                using var insertCmd = new Microsoft.Data.SqlClient.SqlCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                insertCmd.Parameters.AddWithValue("@invoiceId", _invoiceId);
                insertCmd.Parameters.AddWithValue("@description", line.Description);
                insertCmd.Parameters.AddWithValue("@quantity", line.Quantity);
                insertCmd.Parameters.AddWithValue("@unitPrice", line.UnitPrice);
                insertCmd.Parameters.AddWithValue("@taxRate", line.TaxRate);
                insertCmd.Parameters.AddWithValue("@subTotal", line.Quantity * line.UnitPrice);
                insertCmd.Parameters.AddWithValue("@taxAmount", (line.Quantity * line.UnitPrice) * line.TaxRate);
                insertCmd.Parameters.AddWithValue("@total", (line.Quantity * line.UnitPrice) * (1 + line.TaxRate));
                insertCmd.Parameters.AddWithValue("@unit", "pcs");
                insertCmd.Parameters.AddWithValue("@lineOrder", i + 1);
                insertCmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                await insertCmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving invoice");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

// Simple line item class
public partial class EditLineItem : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _taxRate = 0.20m;
}