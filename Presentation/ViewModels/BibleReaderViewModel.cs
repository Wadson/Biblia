using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleReaderViewModel:INotifyPropertyChanged
{
    private const double DefaultFontSize=18, MinimumFontSize=14, MaximumFontSize=30, FontSizeStep=2; private readonly IBibleVersionManager _versions;private readonly IBibleRepository _bible;private readonly ISavedReferenceRepository _saved;private readonly IThemeRepository _themes;private readonly IMessageRepository _messages;private readonly IClipboardService _clipboard;private readonly ISettingsService? _settings;private readonly IAppNavigator _navigator;
    private BibleVersionCatalogEntry? _version;private BibleBook? _book;private int _chapter;private BibleVerse? _start;private BibleVerse? _end;private Theme? _theme;private Message? _message;private SavedReference? _persisted;private string _status="";private string _comparison="";private bool _busy,_awaitingRangeEnd;private double _verseFontSize=DefaultFontSize;private int? _initialBookId,_initialChapter,_initialVerse;private string? _initialVersionCode;
    public BibleReaderViewModel(IBibleVersionManager versions,IBibleRepository bible,ISavedReferenceRepository saved,IThemeRepository themes,IMessageRepository messages,IClipboardService clipboard,IAppNavigator navigator,ISettingsService? settings=null)
    {
        _versions=versions;_bible=bible;_saved=saved;_themes=themes;_messages=messages;_clipboard=clipboard;_navigator=navigator;_settings=settings;
        PreviousChapterCommand=new AsyncCommand(()=>MoveChapterAsync(-1),()=>CanMove(-1));NextChapterCommand=new AsyncCommand(()=>MoveChapterAsync(1),()=>CanMove(1));CopyCommand=new AsyncCommand(CopyAsync,HasSelection);SaveCommand=new AsyncCommand(SaveAsync,HasSelection);ClearSelectionCommand=new AsyncCommand(ClearSelectionAsync,HasSelection);AddThemeCommand=new AsyncCommand(AddThemeAsync,()=>HasSelection()&&SelectedTheme is not null);AddMessageCommand=new AsyncCommand(AddMessageAsync,()=>HasSelection()&&SelectedMessage is not null);CompareCommand=new AsyncCommand(CompareAsync,HasSelection);SelectVerseCommand=new AsyncCommand<BibleVerseItemViewModel>(item=>{SelectVerse(item.Verse);return Task.CompletedTask;});DecreaseFontCommand=new AsyncCommand(()=>SetFontSizeAsync(VerseFontSize-FontSizeStep));ResetFontCommand=new AsyncCommand(()=>SetFontSizeAsync(DefaultFontSize));IncreaseFontCommand=new AsyncCommand(()=>SetFontSizeAsync(VerseFontSize+FontSizeStep));
    }
    public ObservableCollection<BibleVersionCatalogEntry> Versions{get;}=[];public ObservableCollection<BibleBook> Books{get;}=[];public ObservableCollection<int> Chapters{get;}=[];public ObservableCollection<BibleVerse> Verses{get;}=[];public ObservableCollection<BibleVerseItemViewModel> VerseItems{get;}=[];public ObservableCollection<Theme> Themes{get;}=[];public ObservableCollection<Message> Messages{get;}=[];
    public BibleVersionCatalogEntry? SelectedVersion{get=>_version;set{if(Set(ref _version,value))NotifyReaderState();}}
    public BibleBook? SelectedBook{get=>_book;set{if(Set(ref _book,value))NotifyReaderState();}}
    public int SelectedChapter{get=>_chapter;set{if(Set(ref _chapter,value))NotifyReaderState();}}
    public double VerseFontSize{get=>_verseFontSize;private set=>Set(ref _verseFontSize,value);}
    public BibleVerse? SelectionStart{get=>_start;set{if(Set(ref _start,value)){if(_end is null||_end.Verse<value?.Verse)SelectionEnd=value;NotifyReaderState();}}}
    public BibleVerse? SelectionEnd{get=>_end;set{if(Set(ref _end,value))NotifyReaderState();}}
    public Theme? SelectedTheme{get=>_theme;set{if(Set(ref _theme,value))NotifyCommands();}}public Message? SelectedMessage{get=>_message;set{if(Set(ref _message,value))NotifyCommands();}}
    public string Status{get=>_status;private set=>Set(ref _status,value);}public string ReaderTitle=>$"{SelectedBook?.Name ?? "Bíblia"}{(SelectedChapter>0?$" {SelectedChapter}":string.Empty)}";public string ComparisonText{get=>_comparison;private set=>Set(ref _comparison,value);}public bool IsBusy{get=>_busy;private set=>Set(ref _busy,value);}public bool HasSelectionVisible=>SelectionStart is not null&&SelectionEnd is not null;public string SelectedReferenceText=>!HasSelectionVisible?string.Empty:$"{SelectedBook?.Name} {SelectedChapter}:{Math.Min(SelectionStart!.Verse,SelectionEnd!.Verse)}"+(SelectionStart!.Verse==SelectionEnd!.Verse?string.Empty:$"-{Math.Max(SelectionStart.Verse,SelectionEnd.Verse)}");
    public AsyncCommand PreviousChapterCommand{get;}public AsyncCommand NextChapterCommand{get;}public AsyncCommand CopyCommand{get;}public AsyncCommand SaveCommand{get;}public AsyncCommand ClearSelectionCommand{get;}public AsyncCommand AddThemeCommand{get;}public AsyncCommand AddMessageCommand{get;}public AsyncCommand CompareCommand{get;}public AsyncCommand<BibleVerseItemViewModel> SelectVerseCommand{get;}public AsyncCommand DecreaseFontCommand{get;}public AsyncCommand ResetFontCommand{get;}public AsyncCommand IncreaseFontCommand{get;}
    public event PropertyChangedEventHandler? PropertyChanged;

    public void ConfigureInitialReference(string? versionCode,int bookReferenceId,int chapter,int verse){_initialVersionCode=versionCode;_initialBookId=bookReferenceId;_initialChapter=chapter;_initialVerse=verse;}
    public async Task InitializeAsync(){await RunAsync(async()=>{if(double.TryParse(await (_settings?.GetAsync("BibleReaderFontSize")??Task.FromResult<string?>(null)),out var size))VerseFontSize=Math.Clamp(size,MinimumFontSize,MaximumFontSize);var active=await _versions.GetActiveVersionAsync();Versions.Clear();foreach(var v in (await _versions.GetVersionsAsync()).Where(x=>x.IsInstalled&&x.IsEnabled))Versions.Add(v);Themes.Clear();foreach(var x in await _themes.GetAllAsync())Themes.Add(x);Messages.Clear();foreach(var x in await _messages.GetAllAsync())Messages.Add(x);SelectedVersion=Versions.FirstOrDefault(x=>x.Code==_initialVersionCode)??Versions.FirstOrDefault(x=>x.Code==active?.Code)??Versions.FirstOrDefault();if(SelectedVersion is not null){await LoadBooksCoreAsync(_initialBookId,_initialChapter??1);if(_initialVerse is int verse){var selected=Verses.FirstOrDefault(x=>x.Verse==verse);if(selected is not null)SelectVerse(selected);}}});}
    public async Task ChangeVersionAsync()
    {
        if(SelectedVersion is null)return;
        var bookId=SelectedBook?.BookReferenceId;var chapter=SelectedChapter;var start=SelectionStart?.Verse;var end=SelectionEnd?.Verse;
        await RunAsync(async()=>{await _versions.SetActiveVersionAsync(SelectedVersion.Code);await LoadBooksCoreAsync(bookId,chapter);RestoreSelection(start,end);});
    }
    public async Task ChangeBookAsync(){if(SelectedVersion is null||SelectedBook is null)return;await RunAsync(()=>LoadChaptersCoreAsync(SelectedChapter));}
    public async Task ChangeChapterAsync(){if(SelectedVersion is null||SelectedBook is null||SelectedChapter<1)return;await RunAsync(LoadVersesCoreAsync);}
    private async Task LoadBooksCoreAsync(int? preserveBook,int? preserveChapter=null){Books.Clear();foreach(var b in await _bible.GetBooksAsync(SelectedVersion!.Code))Books.Add(b);SelectedBook=Books.FirstOrDefault(x=>x.BookReferenceId==preserveBook)??Books.FirstOrDefault();if(SelectedBook is not null)await LoadChaptersCoreAsync(preserveChapter??1);}
    private async Task LoadChaptersCoreAsync(int preserve){Chapters.Clear();foreach(var c in await _bible.GetChaptersAsync(SelectedVersion!.Code,SelectedBook!.BookReferenceId))Chapters.Add(c);SelectedChapter=Chapters.Contains(preserve)?preserve:Chapters.FirstOrDefault();if(SelectedChapter>0)await LoadVersesCoreAsync();}
    private async Task LoadVersesCoreAsync(){Verses.Clear();VerseItems.Clear();foreach(var v in await _bible.GetVersesAsync(SelectedVersion!.Code,SelectedBook!.BookReferenceId,SelectedChapter)){Verses.Add(v);VerseItems.Add(new(v));}SelectionStart=null;SelectionEnd=null;_awaitingRangeEnd=false;_persisted=null;ComparisonText="";Status="";RefreshVerseSelectionState();NotifyReaderState();}
    private async Task MoveChapterAsync(int delta)
    {
        if(IsBusy||SelectedBook is null)return;
        var chapterIndex=Chapters.IndexOf(SelectedChapter);
        if(chapterIndex+delta>=0&&chapterIndex+delta<Chapters.Count)
        {
            SelectedChapter=Chapters[chapterIndex+delta];
            await ChangeChapterAsync();
            return;
        }

        var bookIndex=Books.IndexOf(SelectedBook);
        if(bookIndex+delta<0||bookIndex+delta>=Books.Count)return;
        SelectedBook=Books[bookIndex+delta];
        await RunAsync(()=>LoadChaptersCoreAsync(delta>0?1:int.MaxValue));
    }
    private async Task SetFontSizeAsync(double value){VerseFontSize=Math.Clamp(value,MinimumFontSize,MaximumFontSize);if(_settings is not null)await _settings.SetAsync("BibleReaderFontSize",VerseFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));}
    private async Task CopyAsync(){await RunAsync(async()=>{await _clipboard.SetTextAsync(FormatSelection());Status="Texto copiado.";});}
    private async Task SaveAsync(){await RunAsync(async()=>{await EnsureSavedAsync();Status="Referência salva.";});}
    private Task ClearSelectionAsync(){SelectionStart=null;SelectionEnd=null;_awaitingRangeEnd=false;_persisted=null;ComparisonText="";Status="Seleção limpa.";RefreshVerseSelectionState();return Task.CompletedTask;}
    private async Task AddThemeAsync(){await RunAsync(async()=>{var item=await EnsureSavedAsync();await _saved.AddThemeAsync(item.Id,SelectedTheme!.Id);Status=$"Referência adicionada ao tema {SelectedTheme.Name}.";});}
    private async Task AddMessageAsync(){await RunAsync(async()=>{var item=await EnsureSavedAsync();var order=await _messages.GetNextReferenceOrderAsync(SelectedMessage!.Id);await _messages.AddReferenceAsync(new MessageReference(0,SelectedMessage.Id,item.Id,null,order,null,SelectedVersion!.Id));Status=$"Referência adicionada à mensagem {SelectedMessage.Title}.";});}

    private async Task CompareAsync()
    {
        if(SelectionStart is null||SelectionEnd is null||SelectedBook is null){Status="Selecione um versículo ou intervalo para comparar.";return;}
        var first=Math.Min(SelectionStart.Verse,SelectionEnd.Verse);var last=Math.Max(SelectionStart.Verse,SelectionEnd.Verse);
        try{await _navigator.GoToAsync("BibleComparison",new Dictionary<string,object>{{"BookReferenceId",SelectedBook.BookReferenceId},{"BookName",SelectedBook.Name},{"Chapter",SelectedChapter},{"VerseStart",first},{"VerseEnd",last},{"SourceVersionCode",SelectedVersion?.Code??string.Empty}});}catch(Exception){Status="Não foi possível abrir a comparação.";}
    }

    private async Task<SavedReference> EnsureSavedAsync(){if(_persisted is not null)return _persisted;var first=Math.Min(SelectionStart!.Verse,SelectionEnd!.Verse);var last=Math.Max(SelectionStart.Verse,SelectionEnd.Verse);_persisted=await _saved.CreateAsync(new SavedReference(0,SelectedBook!.BookReferenceId,SelectedChapter,first,last,null,SelectedVersion!.Id,default,default));return _persisted;}
    private string FormatSelection(){var first=Math.Min(SelectionStart!.Verse,SelectionEnd!.Verse);var last=Math.Max(SelectionStart.Verse,SelectionEnd.Verse);var texts=Verses.Where(v=>v.Verse>=first&&v.Verse<=last).Select(v=>$"{v.Verse} {v.Text}");return $"{SelectedBook!.Name} {SelectedChapter}:{first}"+(last==first?"":$"-{last}")+$" ({SelectedVersion!.Code})"+Environment.NewLine+string.Join(Environment.NewLine,texts);}
    public void SelectVerse(BibleVerse verse)
    {
        // Nenhuma seleção existente
        if (SelectionStart is null || SelectionEnd is null)
        {
            _start = verse;
            _end = verse;
            _awaitingRangeEnd = true;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(SelectionStart)));

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(SelectionEnd)));

            RefreshVerseSelectionState();NotifyReaderState();

            return;
        }

        var first = Math.Min(
            SelectionStart.Verse,
            SelectionEnd.Verse);

        var last = Math.Max(
            SelectionStart.Verse,
            SelectionEnd.Verse);

        // Tocou novamente no único versículo selecionado:
        // remove a seleção.
        if (first == last &&
            verse.Verse == first)
        {
            _start = null;
            _end = null;
            _awaitingRangeEnd = false;
            _persisted = null;

            ComparisonText = string.Empty;
            Status = string.Empty;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(SelectionStart)));

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(SelectionEnd)));

            RefreshVerseSelectionState();NotifyReaderState();

            return;
        }

        // Está esperando o final do intervalo
        if (_awaitingRangeEnd)
        {
            _end = verse;
            _awaitingRangeEnd = false;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(SelectionEnd)));

            RefreshVerseSelectionState();NotifyReaderState();

            return;
        }

        // Nova seleção
        _start = verse;
        _end = verse;
        _awaitingRangeEnd = true;
        _persisted = null;

        ComparisonText = string.Empty;

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(SelectionStart)));

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(SelectionEnd)));

        RefreshVerseSelectionState();NotifyReaderState();
    }
    private bool HasSelection()=>SelectionStart is not null&&SelectionEnd is not null&&!IsBusy;
    private bool CanMove(int d)
    {
        if(IsBusy||SelectedBook is null)return false;
        var chapterIndex=Chapters.IndexOf(SelectedChapter);
        if(chapterIndex<0)return false;
        return chapterIndex+d>=0&&chapterIndex+d<Chapters.Count||Books.IndexOf(SelectedBook)+d>=0&&Books.IndexOf(SelectedBook)+d<Books.Count;
    }
    private async Task RunAsync(Func<Task> action){if(IsBusy)return;IsBusy=true;try{await action();}catch(Exception ex){Status=ex.Message;}finally{IsBusy=false;NotifyCommands();}}
    private void RestoreSelection(int? startVerse,int? endVerse)
    {
        if(startVerse is null)return;
        var start=Verses.FirstOrDefault(x=>x.Verse==startVerse.Value);var end=Verses.FirstOrDefault(x=>x.Verse==endVerse.GetValueOrDefault(startVerse.Value));
        if(start is null||end is null){Status="O intervalo não está disponível nesta versão.";return;}
        _start=start;_end=end;_awaitingRangeEnd=false;PropertyChanged?.Invoke(this,new(nameof(SelectionStart)));PropertyChanged?.Invoke(this,new(nameof(SelectionEnd)));RefreshVerseSelectionState();NotifyReaderState();
    }
    private void RefreshVerseSelectionState()
    {
        if(SelectionStart is null||SelectionEnd is null){foreach(var item in VerseItems)item.SetSelected(false);return;}
        var first=Math.Min(SelectionStart.Verse,SelectionEnd.Verse);var last=Math.Max(SelectionStart.Verse,SelectionEnd.Verse);
        foreach(var item in VerseItems)item.SetSelected(item.Number>=first&&item.Number<=last);
    }
    private void NotifyReaderState(){NotifyCommands();PropertyChanged?.Invoke(this,new(nameof(ReaderTitle)));PropertyChanged?.Invoke(this,new(nameof(HasSelectionVisible)));PropertyChanged?.Invoke(this,new(nameof(SelectedReferenceText)));}
    private void NotifyCommands(){PreviousChapterCommand.NotifyCanExecuteChanged();NextChapterCommand.NotifyCanExecuteChanged();CopyCommand.NotifyCanExecuteChanged();SaveCommand.NotifyCanExecuteChanged();ClearSelectionCommand.NotifyCanExecuteChanged();AddThemeCommand.NotifyCanExecuteChanged();AddMessageCommand.NotifyCanExecuteChanged();CompareCommand.NotifyCanExecuteChanged();}
    private bool Set<T>(ref T field,T value,[CallerMemberName]string? name=null){if(EqualityComparer<T>.Default.Equals(field,value))return false;field=value;PropertyChanged?.Invoke(this,new(name));return true;}
}
