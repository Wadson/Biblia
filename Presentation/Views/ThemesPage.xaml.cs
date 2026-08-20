using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class ThemesPage : ContentPage
{
    private readonly ThemesViewModel _viewModel;
    public ThemesPage(ThemesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        if (_viewModel.LoadCommand.CanExecute(null)) _viewModel.LoadCommand.Execute(null);
    }
}
