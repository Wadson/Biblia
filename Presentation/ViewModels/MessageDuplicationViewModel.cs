using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class MessageDuplicationViewModel : INotifyPropertyChanged
{
    private readonly IMessageRepository _messages;
    private readonly IMessageDuplicationService _duplication;
    private Message? _selected;
    private string _title = string.Empty, _status = string.Empty;
    private bool _busy;
    public MessageDuplicationViewModel(IMessageRepository messages, IMessageDuplicationService duplication,IAppNavigator navigator) { _messages = messages; _duplication = duplication; LoadCommand = new AsyncCommand(LoadAsync, () => !IsBusy); DuplicateCommand = new AsyncCommand(DuplicateAsync, () => SelectedMessage is not null && !IsBusy);BackCommand=new AsyncCommand(()=>navigator.GoBackAsync(),()=>!IsBusy); }
    public ObservableCollection<Message> Messages { get; } = [];
    public Message? SelectedMessage { get => _selected; set => Set(ref _selected, value); }
    public string Title { get => _title; set => Set(ref _title, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) { LoadCommand.NotifyCanExecuteChanged(); DuplicateCommand.NotifyCanExecuteChanged(); } } }
    public AsyncCommand LoadCommand { get; }
    public AsyncCommand DuplicateCommand { get; }
    public AsyncCommand BackCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
    private async Task LoadAsync() { if (IsBusy) return; IsBusy = true; try { Messages.Clear(); foreach (var message in await _messages.GetAllAsync()) Messages.Add(message); SelectedMessage = Messages.FirstOrDefault(); } catch (Exception ex) { Status = ex.Message; } finally { IsBusy = false; } }
    private async Task DuplicateAsync() { if (SelectedMessage is null) return; IsBusy = true; try { var copy = await _duplication.DuplicateAsync(SelectedMessage.Id, Title); Messages.Insert(0, copy); SelectedMessage = copy; Title = string.Empty; Status = "Mensagem duplicada com tópicos, referências e observações."; } catch (Exception ex) { Status = ex.Message; } finally { IsBusy = false; } }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; PropertyChanged?.Invoke(this, new(name)); return true; }
}
