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
}
