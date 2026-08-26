using System.ComponentModel;
using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class ReportsPage : ContentPage
{
    private readonly ReportsViewModel _viewModel;
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    private void OnLoaded(object? sender, EventArgs e)
    {
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        if (_viewModel.LoadCommand.CanExecute(null)) _viewModel.LoadCommand.Execute(null);
    }
    private void OnUnloaded(object? sender, EventArgs e) => _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ReportsViewModel.Report) && _viewModel.Report is not null)
            MainThread.BeginInvokeOnMainThread(async () => await PageScroll.ScrollToAsync(0, 0, true));
    }
    private void OnCancelExport(object? sender, EventArgs e) => _viewModel.CancelExport();
    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("//Home");
    }
}
