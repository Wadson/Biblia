using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class ReportsPage : ContentPage
{
    private readonly ReportsViewModel _viewModel;
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        Loaded += (_, _) => _viewModel.LoadCommand.Execute(null);
    }
    private void OnCancelExport(object? sender, EventArgs e) => _viewModel.CancelExport();
    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("//Home");
    }
}
