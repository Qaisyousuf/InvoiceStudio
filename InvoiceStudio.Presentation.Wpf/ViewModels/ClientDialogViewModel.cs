using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;
using static Azure.Core.HttpHeader;

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
    private string? _website;

    [ObservableProperty]
    private string? _street;

    [ObservableProperty]
    private string? _city;

    [ObservableProperty]
    private string? _postalCode;

    [ObservableProperty]
    private string? _countryName;

    [ObservableProperty]
    private string? _taxId;

    [ObservableProperty]
    private string? _vatNumber;

    [ObservableProperty]
    private string _preferredCurrency = "EUR";

    [ObservableProperty]
    private int _paymentTermDays = 30;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private int _selectedTypeIndex = 0; // 0=Individual, 1=Company


    [ObservableProperty]
    private int _selectedCountryIndex = 0; // 0=International, 1=France, 2=Denmark

    [ObservableProperty]
    private string? _siret; // French SIRET number

    [ObservableProperty]
    private string? _intraCommunityVat; // French intra-community VAT

    [ObservableProperty]
    private string? _cvrNumber; // Danish CVR number

    [ObservableProperty]
    private string? _danishVatNumber; // Danish VAT number

    // Helper properties for UI visibility
    public bool ShowFrenchFields => SelectedCountryIndex == 1;
    public bool ShowDanishFields => SelectedCountryIndex == 2;
    public bool ShowGenericFields => SelectedCountryIndex == 0;

    // Update the property changed notification
    partial void OnSelectedCountryIndexChanged(int value)
    {
        OnPropertyChanged(nameof(ShowFrenchFields));
        OnPropertyChanged(nameof(ShowDanishFields));
        OnPropertyChanged(nameof(ShowGenericFields));
    }

    public bool IsEditMode => _existingClient != null;

    private ClientType SelectedType
    {
        get => (ClientType)_selectedTypeIndex;
        set => SelectedTypeIndex = (int)value;
    }

    public ClientDialogViewModel(IClientRepository clientRepository, ILogger logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        Title = "New Client";
    }

    public void LoadClient(Client client)
    {
        _existingClient = client;
        Title = "Edit Client";

        // Basic Information
        Name = client.Name;
        LegalName = client.LegalName;
        Email = client.Email;
        Phone = client.Phone;
        Website = client.Website;

        // Address Information  
        Street = client.Street;
        City = client.City;
        PostalCode = client.PostalCode;
        CountryName = client.CountryName;

        // Business Information
        TaxId = client.TaxId;
        VatNumber = client.VatNumber;
        PreferredCurrency = client.PreferredCurrency;
        PaymentTermDays = client.PaymentTermDays;

        // Status and Notes
        IsActive = client.IsActive;
        Notes = client.Notes;
        SelectedTypeIndex = (int)client.Type;

       

        // Country-specific business information
        if (!string.IsNullOrEmpty(client.Siret))
        {
            // French client
            SelectedCountryIndex = 1;
            Siret = client.Siret;
            IntraCommunityVat = client.IntraCommunityVatFr;
        }
        else if (!string.IsNullOrEmpty(client.CvrNumber))
        {
            // Danish client  
            SelectedCountryIndex = 2;
            CvrNumber = client.CvrNumber;
            DanishVatNumber = client.DanishVatNumber;
        }
        else
        {
            // International client
            SelectedCountryIndex = 0;
        }
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

            _logger.Information("=== SAVE CLIENT DEBUG ===");
            _logger.Information("SelectedCountryIndex: {Index}", SelectedCountryIndex);
            _logger.Information("Siret: '{Siret}'", Siret ?? "NULL");
            _logger.Information("CvrNumber: '{CvrNumber}'", CvrNumber ?? "NULL");

            IsBusy = true;

            if (IsEditMode && _existingClient != null)
            {
                _logger.Information("EDIT MODE - Updating existing client");
                // ... existing update code ...
            }
            else
            {
                _logger.Information("CREATE MODE - Creating new client");

                // Create new client
                var newClient = new Client(Name, Email, SelectedType);
                newClient.UpdateDetails(Name, LegalName, TaxId, VatNumber);
                newClient.UpdateContact(Email, Phone, Website);
                newClient.UpdateAddress(Street, City, PostalCode, CountryName);
                newClient.UpdateBusinessSettings(PreferredCurrency, PaymentTermDays);
                newClient.UpdateNotes(Notes);

                _logger.Information("About to handle country-specific info - SelectedCountryIndex: {Index}", SelectedCountryIndex);

                // Handle country-specific business info
                switch (SelectedCountryIndex)
                {
                    case 1: // France
                        _logger.Information("CASE 1 - FRANCE: Siret='{Siret}', IntraVat='{IntraVat}'",
                                           Siret ?? "NULL", IntraCommunityVat ?? "NULL");
                        if (!string.IsNullOrEmpty(Siret))
                        {
                            _logger.Information("Calling UpdateFrenchBusinessInfo...");
                            newClient.UpdateFrenchBusinessInfo(Siret, IntraCommunityVat);
                            _logger.Information("UpdateFrenchBusinessInfo completed");
                        }
                        else
                        {
                            _logger.Warning("SIRET is empty, skipping French business info");
                        }
                        break;

                    case 2: // Denmark
                        _logger.Information("CASE 2 - DENMARK: CVR='{CVR}', DanishVat='{DanishVat}'",
                                           CvrNumber ?? "NULL", DanishVatNumber ?? "NULL");
                        if (!string.IsNullOrEmpty(CvrNumber))
                        {
                            _logger.Information("Calling UpdateDanishBusinessInfo...");
                            newClient.UpdateDanishBusinessInfo(CvrNumber, DanishVatNumber);
                            _logger.Information("UpdateDanishBusinessInfo completed");
                        }
                        else
                        {
                            _logger.Warning("CVR is empty, skipping Danish business info");
                        }
                        break;

                    default:
                        _logger.Information("CASE DEFAULT - INTERNATIONAL (index: {Index})", SelectedCountryIndex);
                        break;
                }

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