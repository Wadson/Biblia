using System.ComponentModel;
using Biblia.Presentation.ViewModels;
namespace Biblia.Presentation.Views;
public partial class MessageBibleReferencesPage : ContentPage, IQueryAttributable
{
 private readonly MessageReferencesViewModel _viewModel;
 public MessageBibleReferencesPage(MessageReferencesViewModel viewModel){InitializeComponent();BindingContext=_viewModel=viewModel;Loaded+=OnLoaded;Unloaded+=OnUnloaded;}
 private void OnLoaded(object? sender,EventArgs e){_viewModel.PropertyChanged+=OnViewModelPropertyChanged;if(_viewModel.LoadCommand.CanExecute(null))_viewModel.LoadCommand.Execute(null);}
 private void OnUnloaded(object? sender,EventArgs e)=>_viewModel.PropertyChanged-=OnViewModelPropertyChanged;
 public void ApplyQueryAttributes(IDictionary<string,object> query){if(query.TryGetValue("MessageId",out var value)&&long.TryParse(value?.ToString(),out var id))_viewModel.RequestedMessageId=id;}
 private void OnViewModelPropertyChanged(object? sender,PropertyChangedEventArgs e){MainThread.BeginInvokeOnMainThread(()=>{if(e.PropertyName==nameof(MessageReferencesViewModel.SelectedVersion)&&_viewModel.SelectedVersion is not null)VersionsList.ScrollTo(_viewModel.SelectedVersion,position:ScrollToPosition.MakeVisible,animate:true);else if(e.PropertyName==nameof(MessageReferencesViewModel.SelectedBook)&&_viewModel.SelectedBook is not null)BooksList.ScrollTo(_viewModel.SelectedBook,position:ScrollToPosition.MakeVisible,animate:true);else if(e.PropertyName==nameof(MessageReferencesViewModel.SelectedChapter)&&_viewModel.SelectedChapter>0)ChaptersList.ScrollTo(_viewModel.SelectedChapter,position:ScrollToPosition.MakeVisible,animate:true);});}
}
