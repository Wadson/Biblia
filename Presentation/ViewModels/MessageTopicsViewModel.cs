using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class MessageTopicsViewModel : INotifyPropertyChanged
{
    private readonly IMessageService _messagesService;
    private readonly IMessageTopicService _topicsService;
    private readonly IAppNavigator? _navigator;
    private Message? _selectedMessage;
    private MessageTopic? _selectedTopic;
    private string _title = "", _content = "", _status = "";
    private bool _isBusy;
    private long? _requestedMessageId;

    public MessageTopicsViewModel(IMessageService messagesService, IMessageTopicService topicsService, IAppNavigator? navigator = null)
    {
        _messagesService = messagesService; _topicsService = topicsService; _navigator = navigator;
        LoadCommand = new(LoadAsync, () => !IsBusy);
        NewCommand = new(NewAsync, () => SelectedMessage is not null && !IsBusy);
        SaveCommand = new(SaveAsync, CanSave);
        DeleteCommand = new(DeleteAsync, () => SelectedTopic is not null && !IsBusy);
        UpCommand = new(() => MoveAsync(-1), CanMoveUp);
        DownCommand = new(() => MoveAsync(1), CanMoveDown);
        SelectMessageCommand = new(OpenMessagesAsync, () => !IsBusy);
    }

    public ObservableCollection<Message> Messages { get; } = [];
    public ObservableCollection<MessageTopic> Topics { get; } = [];
    public long? RequestedMessageId { get => _requestedMessageId; set => Set(ref _requestedMessageId, value); }
    public Message? SelectedMessage
    {
        get => _selectedMessage;
        set
        {
            if (!Set(ref _selectedMessage, value)) return;
            On(nameof(HasMessage)); On(nameof(MessageTitle)); On(nameof(MessageType)); On(nameof(RequirementText));
            _ = ChangeMessageAsync(); NotifyCommands();
        }
    }
    public MessageTopic? SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            if (!Set(ref _selectedTopic, value)) return;
            if (value is not null) { Title = value.Title; Content = value.Content ?? ""; }
            On(nameof(EditorTitle)); NotifyCommands();
        }
    }
    public string Title { get => _title; set { if (Set(ref _title, value)) { On(nameof(RequirementText)); NotifyCommands(); } } }
    public string Content { get => _content; set => Set(ref _content, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public bool IsBusy { get => _isBusy; private set { if (Set(ref _isBusy, value)) NotifyCommands(); } }
    public bool HasMessage => SelectedMessage is not null;
    public string MessageTitle => SelectedMessage?.Title ?? "Nenhuma mensagem selecionada";
    public string MessageType => SelectedMessage?.Type.ToString() ?? "Escolha ou crie uma mensagem para organizar seus tópicos.";
    public string EditorTitle => SelectedTopic is null ? "Novo tópico" : "Editar tópico";
    public string RequirementText => SelectedMessage is null ? "Selecione uma mensagem antes de salvar tópicos." : string.IsNullOrWhiteSpace(Title) ? "Informe o título do tópico." : "Pronto para salvar o tópico.";

    public AsyncCommand LoadCommand { get; }
    public AsyncCommand NewCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public AsyncCommand DeleteCommand { get; }
    public AsyncCommand UpCommand { get; }
    public AsyncCommand DownCommand { get; }
    public AsyncCommand SelectMessageCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync() => await RunAsync(async () =>
    {
        Messages.Clear();
        foreach (var message in await _messagesService.SearchAsync(null, null)) Messages.Add(message);
        var target = Messages.FirstOrDefault(x => x.Id == RequestedMessageId) ?? Messages.FirstOrDefault();
        if (target is null) { SelectedMessage = null; Topics.Clear(); Status = "Crie e salve uma mensagem antes de cadastrar tópicos."; return; }
        _selectedMessage = target; On(nameof(SelectedMessage)); On(nameof(HasMessage)); On(nameof(MessageTitle)); On(nameof(MessageType));
        await LoadTopicsCoreAsync(); Status = Topics.Count == 0 ? "Nenhum tópico cadastrado nesta mensagem." : $"{Topics.Count} tópico(s) nesta mensagem.";
    });

    private async Task ChangeMessageAsync()
    {
        if (IsBusy) return;
        await RunAsync(async () => { await NewCoreAsync(); await LoadTopicsCoreAsync(); Status = SelectedMessage is null ? "Selecione uma mensagem." : $"{Topics.Count} tópico(s) nesta mensagem."; });
    }

    private Task NewAsync() { NewCoreAsync(); Status = "Preencha o novo tópico."; return Task.CompletedTask; }
    private Task NewCoreAsync() { _selectedTopic = null; On(nameof(SelectedTopic)); On(nameof(EditorTitle)); Title = ""; Content = ""; NotifyCommands(); return Task.CompletedTask; }

    private async Task SaveAsync() => await RunAsync(async () =>
    {
        var isNew = SelectedTopic is null;
        var saved = await _topicsService.SaveAsync(SelectedMessage!.Id, SelectedTopic?.Id, Title, Content);
        await LoadTopicsCoreAsync(saved.Id);
        Status = isNew ? "Tópico salvo com sucesso." : "Tópico atualizado com sucesso.";
    });

    private async Task DeleteAsync() => await RunAsync(async () =>
    {
        await _topicsService.DeleteAsync(SelectedTopic!.Id);
        await NewCoreAsync(); await LoadTopicsCoreAsync(); Status = "Tópico excluído.";
    });

    private async Task MoveAsync(int direction) => await RunAsync(async () =>
    {
        var selectedId = SelectedTopic!.Id;
        await _topicsService.MoveAsync(SelectedMessage!.Id, Topics.ToArray(), selectedId, direction);
        await LoadTopicsCoreAsync(selectedId); Status = direction < 0 ? "Tópico movido acima." : "Tópico movido abaixo.";
    });

    private async Task LoadTopicsCoreAsync(long? selectedId = null)
    {
        Topics.Clear();
        if (SelectedMessage is null) { SelectedTopic = null; return; }
        foreach (var topic in await _topicsService.GetTopicsAsync(SelectedMessage.Id)) Topics.Add(topic);
        SelectedTopic = selectedId is null ? null : Topics.FirstOrDefault(x => x.Id == selectedId);
        On(nameof(RequirementText)); NotifyCommands();
    }

    private Task OpenMessagesAsync() => _navigator?.GoToAsync("Messages") ?? Task.CompletedTask;
    private bool CanSave() => SelectedMessage is not null && SelectedMessage.Id > 0 && !string.IsNullOrWhiteSpace(Title) && !IsBusy;
    private bool CanMoveUp() => SelectedTopic is not null && Topics.IndexOf(SelectedTopic) > 0 && !IsBusy;
    private bool CanMoveDown() => SelectedTopic is not null && Topics.IndexOf(SelectedTopic) >= 0 && Topics.IndexOf(SelectedTopic) < Topics.Count - 1 && !IsBusy;
    private async Task RunAsync(Func<Task> action) { if (IsBusy) return; IsBusy = true; try { await action(); } catch (Exception exception) { Status = exception.Message; } finally { IsBusy = false; } }
    private void NotifyCommands() { LoadCommand.NotifyCanExecuteChanged(); NewCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged(); DeleteCommand.NotifyCanExecuteChanged(); UpCommand.NotifyCanExecuteChanged(); DownCommand.NotifyCanExecuteChanged(); SelectMessageCommand.NotifyCanExecuteChanged(); }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; On(name); return true; }
    private void On(string? name) => PropertyChanged?.Invoke(this, new(name));
}
