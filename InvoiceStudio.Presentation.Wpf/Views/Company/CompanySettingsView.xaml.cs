using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Company;

public partial class CompanySettingsView : UserControl
{
    public CompanySettingsView(CompanySettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}