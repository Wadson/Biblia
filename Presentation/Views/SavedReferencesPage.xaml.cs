using Biblia.Presentation.ViewModels;

namespace Biblia.Presentation.Views;

public partial class SavedReferencesPage : ContentPage
{
    private readonly SavedReferencesViewModel _viewModel;
    private bool _changing;
    public SavedReferencesPage(SavedReferencesViewModel viewModel) { InitializeComponent(); BindingContext = _viewModel = viewModel; Loaded += OnLoaded; }
    private void OnLoaded(object? sender, EventArgs e) { Loaded -= OnLoaded; _viewModel.LoadCommand.Execute(null); }
    private async void OnVersionChanged(object? sender, EventArgs e) { if (_changing) return; _changing = true; try { await _viewModel.ChangeVersionAsync(); } finally { _changing = false; } }
    private async void OnBookChanged(object? sender, EventArgs e) { if (_changing) return; _changing = true; try { await _viewModel.ChangeBookAsync(); } finally { _changing = false; } }
    private async void OnChapterChanged(object? sender, EventArgs e) { if (_changing) return; _changing = true; try { await _viewModel.ChangeChapterAsync(); } finally { _changing = false; } }
    private async void OnVerseChanged(object? sender, EventArgs e) { if (_changing) return; _changing = true; try { await _viewModel.RefreshPreviewAsync(); } finally { _changing = false; } }
}
