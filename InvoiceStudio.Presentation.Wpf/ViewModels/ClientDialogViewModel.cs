using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using Microsoft.Win32;
using Serilog;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ClientDialogViewModel : ObservableValidator
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger _logger;
    private Client? _existingClient;

    #region Base Properties
    [ObservableProperty]
    private string _title = "Client Details";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string? _errorMessage;
    #endregion

    #region Wizard Navigation Properties
    [ObservableProperty]
    private int _currentStep = 1;

    private int _totalSteps = 4; // Reduced from 6 to 4

    public bool IsFirstStep => CurrentStep == 1;
    public bool IsLastStep => CurrentStep == _totalSteps;
    public bool CanGoNext => CurrentStep < _totalSteps && ValidateCurrentStep();
    public bool CanGoPrevious => CurrentStep > 1;

    // Step visibility properties
    public bool IsStep1Visible => CurrentStep == 1;
    public bool IsStep2Visible => CurrentStep == 2;
    public bool IsStep3Visible => CurrentStep == 3;
    public bool IsStep4Visible => CurrentStep == 4;

    public string StepTitle => CurrentStep switch
    {
        1 => "Basic Information",
        2 => "Address Details",
        3 => "Tax & Business",
        4 => "Logo & Notes",
        _ => "Unknown Step"
    };

    public string ProgressText => $"Step {CurrentStep} of {_totalSteps}";
    public double ProgressPercentage => (double)CurrentStep / _totalSteps * 100;
    #endregion

    #region Basic Information
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
    private string? _internalReference;

    [ObservableProperty]
    private ClientType _clientType = ClientType.Individual;

    [ObservableProperty]
    private string? _country;

    [ObservableProperty]
    private ClientPriority _priority = ClientPriority.Normal;

    [ObservableProperty]
    private bool _isActive = true;
    #endregion

    #region Primary Address
    [ObservableProperty]
    private string? _street;

    [ObservableProperty]
    private string? _city;

    [ObservableProperty]
    private string? _postalCode;

    [ObservableProperty]
    private string? _countryName;
    #endregion

    #region Business Information
    [ObservableProperty]
    private string? _taxId;

    [ObservableProperty]
    private string _preferredCurrency = "EUR";

    [ObservableProperty]
    private int _paymentTermDays = 30;

    [ObservableProperty]
    private decimal? _defaultDiscountPercent;

    [ObservableProperty]
    private string? _notes;
    #endregion

    #region Country-Specific Fields
    [ObservableProperty]
    private string? _siret;

    [ObservableProperty]
    private string? _siren; // Added SIREN field

    [ObservableProperty]
    private string? _cvrNumber;

    [ObservableProperty]
    private string? _danishVatNumber;
    #endregion

    #region Client Management
    [ObservableProperty]
    private string? _category;
    #endregion

    #region Statistics (Read-Only)
    [ObservableProperty]
    private DateTime? _firstInvoiceDate;

    [ObservableProperty]
    private DateTime? _lastInvoiceDate;

    [ObservableProperty]
    private int _totalInvoices = 0;

    [ObservableProperty]
    private decimal _totalRevenue = 0;

    [ObservableProperty]
    private decimal _overdueAmount = 0;

    [ObservableProperty]
    private double _averagePaymentDays = 0;
    #endregion

    #region Logo Management
    [ObservableProperty]
    private string? _logoPath;

    [ObservableProperty]
    private bool _hasLogo = false;
    #endregion

    #region Collections for ComboBoxes
    public IEnumerable<ClientType> ClientTypes => Enum.GetValues<ClientType>();
    public IEnumerable<ClientPriority> ClientPriorities => Enum.GetValues<ClientPriority>();
    public IEnumerable<string> Currencies => new[] { "EUR", "USD", "GBP", "DKK", "CHF" };
    public IEnumerable<string> Countries => new[] { "France", "Denmark", "Germany", "United States", "United Kingdom", "Spain", "Italy" };
    public IEnumerable<string> Categories => new[] { "VIP", "Regular", "Prospect", "Partner", "Lead", "Inactive" };
    #endregion

    #region UI Properties
    public bool IsEditMode => _existingClient != null;
    public string DialogTitle => IsEditMode ? $"Edit Client - {Name}" : "Create New Client";
    #endregion

    public ClientDialogViewModel(IClientRepository clientRepository, ILogger logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        Title = "Client Details";
        PropertyChanged += OnPropertyChanged;
    }

    #region Property Change Handling
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)) return;

        // Auto-generate Danish VAT from CVR
        if (e.PropertyName == nameof(CvrNumber) && !string.IsNullOrEmpty(CvrNumber))
        {
            DanishVatNumber = $"DK{CvrNumber}";
        }

        // Update navigation properties when step changes
        if (e.PropertyName == nameof(CurrentStep))
        {
            OnPropertyChanged(nameof(IsFirstStep));
            OnPropertyChanged(nameof(IsLastStep));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(StepTitle));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(IsStep1Visible));
            OnPropertyChanged(nameof(IsStep2Visible));
            OnPropertyChanged(nameof(IsStep3Visible));
            OnPropertyChanged(nameof(IsStep4Visible));
        }

        // Clear errors
        if (HasErrors && !string.IsNullOrEmpty(e.PropertyName))
        {
            HasErrors = false;
            ErrorMessage = null;
        }
    }
    #endregion

    #region Step Validation
    private bool ValidateCurrentStep()
    {
        return CurrentStep switch
        {
            1 => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Country),
            2 => !string.IsNullOrWhiteSpace(Street) && !string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(PostalCode),
            3 => !string.IsNullOrWhiteSpace(PreferredCurrency) && PaymentTermDays > 0,
            4 => true, // Optional step
            _ => false
        };
    }

    private string GetStepValidationMessage()
    {
        return CurrentStep switch
        {
            1 => "Please fill in Name, Email, and Country",
            2 => "Please fill in Street, City, and Postal Code",
            3 => "Please select Currency and enter Payment Terms",
            4 => "",
            _ => "Please complete required fields"
        };
    }
    #endregion

    #region Navigation Commands
    [RelayCommand]
    private void NextStep()
    {
        if (!ValidateCurrentStep())
        {
            ErrorMessage = GetStepValidationMessage();
            HasErrors = true;
            return;
        }

        if (CurrentStep < _totalSteps)
        {
            CurrentStep++;
            HasErrors = false;
            ErrorMessage = null;
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            HasErrors = false;
            ErrorMessage = null;
        }
    }

    [RelayCommand]
    private async Task SaveAndNextAsync()
    {
        if (!ValidateCurrentStep())
        {
            ErrorMessage = GetStepValidationMessage();
            HasErrors = true;
            return;
        }

        bool saved = await SaveAsync();
        if (saved && CurrentStep < _totalSteps)
        {
            CurrentStep++;
        }
    }
    #endregion

    #region Load Client Data
    public void LoadClient(Client client)
    {
        _existingClient = client;
        Title = "Edit Client";

        Name = client.Name;
        LegalName = client.LegalName;
        Email = client.Email;
        Phone = client.Phone;
        Website = client.Website;
        ClientType = client.Type;
        Country = client.Country;
        InternalReference = client.InternalReference;
        Priority = client.Priority;
        IsActive = client.IsActive;

        Street = client.Street;
        City = client.City;
        PostalCode = client.PostalCode;
        CountryName = client.CountryName;

        TaxId = client.TaxId;
        PreferredCurrency = client.PreferredCurrency;
        PaymentTermDays = client.PaymentTermDays;
        DefaultDiscountPercent = client.DefaultDiscountPercent;
        Notes = client.Notes;

        Siret = client.Siret;
        Siren = client.Siren; // Load SIREN
        CvrNumber = client.CvrNumber;
        DanishVatNumber = client.DanishVatNumber;

        Category = client.Category;

        FirstInvoiceDate = client.FirstInvoiceDate;
        LastInvoiceDate = client.LastInvoiceDate;
        TotalInvoices = client.TotalInvoices;
        TotalRevenue = client.TotalRevenue;
        OverdueAmount = client.OverdueAmount;
        AveragePaymentDays = client.AveragePaymentDays;

        LogoPath = client.LogoPath;
        HasLogo = !string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath);

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(DialogTitle));
        _logger.Information("Client loaded: {ClientName}", client.Name);
    }
    #endregion

    #region Save Client
    public async Task<bool> SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Client name is required.";
                HasErrors = true;
                return false;
            }

            IsBusy = true;
            IsLoading = true;
            ErrorMessage = null;
            HasErrors = false;

            _logger.Information("Saving client: {ClientName}", Name);

            if (IsEditMode && _existingClient != null)
            {
                await UpdateExistingClient(_existingClient);
            }
            else
            {
                await CreateNewClient();
            }

            await _clientRepository.SaveChangesAsync();
            _logger.Information("Client saved successfully: {ClientName}", Name);

            MessageBox.Show(
                $"Client '{Name}' has been saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving client: {ClientName}", Name);
            ErrorMessage = $"Error saving client: {ex.Message}";
            HasErrors = true;

            MessageBox.Show(
                $"Failed to save client: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return false;
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
        }
    }

    private async Task UpdateExistingClient(Client client)
    {
        client.UpdateDetails(Name, LegalName, TaxId, Siren); // Use SIREN instead of VatNumber
        client.UpdateContact(Email, Phone, Website);
        client.UpdatePrimaryAddress(Street, City, PostalCode, CountryName ?? Country);
        client.UpdateBusinessSettings(PreferredCurrency, PaymentTermDays, DefaultDiscountPercent);
        client.SetType(ClientType);

        // Set Country property
        if (!string.IsNullOrEmpty(Country))
        {
            try
            {
                var clientType = client.GetType();
                var countryProperty = clientType.GetProperty("Country");
                if (countryProperty != null && countryProperty.CanWrite)
                {
                    countryProperty.SetValue(client, Country);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("Could not set Country property: {Error}", ex.Message);
            }
        }

        // Country-specific updates
        if (ClientType == ClientType.Company)
        {
            if (Country == "France" && !string.IsNullOrEmpty(Siret))
            {
                client.UpdateFrenchBusinessInfo(Siret, Siren); // Use SIREN instead of IntraCommunityVat
            }
            else if (Country == "Denmark" && !string.IsNullOrEmpty(CvrNumber))
            {
                client.UpdateDanishBusinessInfo(CvrNumber, DanishVatNumber);
            }
        }

        // Try to update category if method exists
        try
        {
            client.UpdateClientManagement(Category, Priority);
        }
        catch (Exception ex)
        {
            _logger.Information("Client management properties not available: {Error}", ex.Message);
        }

        if (IsActive)
            client.Activate();
        else
            client.Deactivate();

        if (!string.IsNullOrEmpty(LogoPath))
            client.UpdateLogo(LogoPath);

        if (!string.IsNullOrEmpty(Notes))
            client.UpdateNotes(Notes);

        if (!string.IsNullOrEmpty(InternalReference))
            client.UpdateInternalReference(InternalReference);
    }

    private async Task CreateNewClient()
    {
        var client = new Client(Name, Email, ClientType);
        await UpdateExistingClient(client);
        await _clientRepository.AddAsync(client);
        _existingClient = client;
    }
    #endregion

    #region Commands
    [RelayCommand]
    public async Task SelectLogoAsync()
    {
        try
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Select Client Logo",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                string logosDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logos", "Clients");
                Directory.CreateDirectory(logosDirectory);

                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(selectedFile)}";
                string destinationPath = Path.Combine(logosDirectory, fileName);

                File.Copy(selectedFile, destinationPath, true);

                LogoPath = destinationPath;
                HasLogo = true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to select logo");
            ErrorMessage = "Failed to select logo.";
            HasErrors = true;
        }
    }

    [RelayCommand]
    public void RemoveLogo()
    {
        LogoPath = null;
        HasLogo = false;
    }
    #endregion
}