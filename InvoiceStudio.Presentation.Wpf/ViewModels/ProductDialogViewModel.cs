using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ProductDialogViewModel : ViewModelBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;
    private Product? _existingProduct;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _sku;

    [ObservableProperty]
    private decimal _unitPrice = 0;

    [ObservableProperty]
    private string _currency = "EUR";

    [ObservableProperty]
    private decimal _taxRate = 20.0m;

    [ObservableProperty]
    private int _selectedTypeIndex = 0; // 0=Service, 1=Product, 2=Subscription

    [ObservableProperty]
    private string _unit = "hours";

    [ObservableProperty]
    private bool _isActive = true;

    public bool IsEditMode => _existingProduct != null;

    private ProductType SelectedType
    {
        get => (ProductType)_selectedTypeIndex;
        set => SelectedTypeIndex = (int)value;
    }

    public ProductDialogViewModel(IProductRepository productRepository, ILogger logger)
    {
        _productRepository = productRepository;
        _logger = logger;
        Title = "New Product";
    }

    public void LoadProduct(Product product)
    {
        _existingProduct = product;
        Title = "Edit Product";

        Name = product.Name;
        Description = product.Description;
        Sku = product.Sku;
        UnitPrice = product.UnitPrice;
        Currency = product.Currency;
        TaxRate = product.TaxRate;
        SelectedTypeIndex = (int)product.Type;
        Unit = product.Unit;
        IsActive = product.IsActive;
    }

    [RelayCommand]
    public async Task<bool> SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _logger.Warning("Product name is required");
                return false;
            }

            IsBusy = true;

            if (IsEditMode && _existingProduct != null)
            {
                // Update existing product
                _existingProduct.UpdateDetails(Name, Description, Sku);
                _existingProduct.UpdatePricing(UnitPrice, Currency, TaxRate);
                _existingProduct.SetType(SelectedType);
                _existingProduct.SetUnit(Unit);

                if (IsActive)
                    _existingProduct.Activate();
                else
                    _existingProduct.Deactivate();

                await _productRepository.UpdateAsync(_existingProduct);
                await _productRepository.SaveChangesAsync();

                _logger.Information("Product {Name} updated successfully", Name);
            }
            else
            {
                // Create new product
                var newProduct = new Product(Name, UnitPrice, Currency);
                newProduct.UpdateDetails(Name, Description, Sku);
                newProduct.UpdatePricing(UnitPrice, Currency, TaxRate);
                newProduct.SetType(SelectedType);
                newProduct.SetUnit(Unit);

                await _productRepository.AddAsync(newProduct);
                await _productRepository.SaveChangesAsync();

                _logger.Information("Product {Name} created successfully", Name);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving product");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}