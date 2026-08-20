using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private bool _initialized;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_initialized)
        {
            _initialized = true;

            await _viewModel.InitializeAsync();
            return;
        }

        await _viewModel.RefreshAsync();
    }
}