using CommunityToolkit.Mvvm.ComponentModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels.Base;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _title = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}