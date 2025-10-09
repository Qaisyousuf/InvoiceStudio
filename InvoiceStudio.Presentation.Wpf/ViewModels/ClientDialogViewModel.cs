using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ClientDialogViewModel : ViewModelBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger _logger;
    private Client? _existingClient;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _legalName;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private string? _street;

    [ObservableProperty]
    private string? _city;

    [ObservableProperty]
    private string? _postalCode;

    [ObservableProperty]
    private int _selectedCountryIndex = 0; // 0=France, 1=Denmark

    [ObservableProperty]
    private string _preferredCurrency = "EUR";

    [ObservableProperty]
    private int _paymentTermDays = 30;

    [ObservableProperty]
    private int _selectedTypeIndex = 0;

    [ObservableProperty]
    private bool _isActive = true;

    public bool IsEditMode => _existingClient != null;

    private ClientType SelectedType
    {
        get => (ClientType)_selectedTypeIndex;
        set => SelectedTypeIndex = (int)value;
    }

    private string Country => _selectedCountryIndex == 0 ? "FR" : "DK";

    public ClientDialogViewModel(IClientRepository clientRepository, ILogger logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        Title = "New Client";

        // Set defaults for France
        ApplyCountryDefaults();
    }

    partial void OnSelectedCountryIndexChanged(int value)
    {
        ApplyCountryDefaults();
    }

    private void ApplyCountryDefaults()
    {
        if (_selectedCountryIndex == 0) // France
        {
            PreferredCurrency = "EUR";
            PaymentTermDays = 30;
        }
        else if (_selectedCountryIndex == 1) // Denmark
        {
            PreferredCurrency = "DKK";
            PaymentTermDays = 30;
        }
    }

    public void LoadClient(Client client)
    {
        _existingClient = client;
        Title = "Edit Client";

        Name = client.Name;
        LegalName = client.LegalName;
        Email = client.Email;
        Phone = client.Phone;
        Street = client.Street;
        City = client.City;
        PostalCode = client.PostalCode;

        // Set country index based on stored country code
        SelectedCountryIndex = client.Country == "DK" ? 1 : 0;

        PreferredCurrency = client.PreferredCurrency;
        PaymentTermDays = client.PaymentTermDays;
        SelectedTypeIndex = (int)client.Type;
        IsActive = client.IsActive;
    }

    [RelayCommand]
    public async Task<bool> SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _logger.Warning("Client name is required");
                return false;
            }

            IsBusy = true;

            if (IsEditMode && _existingClient != null)
            {
                // Update existing client
                _existingClient.UpdateDetails(Name, LegalName, null, null);
                _existingClient.UpdateContact(Email, Phone, null);
                _existingClient.UpdateAddress(Street, City, PostalCode, Country);
                _existingClient.UpdateBusinessSettings(PreferredCurrency, PaymentTermDays);
                _existingClient.SetType(SelectedType);

                if (IsActive)
                    _existingClient.Activate();
                else
                    _existingClient.Deactivate();

                await _clientRepository.UpdateAsync(_existingClient);
                await _clientRepository.SaveChangesAsync();

                _logger.Information("Client {Name} updated successfully", Name);
            }
            else
            {
                // Create new client
                var newClient = new Client(Name, Email, SelectedType);
                newClient.UpdateDetails(Name, LegalName, null, null);
                newClient.UpdateContact(Email, Phone, null);
                newClient.UpdateAddress(Street, City, PostalCode, Country);
                newClient.UpdateBusinessSettings(PreferredCurrency, PaymentTermDays);

                await _clientRepository.AddAsync(newClient);
                await _clientRepository.SaveChangesAsync();

                _logger.Information("Client {Name} created successfully", Name);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving client");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}