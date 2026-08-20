using Biblia.Presentation.ViewModels;
namespace Biblia.Presentation.Views;
public partial class MessageTopicsPage : ContentPage, IQueryAttributable
{
 private readonly MessageTopicsViewModel _viewModel;
 public MessageTopicsPage(MessageTopicsViewModel viewModel){InitializeComponent();BindingContext=_viewModel=viewModel;Loaded+=OnLoaded;}
 private void OnLoaded(object? sender,EventArgs e){if(_viewModel.LoadCommand.CanExecute(null))_viewModel.LoadCommand.Execute(null);}
 public void ApplyQueryAttributes(IDictionary<string,object> query){if(query.TryGetValue("MessageId",out var value)&&long.TryParse(value?.ToString(),out var id))_viewModel.RequestedMessageId=id;}
}
