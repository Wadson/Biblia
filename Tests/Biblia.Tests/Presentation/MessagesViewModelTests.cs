using Biblia.Domain.Enums;
using Biblia.Presentation.ViewModels;
using Xunit;
namespace Biblia.Tests.Presentation;
public sealed class MessagesViewModelTests
{
 [Fact]
 public void Constructor_DoesNotStartBusy_AndUsesPortugueseTypeLabels()
 {
  var viewModel=new MessagesViewModel(null!,null!,null!);
  Assert.False(viewModel.IsBusy); Assert.True(viewModel.NewCommand.CanExecute(null)); Assert.True(viewModel.SearchCommand.CanExecute(null)); Assert.False(viewModel.SaveCommand.CanExecute(null));
  Assert.Equal(["Mensagem","Pregação","Estudo","Devocional"],viewModel.TypeOptions.Select(x=>x.Label).ToArray());
  viewModel.Title="Sermão de teste"; Assert.True(viewModel.SaveCommand.CanExecute(null));
  viewModel.SelectedTypeOption=viewModel.TypeOptions.Single(x=>x.Value==MessageType.Sermon); Assert.Equal(MessageType.Sermon,viewModel.Type);
 }
}
