using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed record MessageTypeFilter(string Label, MessageType? Type);
public sealed record MessageTypeOption(string Label, MessageType Value);

public sealed class MessagesViewModel : INotifyPropertyChanged
{
    private readonly IMessageService _service;
    private readonly IBibleVersionManager _versions;
    private readonly IAppNavigator _navigator;
    private Message? _selected;
    private BibleVersionCatalogEntry? _preferredVersion;
    private string _title = "", _description = "", _introduction = "", _conclusion = "", _search = "", _status = "";
    private MessageType _type = MessageType.Message;
    private MessageTypeOption? _selectedTypeOption;
    private MessageTypeFilter? _filter;
    private bool _busy;

    public MessagesViewModel(IMessageService service, IBibleVersionManager versions, IAppNavigator navigator)
    {
        _service = service;
        _versions = versions;
        _navigator = navigator;
        Filters = [new("Todos", null), new("Mensagens", MessageType.Message), new("Pregações", MessageType.Sermon), new("Estudos", MessageType.Study), new("Devocionais", MessageType.Devotional)];
        TypeOptions = [new("Mensagem",MessageType.Message),new("Pregação",MessageType.Sermon),new("Estudo",MessageType.Study),new("Devocional",MessageType.Devotional)];
        _filter = Filters[0];
        _selectedTypeOption = TypeOptions[0];
        LoadCommand = new AsyncCommand(LoadAsync, () => !IsBusy);
        SearchCommand = new AsyncCommand(LoadAsync, () => !IsBusy);
        NewCommand = new AsyncCommand(NewAsync, () => !IsBusy);
        SaveCommand = new AsyncCommand(SaveAsync, () => !string.IsNullOrWhiteSpace(Title) && !IsBusy);
        DeleteCommand = new AsyncCommand(DeleteAsync, () => SelectedItem is not null && !IsBusy);
        ReferencesCommand = new AsyncCommand(OpenReferencesAsync, () => SelectedItem is not null && SelectedItem.Id > 0 && !IsBusy);
        TopicsCommand = new AsyncCommand(OpenTopicsAsync, () => SelectedItem is not null && SelectedItem.Id > 0 && !IsBusy);
    }

    public ObservableCollection<Message> Items { get; } = [];
    public ObservableCollection<BibleVersionCatalogEntry> Versions { get; } = [];
    public IReadOnlyList<MessageTypeFilter> Filters { get; }
    public IReadOnlyList<MessageTypeOption> TypeOptions { get; }
    public MessageTypeFilter? SelectedFilter { get => _filter; set { if (Set(ref _filter, value) && value is not null && !IsBusy) _ = RunAsync(() => LoadMessagesAsync(SelectedItem?.Id)); } }
    public Message? SelectedItem { get => _selected; set { if (Set(ref _selected, value)) PopulateEditor(value); } }
    public BibleVersionCatalogEntry? PreferredVersion { get => _preferredVersion; set => Set(ref _preferredVersion, value); }
    public string Title { get => _title; set { if(Set(ref _title,value)){On(nameof(RequirementText));NotifyCommands();} } }
    public string Description { get => _description; set => Set(ref _description, value); }
    public string Introduction { get => _introduction; set => Set(ref _introduction, value); }
    public string Conclusion { get => _conclusion; set => Set(ref _conclusion, value); }
    public string SearchText { get => _search; set => Set(ref _search, value); }
    public MessageType Type { get => _type; set { if(Set(ref _type,value)){_selectedTypeOption=TypeOptions.First(x=>x.Value==value);On(nameof(SelectedTypeOption));} } }
    public MessageTypeOption? SelectedTypeOption { get=>_selectedTypeOption; set { if(Set(ref _selectedTypeOption,value)&&value is not null)Type=value.Value; } }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) NotifyCommands(); } }
    public string EditorTitle => SelectedItem is null ? "Nova mensagem" : "Editar mensagem";
    public string RequirementText => string.IsNullOrWhiteSpace(Title) ? "Informe o título para habilitar Salvar." : "Pronto para salvar a mensagem.";
    public AsyncCommand LoadCommand { get; }
    public AsyncCommand SearchCommand { get; }
    public AsyncCommand NewCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public AsyncCommand DeleteCommand { get; }
    public AsyncCommand ReferencesCommand { get; }
    public AsyncCommand TopicsCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync() => await RunAsync(async () =>
    {
        await _versions.InitializeCatalogAsync();
        Versions.Clear();
        foreach (var version in (await _versions.GetVersionsAsync()).Where(x => x.IsInstalled && x.IsEnabled)) Versions.Add(version);
        var activeVersion = await _versions.GetActiveVersionAsync();
        PreferredVersion ??= Versions.FirstOrDefault(x => x.Id == activeVersion?.Id) ?? Versions.FirstOrDefault();
        await LoadMessagesAsync(SelectedItem?.Id);
    });

    private Task NewAsync()
    {
        SelectedItem = null;
        Title = Description = Introduction = Conclusion = "";
        Type = MessageType.Message;
        SelectedTypeOption = TypeOptions[0];
        PreferredVersion = Versions.FirstOrDefault();
        On(nameof(EditorTitle));
        return Task.CompletedTask;
    }

    private async Task SaveAsync() => await RunAsync(async () =>
    {
        var saved = await _service.SaveAsync(SelectedItem?.Id, Title, Description, Type, Introduction, Conclusion, PreferredVersion?.Id);
        await LoadMessagesAsync(saved.Id);
        Status = "Mensagem salva.";
    });

    private async Task DeleteAsync() => await RunAsync(async () =>
    {
        await _service.DeleteAsync(SelectedItem!.Id);
        await NewAsync();
        await LoadMessagesAsync(null);
        Status = "Mensagem excluída.";
    });

    private Task OpenReferencesAsync() => _navigator.GoToAsync("//MessageBibleReferences", new Dictionary<string, object> { ["MessageId"] = SelectedItem!.Id });
    private Task OpenTopicsAsync() => _navigator.GoToAsync("MessageTopics", new Dictionary<string, object> { ["MessageId"] = SelectedItem!.Id });

    private async Task LoadMessagesAsync(long? selectedId)
    {
        var messages = await _service.SearchAsync(SearchText, SelectedFilter?.Type);
        Items.Clear();
        foreach (var message in messages) Items.Add(message);
        SelectedItem = Items.FirstOrDefault(x => x.Id == selectedId);
        Status = Items.Count == 0 ? "Nenhuma mensagem encontrada." : $"{Items.Count} mensagem(ns).";
    }

    private void PopulateEditor(Message? message)
    {
        if (message is null) return;
        Title = message.Title; Description = message.Description ?? ""; Introduction = message.Introduction ?? ""; Conclusion = message.Conclusion ?? ""; Type = message.Type;
        SelectedTypeOption = TypeOptions.First(x=>x.Value==message.Type);
        PreferredVersion = Versions.FirstOrDefault(x => x.Id == message.PreferredBibleVersionId);
        On(nameof(EditorTitle));
        NotifyCommands();
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await action(); }
        catch (Exception exception) { Status = exception.Message; }
        finally { IsBusy = false; }
    }

    private void NotifyCommands() { LoadCommand.NotifyCanExecuteChanged(); SearchCommand.NotifyCanExecuteChanged(); NewCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged(); DeleteCommand.NotifyCanExecuteChanged(); ReferencesCommand.NotifyCanExecuteChanged(); TopicsCommand.NotifyCanExecuteChanged(); }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; On(name); return true; }
    private void On(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
