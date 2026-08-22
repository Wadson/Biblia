using CommunityToolkit.Maui.Extensions;

namespace Biblia.Presentation.Views;

public partial class ExitConfirmationView : ContentView
{
    private readonly Page _host;
    public ExitConfirmationView(Page host){InitializeComponent();_host=host;Loaded+=(_,_)=>CancelButton.Focus();}
    private async void OnCancelClicked(object? sender,EventArgs e)=>await _host.ClosePopupAsync(false);
    private async void OnConfirmClicked(object? sender,EventArgs e)=>await _host.ClosePopupAsync(true);
}
