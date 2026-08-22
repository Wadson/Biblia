using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class ThemeSelectionItem(Theme theme) : INotifyPropertyChanged
{
    private bool _isSelected;
    public Theme Theme { get; } = theme;
    public string DisplayText => Theme.Name;
    public string ColorHex => string.IsNullOrWhiteSpace(Theme.ColorHex) ? "#0066CC" : Theme.ColorHex;
    public bool IsSelected { get => _isSelected; set { if (_isSelected == value) return; _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class TopicSelectionItem(MessageTopic? topic) : INotifyPropertyChanged
{
    private bool _isSelected;
    public MessageTopic? Topic { get; } = topic;
    public string DisplayText => Topic?.Title ?? "Sem tópico";
    public string ColorHex => "#0066CC";
    public bool IsSelected { get => _isSelected; set { if (_isSelected == value) return; _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class MessageReferenceItem
{
    public MessageReferenceItem(MessageReference reference, SavedReferenceDetails details, string bookName, string? topicTitle)
    {
        Reference = reference; Details = details;
        DisplayReference = $"{bookName} {details.Reference.Chapter}:{details.Reference.VerseStart}" + (details.Reference.VerseEnd == details.Reference.VerseStart ? "" : $"–{details.Reference.VerseEnd}");
        Version = reference.PreferredBibleVersionId is null ? "Versão não definida" : "Versão preferida vinculada";
        Themes = details.Themes.Count == 0 ? "Sem temas" : string.Join(" • ", details.Themes.Select(x => x.Name));
        Comment = details.Reference.Comment ?? "Sem comentário."; Observation = reference.Observation ?? "Sem observação nesta mensagem."; Topic = topicTitle ?? "Sem tópico";
    }
    public MessageReference Reference { get; }
    public SavedReferenceDetails Details { get; }
    public string DisplayReference { get; }
    public string Version { get; }
    public string Themes { get; }
    public string Comment { get; }
    public string Observation { get; }
    public string Topic { get; }
}

public sealed class MessageReferencesViewModel : INotifyPropertyChanged
{
    private readonly IMessageService _messagesService;
    private readonly IMessageTopicService _topicsService;
    private readonly IMessageReferenceService _references;
    private readonly ISavedReferenceService _saved;
    private readonly IThemeService _themesService;
    private readonly IBibleVersionManager _versionsService;
    private readonly IBibleRepository _bible;
    private readonly IAppNavigator? _navigator;
    private readonly IMessageDuplicationService? _duplication;
    private Message? _message;
    private MessageTopic? _topic;
    private MessageReferenceItem? _editingItem;
    private BibleVersionCatalogEntry? _version;
    private BibleBook? _book;
    private int _chapter;
    private int? _selectionStart, _selectionEnd;
    private ThemeSelectionItem? _selectedTheme;
    private string _comment = "", _observation = "", _status = "", _themeSearchText = "";
    private bool _busy, _bibleBusy, _themeSheetOpen, _topicSheetOpen;
    private CancellationTokenSource? _themeSearchCancellation;
    private readonly List<MessageReferenceItem> _allItems = [];
    private long? _requestedMessageId;

    public MessageReferencesViewModel(IMessageService messagesService, IMessageTopicService topicsService, IMessageReferenceService references, ISavedReferenceService saved, IThemeService themesService, IBibleVersionManager versionsService, IBibleRepository bible, IAppNavigator? navigator = null,IMessageDuplicationService? duplication=null)
    {
        _messagesService = messagesService; _topicsService = topicsService; _references = references; _saved = saved; _themesService = themesService; _versionsService = versionsService; _bible = bible; _navigator = navigator;_duplication=duplication;
        LoadCommand = new(LoadAsync, () => !IsBusy);
        AddCommand = new(AddAsync, CanAdd);
        SaveCommand = new(SaveAsync, () => EditingItem is not null && SelectedTheme is not null && !IsBusy);
        ToggleVerseCommand = new(ToggleVerseAsync, _ => !IsBibleBusy);
        OpenThemeSheetCommand = new(() => { IsThemeSheetOpen = true; return Task.CompletedTask; });
        CloseThemeSheetCommand = new(() => { IsThemeSheetOpen = false; return Task.CompletedTask; });
        SelectThemeCommand = new AsyncCommand<ThemeSelectionItem>(SelectThemeAsync);
        SearchThemeCommand = new AsyncCommand<string>(SearchThemesAsync);
        OpenTopicSheetCommand = new(() => { IsTopicSheetOpen = true; return Task.CompletedTask; });
        CloseTopicSheetCommand = new(() => { IsTopicSheetOpen = false; return Task.CompletedTask; });
        SelectTopicCommand = new AsyncCommand<TopicSelectionItem>(SelectTopicAsync);
        EditCommand = new AsyncCommand<MessageReferenceItem>(BeginEditAsync, _ => !IsBusy);
        DeleteCommand = new AsyncCommand<MessageReferenceItem>(DeleteAsync, _ => !IsBusy);
        MoveUpCommand=new AsyncCommand<MessageReferenceItem>(item=>MoveAsync(item,-1),_=>!IsBusy);
        MoveDownCommand=new AsyncCommand<MessageReferenceItem>(item=>MoveAsync(item,1),_=>!IsBusy);
        CancelEditCommand = new(CancelEditAsync, () => EditingItem is not null && !IsBusy);
        SelectMessageCommand = new(OpenMessagesAsync, () => !IsBusy);
        CreateThemeCommand=new(()=>_navigator?.GoToAsync("//Themes")??Task.CompletedTask);
        DuplicateMessageCommand=new(DuplicateMessageAsync,()=>SelectedMessage is not null&&!IsBusy&&_duplication is not null);
        OpenReportsCommand=new(()=>_navigator?.GoToAsync("//Reports")??Task.CompletedTask);
        BackCommand=new(()=>_navigator?.GoBackAsync()??Task.CompletedTask,()=>!IsBusy);
    }

    public ObservableCollection<Message> Messages { get; } = [];
    public ObservableCollection<MessageTopic> Topics { get; } = [];
    public ObservableCollection<TopicSelectionItem> TopicOptions { get; } = [];
    public ObservableCollection<MessageReferenceItem> Items { get; } = [];
    public ObservableCollection<BibleVersionCatalogEntry> Versions { get; } = [];
    public ObservableCollection<BibleBook> Books { get; } = [];
    public ObservableCollection<int> Chapters { get; } = [];
    public ObservableCollection<BibleVerseItemViewModel> Verses { get; } = [];
    public ObservableCollection<ThemeSelectionItem> ThemeOptions { get; } = [];

    public Message? SelectedMessage { get => _message; set { if (Set(ref _message, value)) _ = LoadMessageAsync(); } }
    public long? RequestedMessageId { get => _requestedMessageId; set => Set(ref _requestedMessageId, value); }
    public string MessageTitle => SelectedMessage?.Title ?? "Nenhuma mensagem selecionada";
    public string MessageType => SelectedMessage?.Type.ToString() ?? "Selecione uma mensagem para continuar.";
    public bool HasMessage => SelectedMessage is not null;
    public bool HasThemes=>ThemeOptions.Count>0;
    public bool HasTopics => Topics.Count > 0;
    public MessageTopic? SelectedTopic { get => _topic; private set { if (Set(ref _topic, value)) { RefreshTopicSelection(); On(nameof(SelectedTopicText)); NotifyCommands(); } } }
    public string SelectedTopicText => SelectedTopic?.Title ?? (HasTopics ? "Selecionar tópico (opcional)" : "Nenhum tópico criado.");
    public MessageReferenceItem? EditingItem { get => _editingItem; private set { if (Set(ref _editingItem, value)) { On(nameof(IsEditing)); NotifyCommands(); } } }
    public bool IsEditing => EditingItem is not null;
    public BibleVersionCatalogEntry? SelectedVersion { get => _version; set { if (Set(ref _version, value)) { NotifyCommands(); _ = LoadBooksAsync(); } } }
    public BibleBook? SelectedBook { get => _book; set { if (Set(ref _book, value)) { NotifyCommands(); _ = LoadChaptersAsync(); } } }
    public int SelectedChapter { get => _chapter; set { if (Set(ref _chapter, value)) { NotifyCommands(); _ = LoadVersesAsync(); } } }
    public ThemeSelectionItem? SelectedTheme { get => _selectedTheme; private set { if (Set(ref _selectedTheme, value)) { RefreshThemeSelection(); RefreshVisibleItems(); On(nameof(SelectedThemeText)); On(nameof(SelectedThemeColor)); On(nameof(ReferencesTitle)); NotifyCommands(); } } }
    public string SelectedThemeText => SelectedTheme?.DisplayText ?? "Selecionar tema";
    public string SelectedThemeColor => SelectedTheme?.ColorHex ?? "#0066CC";
    public string ReferencesTitle => SelectedTheme is null ? "Referências da mensagem" : $"Referências vinculadas ao tema {SelectedTheme.DisplayText}";
    public string Comment { get => _comment; set => Set(ref _comment, value); }
    public string Observation { get => _observation; set => Set(ref _observation, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public string ThemeSearchText { get => _themeSearchText; set => Set(ref _themeSearchText, value); }
    public string FormattedReference => SelectedBook is null || _selectionStart is null ? "Nenhum versículo selecionado." : $"{SelectedBook.Name} {SelectedChapter}:{_selectionStart}" + (_selectionEnd == _selectionStart ? "" : $"–{_selectionEnd}");
    public string SelectionHint => _selectionStart is null ? "Toque em um versículo para iniciar." : $"{SelectionCount} " + (SelectionCount == 1 ? "versículo" : "versículos");
    public int SelectionCount => _selectionStart is null || _selectionEnd is null ? 0 : _selectionEnd.Value - _selectionStart.Value + 1;
    public string AddRequirementText
    {
        get
        {
            var missing = new List<string>();
            if (SelectedVersion is null) missing.Add("versão");
            if (SelectedBook is null) missing.Add("livro");
            if (SelectedChapter <= 0) missing.Add("capítulo");
            if (_selectionStart is null || _selectionEnd is null) missing.Add("versículo");
            if (SelectedTheme is null) missing.Add("tema");
            return missing.Count == 0 ? "Pronto para adicionar à mensagem." : $"Para adicionar, selecione: {string.Join(", ", missing)}.";
        }
    }
    public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) NotifyCommands(); } }
    public bool IsBibleBusy { get => _bibleBusy; private set { if (Set(ref _bibleBusy, value)) ToggleVerseCommand.NotifyCanExecuteChanged(); } }
    public bool IsThemeSheetOpen { get => _themeSheetOpen; private set => Set(ref _themeSheetOpen, value); }
    public bool IsTopicSheetOpen { get => _topicSheetOpen; private set => Set(ref _topicSheetOpen, value); }

    public AsyncCommand LoadCommand { get; }
    public AsyncCommand AddCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public AsyncCommand<BibleVerseItemViewModel> ToggleVerseCommand { get; }
    public AsyncCommand OpenThemeSheetCommand { get; }
    public AsyncCommand CloseThemeSheetCommand { get; }
    public AsyncCommand<ThemeSelectionItem> SelectThemeCommand { get; }
    public AsyncCommand<string> SearchThemeCommand { get; }
    public AsyncCommand OpenTopicSheetCommand { get; }
    public AsyncCommand CloseTopicSheetCommand { get; }
    public AsyncCommand<TopicSelectionItem> SelectTopicCommand { get; }
    public AsyncCommand<MessageReferenceItem> EditCommand { get; }
    public AsyncCommand<MessageReferenceItem> DeleteCommand { get; }
    public AsyncCommand<MessageReferenceItem> MoveUpCommand{get;}
    public AsyncCommand<MessageReferenceItem> MoveDownCommand{get;}
    public AsyncCommand CancelEditCommand { get; }
    public AsyncCommand SelectMessageCommand { get; }
    public AsyncCommand CreateThemeCommand{get;}
    public AsyncCommand DuplicateMessageCommand{get;}
    public AsyncCommand OpenReportsCommand{get;}
    public AsyncCommand BackCommand{get;}
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync() => await RunAsync(async () =>
    {
        await _versionsService.InitializeCatalogAsync();
        Versions.Clear();
        foreach (var version in (await _versionsService.GetVersionsAsync()).Where(x => x.IsInstalled && x.IsEnabled)) Versions.Add(version);
        await RefreshThemesAsync(null, CancellationToken.None);
        Messages.Clear();
        foreach (var message in await _messagesService.SearchAsync(null, null)) Messages.Add(message);
        var active = await _versionsService.GetActiveVersionAsync();
        SelectedMessage = Messages.FirstOrDefault(x => x.Id == RequestedMessageId) ?? Messages.FirstOrDefault();
        SelectedVersion = Versions.FirstOrDefault(x => x.Id == SelectedMessage?.PreferredBibleVersionId) ?? Versions.FirstOrDefault(x => x.Id == active?.Id) ?? Versions.FirstOrDefault();
        Status = Messages.Count == 0 ? "Selecione o tema e o trecho; a pregação será criada automaticamente ao adicionar." : "Selecione o texto bíblico e um tema.";
    });

    private async Task LoadBooksAsync()
    {
        IsBibleBusy = true;
        try
        {
            var prior = _book?.BookReferenceId;
            Books.Clear(); Chapters.Clear(); Verses.Clear(); ClearVerseSelection();
            if (SelectedVersion is null) return;
            foreach (var book in await _bible.GetBooksAsync(SelectedVersion.Code)) Books.Add(book);
            SelectedBook = Books.FirstOrDefault(x => x.BookReferenceId == prior) ?? Books.FirstOrDefault();
            if (SelectedMessage is not null) await LoadItemsAsync();
        }
        catch (Exception exception) { Status = exception.Message; }
        finally { IsBibleBusy = false; }
    }

    private async Task LoadChaptersAsync()
    {
        IsBibleBusy = true;
        try
        {
            var prior = _chapter;
            Chapters.Clear(); Verses.Clear(); ClearVerseSelection();
            if (SelectedVersion is null || SelectedBook is null) return;
            foreach (var chapter in await _bible.GetChaptersAsync(SelectedVersion.Code, SelectedBook.BookReferenceId)) Chapters.Add(chapter);
            SelectedChapter = Chapters.Contains(prior) ? prior : Chapters.FirstOrDefault();
        }
        catch (Exception exception) { Status = exception.Message; }
        finally { IsBibleBusy = false; }
    }

    private async Task LoadVersesAsync()
    {
        IsBibleBusy = true;
        try
        {
            Verses.Clear(); ClearVerseSelection();
            if (SelectedVersion is null || SelectedBook is null || SelectedChapter < 1) return;
            foreach (var verse in await _bible.GetVersesAsync(SelectedVersion.Code, SelectedBook.BookReferenceId, SelectedChapter)) Verses.Add(new(verse));
        }
        catch (Exception exception) { Status = exception.Message; }
        finally { IsBibleBusy = false; }
    }

    private async Task LoadMessageAsync()
    {
        try
        {
            Topics.Clear(); TopicOptions.Clear(); SelectedTopic = null; EditingItem = null;
            On(nameof(MessageTitle)); On(nameof(MessageType)); On(nameof(HasMessage));
            if (SelectedMessage is null) { Items.Clear(); return; }
            foreach (var topic in await _topicsService.GetTopicsAsync(SelectedMessage.Id)) Topics.Add(topic);
            TopicOptions.Add(new(null));
            foreach (var topic in Topics) TopicOptions.Add(new(topic));
            On(nameof(HasTopics)); On(nameof(SelectedTopicText));
            await LoadItemsAsync(); NotifyCommands();
        }
        catch (Exception exception) { Status = exception.Message; }
    }

    private async Task LoadItemsAsync()
    {
        _allItems.Clear(); Items.Clear();
        if (SelectedMessage is null) return;
        foreach (var link in await _references.GetAsync(SelectedMessage.Id))
        {
            var details = await _saved.GetDetailsAsync(link.ReferenceId);
            if (details is null) continue;
            var name = Books.FirstOrDefault(x => x.BookReferenceId == details.Reference.BookReferenceId)?.Name ?? $"Livro {details.Reference.BookReferenceId}";
            _allItems.Add(new(link, details, name, Topics.FirstOrDefault(x => x.Id == link.TopicId)?.Title));
        }
        RefreshVisibleItems();
    }

    private Task ToggleVerseAsync(BibleVerseItemViewModel item)
    {
        var number = item.Number;
        if (_selectionStart == number && _selectionEnd == number) ClearVerseSelection();
        else if (_selectionStart is null) { _selectionStart = _selectionEnd = number; RefreshVerseSelection(); }
        else if (_selectionStart == _selectionEnd) { _selectionStart = Math.Min(_selectionStart.Value, number); _selectionEnd = Math.Max(_selectionEnd!.Value, number); RefreshVerseSelection(); }
        else { _selectionStart = _selectionEnd = number; RefreshVerseSelection(); }
        return Task.CompletedTask;
    }

    private Task SelectThemeAsync(ThemeSelectionItem item) { SelectedTheme = item; IsThemeSheetOpen = false; return Task.CompletedTask; }
    private Task SelectTopicAsync(TopicSelectionItem item) { SelectedTopic = item.Topic; IsTopicSheetOpen = false; return Task.CompletedTask; }
    private async Task SearchThemesAsync(string query)
    {
        _themeSearchCancellation?.Cancel(); _themeSearchCancellation?.Dispose();
        _themeSearchCancellation = new();
        try { await Task.Delay(180, _themeSearchCancellation.Token); await RefreshThemesAsync(query, _themeSearchCancellation.Token); }
        catch (OperationCanceledException) { }
        catch (Exception exception) { Status = exception.Message; }
    }

    private async Task RefreshThemesAsync(string? query, CancellationToken token)
    {
        var selectedId = SelectedTheme?.Theme.Id;
        var themes = await _themesService.SearchAsync(query, token);
        ThemeOptions.Clear();
        foreach (var theme in themes) ThemeOptions.Add(new(theme));
        On(nameof(HasThemes));
        if (selectedId is not null) SelectedTheme = ThemeOptions.FirstOrDefault(x => x.Theme.Id == selectedId) ?? SelectedTheme;
        RefreshThemeSelection();
    }

    private async Task AddAsync() => await RunAsync(async () =>
    {
        if(SelectedMessage is null)
        {
            var title=$"Pregação — {SelectedTheme!.DisplayText}";
            var created=await _messagesService.SaveAsync(null,title,$"Referências relacionadas ao tema {SelectedTheme.DisplayText}.",Biblia.Domain.Enums.MessageType.Sermon,preferredBibleVersionId:SelectedVersion?.Id);
            Messages.Add(created);SelectedMessage=created;
        }
        var existing = await _saved.FindCanonicalAsync(SelectedBook!.BookReferenceId, SelectedChapter, _selectionStart!.Value, _selectionEnd!.Value);
        var themeIds = (existing?.Themes.Select(x => x.Id) ?? []).Append(SelectedTheme!.Theme.Id).Distinct().ToArray();
        var comment = string.IsNullOrWhiteSpace(Comment) ? existing?.Reference.Comment : Comment;
        var saved = await _saved.SaveAsync(existing?.Reference.Id, SelectedBook.BookReferenceId, SelectedChapter, _selectionStart.Value, _selectionEnd.Value, comment, SelectedVersion!.Id, themeIds);
        await _references.AddAsync(SelectedMessage!.Id, saved.Reference.Id, null, null, SelectedVersion.Id);
        ResetAfterAdd(); await LoadItemsAsync(); Status = $"Referência adicionada à mensagem e vinculada ao tema {SelectedTheme!.DisplayText}.";
    });

    private Task BeginEditAsync(MessageReferenceItem item)
    {
        EditingItem = item;
        Comment = item.Details.Reference.Comment ?? "";
        Observation = item.Reference.Observation ?? "";
        SelectedTopic = Topics.FirstOrDefault(x => x.Id == item.Reference.TopicId);
        var preferredThemeId = item.Details.Themes.FirstOrDefault()?.Id;
        SelectedTheme = ThemeOptions.FirstOrDefault(x => x.Theme.Id == preferredThemeId);
        if (item.Reference.PreferredBibleVersionId is not null) SelectedVersion = Versions.FirstOrDefault(x => x.Id == item.Reference.PreferredBibleVersionId) ?? SelectedVersion;
        Status = "Editando vínculo selecionado.";
        return Task.CompletedTask;
    }

    private async Task SaveAsync() => await RunAsync(async () =>
    {
        var details = (await _saved.GetDetailsAsync(EditingItem!.Reference.ReferenceId)) ?? throw new KeyNotFoundException("Referência não encontrada.");
        var themeIds = details.Themes.Select(x => x.Id).Append(SelectedTheme!.Theme.Id).Distinct().ToArray();
        await _saved.SaveAsync(details.Reference.Id, details.Reference.BookReferenceId, details.Reference.Chapter, details.Reference.VerseStart, details.Reference.VerseEnd, Comment, SelectedVersion?.Id, themeIds);
        await _references.UpdateAsync(EditingItem.Reference with { TopicId = SelectedTopic?.Id, Observation = Normalize(Observation), PreferredBibleVersionId = SelectedVersion?.Id });
        ResetForm(); await LoadItemsAsync(); Status = "Vínculo atualizado sem duplicar a referência.";
    });

    private Task CancelEditAsync() { ResetForm(); Status = "Edição cancelada."; return Task.CompletedTask; }
    private Task OpenMessagesAsync() => _navigator?.GoToAsync("Messages") ?? Task.CompletedTask;
    private async Task DuplicateMessageAsync()=>await RunAsync(async()=>{var copy=await _duplication!.DuplicateAsync(SelectedMessage!.Id);Messages.Add(copy);SelectedMessage=copy;Status="Mensagem duplicada. Edite o título e continue organizando.";});
    private async Task DeleteAsync(MessageReferenceItem item) => await RunAsync(async () =>
    {
        await _references.DeleteAsync(item.Reference.Id);
        if (EditingItem?.Reference.Id == item.Reference.Id) ResetForm();
        await LoadItemsAsync(); Status = "Vínculo removido; a referência salva e seus temas foram preservados.";
    });

    private bool CanAdd() => SelectedVersion is not null && SelectedBook is not null && SelectedChapter > 0 && _selectionStart is not null && _selectionEnd is not null && SelectedTheme is not null && !IsBusy;
    private async Task MoveAsync(MessageReferenceItem item,int direction)=>await RunAsync(async()=>{if(SelectedMessage is null)return;await _references.MoveAsync(SelectedMessage.Id,_allItems.Select(x=>x.Reference).ToArray(),item.Reference.Id,direction);await LoadItemsAsync();Status="Ordem das referências atualizada.";});
    private void ResetAfterAdd() { EditingItem = null; Comment = ""; Observation = ""; SelectedTopic = null; ClearVerseSelection(); }
    private void ResetForm() { EditingItem = null; Comment = ""; Observation = ""; SelectedTopic = null; SelectedTheme = null; ClearVerseSelection(); }
    private void RefreshVisibleItems() { Items.Clear(); foreach (var item in _allItems.Where(item => SelectedTheme is null || item.Details.Themes.Any(theme => theme.Id == SelectedTheme.Theme.Id))) Items.Add(item); }
    private void ClearVerseSelection() { _selectionStart = _selectionEnd = null; RefreshVerseSelection(); }
    private void RefreshVerseSelection() { foreach (var item in Verses) item.SetSelected(_selectionStart is not null && item.Number >= _selectionStart && item.Number <= _selectionEnd); On(nameof(FormattedReference)); On(nameof(SelectionHint)); On(nameof(SelectionCount)); NotifyCommands(); }
    private void RefreshThemeSelection() { foreach (var item in ThemeOptions) item.IsSelected = item.Theme.Id == SelectedTheme?.Theme.Id; }
    private void RefreshTopicSelection() { foreach (var item in TopicOptions) item.IsSelected = item.Topic?.Id == SelectedTopic?.Id && (item.Topic is not null || SelectedTopic is null); }
    private async Task RunAsync(Func<Task> action) { if (IsBusy) return; IsBusy = true; try { await action(); } catch (Exception exception) { Status = exception.Message; } finally { IsBusy = false; } }
    private static string? Normalize(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private void NotifyCommands() { On(nameof(AddRequirementText)); LoadCommand.NotifyCanExecuteChanged(); AddCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged(); EditCommand.NotifyCanExecuteChanged(); DeleteCommand.NotifyCanExecuteChanged();MoveUpCommand.NotifyCanExecuteChanged();MoveDownCommand.NotifyCanExecuteChanged(); CancelEditCommand.NotifyCanExecuteChanged(); SelectMessageCommand.NotifyCanExecuteChanged();DuplicateMessageCommand.NotifyCanExecuteChanged();BackCommand.NotifyCanExecuteChanged(); }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; On(name); return true; }
    private void On(string? name) => PropertyChanged?.Invoke(this, new(name));
}
