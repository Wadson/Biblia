using Biblia.Presentation.ViewModels;
namespace Biblia.Presentation.Views;
public partial class MessagesPage : ContentPage
{
 private readonly MessagesViewModel _viewModel;
 public MessagesPage(MessagesViewModel viewModel){InitializeComponent();BindingContext=_viewModel=viewModel;Loaded+=OnLoaded;}
 private void OnLoaded(object? sender,EventArgs e){if(_viewModel.LoadCommand.CanExecute(null))_viewModel.LoadCommand.Execute(null);}
}
