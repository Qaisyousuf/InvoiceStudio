using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Auto-load dashboard data when view loads
        Loaded += (s, e) => _ = viewModel.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}