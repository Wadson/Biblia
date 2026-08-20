using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class ThemeSelection(Theme theme) : INotifyPropertyChanged
{
    private bool _isSelected;
    public Theme Theme { get; } = theme;
    public string Name => Theme.Name;
    public bool IsSelected { get => _isSelected; set { if (_isSelected == value) return; _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed record SavedReferenceListItem(SavedReferenceDetails Details, string DisplayReference)
{
    public string Comment => Details.Reference.Comment ?? "Sem comentário permanente.";
    public string Themes => Details.Themes.Count == 0 ? "Sem temas" : string.Join(", ", Details.Themes.Select(theme => theme.Name));
}

public sealed class SavedReferencesViewModel : INotifyPropertyChanged
{
    private readonly ISavedReferenceService _service; private readonly IBibleVersionManager _versions; private readonly IBibleRepository _bible; private readonly IThemeRepository _themes;
    private SavedReferenceListItem? _selected; private BibleVersionCatalogEntry? _version; private BibleBook? _book; private int _chapter; private int _verseStart; private int _verseEnd; private string _comment = string.Empty; private string _search = string.Empty; private string _status = string.Empty; private string _preview = string.Empty; private bool _busy;
    public SavedReferencesViewModel(ISavedReferenceService service, IBibleVersionManager versions, IBibleRepository bible, IThemeRepository themes)
    { _service = service; _versions = versions; _bible = bible; _themes = themes; LoadCommand = new AsyncCommand(InitializeAsync, () => !IsBusy); SearchCommand = new AsyncCommand(SearchAsync, () => !IsBusy); NewCommand = new AsyncCommand(NewAsync, () => !IsBusy); SaveCommand = new AsyncCommand(SaveAsync, CanSave); DeleteCommand = new AsyncCommand(DeleteAsync, () => SelectedItem is not null && !IsBusy); }
    public ObservableCollection<SavedReferenceListItem> Items { get; } = []; public ObservableCollection<BibleVersionCatalogEntry> Versions { get; } = []; public ObservableCollection<BibleBook> Books { get; } = []; public ObservableCollection<int> Chapters { get; } = []; public ObservableCollection<int> Verses { get; } = []; public ObservableCollection<ThemeSelection> Themes { get; } = [];
    public SavedReferenceListItem? SelectedItem { get => _selected; set { if (!Set(ref _selected, value)) return; _ = LoadSelectedAsync(); Notify(); } }
    public BibleVersionCatalogEntry? SelectedVersion { get => _version; set => Set(ref _version, value); } public BibleBook? SelectedBook { get => _book; set { if(Set(ref _book,value))OnChanged(nameof(FormattedReference)); } } public int SelectedChapter { get => _chapter; set { if(Set(ref _chapter,value))OnChanged(nameof(FormattedReference)); } } public int VerseStart { get => _verseStart; set { if(Set(ref _verseStart,value))OnChanged(nameof(FormattedReference)); } } public int VerseEnd { get => _verseEnd; set { if(Set(ref _verseEnd,value))OnChanged(nameof(FormattedReference)); } } public string Comment { get => _comment; set => Set(ref _comment, value); } public string SearchText { get => _search; set => Set(ref _search, value); } public string Status { get => _status; private set => Set(ref _status, value); } public string PreviewText { get => _preview; private set => Set(ref _preview,value); } public string FormattedReference => SelectedBook is null || SelectedChapter<1 || VerseStart<1 ? "Selecione o trecho." : $"{SelectedBook.Name} {SelectedChapter}:{VerseStart}"+(VerseEnd==VerseStart?string.Empty:$"-{VerseEnd}"); public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) Notify(); } } public bool IsEmpty => !IsBusy && Items.Count == 0; public string EditorTitle => SelectedItem is null ? "Nova referência" : "Editar referência";
    public AsyncCommand LoadCommand { get; } public AsyncCommand SearchCommand { get; } public AsyncCommand NewCommand { get; } public AsyncCommand SaveCommand { get; } public AsyncCommand DeleteCommand { get; } public event PropertyChangedEventHandler? PropertyChanged;
    public async Task InitializeAsync() => await RunAsync(async () => { await _versions.InitializeCatalogAsync(); foreach (var item in (await _versions.GetVersionsAsync()).Where(item => item.IsInstalled && item.IsEnabled)) Versions.Add(item); SelectedVersion = await _versions.GetActiveVersionAsync() ?? Versions.FirstOrDefault(); foreach (var theme in await _themes.GetAllAsync()) Themes.Add(new ThemeSelection(theme)); if (SelectedVersion is not null) await LoadBooksAsync(); await LoadItemsAsync(); });
    public async Task ChangeVersionAsync() { if (SelectedVersion is not null) await RunAsync(LoadBooksAsync); }
    public async Task ChangeBookAsync() { if (SelectedVersion is not null && SelectedBook is not null) await RunAsync(LoadChaptersAsync); }
    public async Task ChangeChapterAsync() { if (SelectedVersion is not null && SelectedBook is not null && SelectedChapter > 0) await RunAsync(LoadVersesAsync); }
    public async Task RefreshPreviewAsync() { if (SelectedVersion is null || SelectedBook is null || SelectedChapter < 1 || VerseStart < 1 || VerseEnd < VerseStart) return; await RunAsync(LoadPreviewAsync); }
    private async Task LoadBooksAsync() { Books.Clear(); foreach (var book in await _bible.GetBooksAsync(SelectedVersion!.Code)) Books.Add(book); SelectedBook = Books.FirstOrDefault(); if (SelectedBook is not null) await LoadChaptersAsync(); }
    private async Task LoadChaptersAsync() { Chapters.Clear(); foreach (var chapter in await _bible.GetChaptersAsync(SelectedVersion!.Code, SelectedBook!.BookReferenceId)) Chapters.Add(chapter); SelectedChapter = Chapters.FirstOrDefault(); await LoadVersesAsync(); }
    private async Task LoadVersesAsync() { var values=await _bible.GetVersesAsync(SelectedVersion!.Code, SelectedBook!.BookReferenceId, SelectedChapter);Verses.Clear(); foreach (var verse in values) Verses.Add(verse.Verse); VerseStart = Verses.FirstOrDefault(); VerseEnd = VerseStart; await LoadPreviewAsync(); }
    private async Task LoadPreviewAsync() { var passage = await _bible.GetPassageAsync(SelectedVersion!.Code, SelectedBook!.BookReferenceId, SelectedChapter, VerseStart, VerseEnd); PreviewText = passage.Verses.Count == 0 ? "Referência não encontrada nesta versão." : string.Join(Environment.NewLine, passage.Verses.Select(verse => $"{verse.Verse}  {verse.Text}")); }
    private async Task SearchAsync() => await RunAsync(() => LoadItemsAsync());
    private Task NewAsync() { SelectedItem = null; Comment = string.Empty; foreach (var theme in Themes) theme.IsSelected = false; if (Verses.Count > 0) { VerseStart = Verses[0]; VerseEnd = VerseStart; } Status = "Selecione o trecho e os temas."; OnChanged(nameof(EditorTitle)); return Task.CompletedTask; }
    private async Task SaveAsync() => await RunAsync(async () => { var item = await _service.SaveAsync(SelectedItem?.Details.Reference.Id, SelectedBook!.BookReferenceId, SelectedChapter, VerseStart, VerseEnd, Comment, SelectedVersion?.Id, Themes.Where(theme => theme.IsSelected).Select(theme => theme.Theme.Id).ToArray()); await LoadItemsAsync(item.Reference.Id); Status = "Referência salva."; });
    private async Task DeleteAsync() => await RunAsync(async () => { await _service.DeleteAsync(SelectedItem!.Details.Reference.Id); await NewAsync(); await LoadItemsAsync(); Status = "Referência excluída."; });
    private async Task LoadItemsAsync(long? selectedId = null) { var data = await _service.SearchAsync(SearchText); Items.Clear(); foreach (var detail in data) Items.Add(new SavedReferenceListItem(detail, Format(detail.Reference))); OnChanged(nameof(IsEmpty)); var selected = Items.FirstOrDefault(item => item.Details.Reference.Id == selectedId); if (selected is not null) SelectedItem = selected; }
    private async Task LoadSelectedAsync() { if (SelectedItem is null) return; var detail = await _service.GetDetailsAsync(SelectedItem.Details.Reference.Id); if (detail is null) return; var reference = detail.Reference; SelectedVersion = Versions.FirstOrDefault(version => version.Id == reference.PreferredBibleVersionId) ?? SelectedVersion; if (SelectedVersion is not null) { await LoadBooksAsync(); SelectedBook = Books.FirstOrDefault(book => book.BookReferenceId == reference.BookReferenceId); if (SelectedBook is not null) { await LoadChaptersAsync(); SelectedChapter = reference.Chapter; await LoadVersesAsync(); } } VerseStart = reference.VerseStart; VerseEnd = reference.VerseEnd; Comment = reference.Comment ?? string.Empty; foreach (var theme in Themes) theme.IsSelected = detail.Themes.Any(linked => linked.Id == theme.Theme.Id); OnChanged(nameof(EditorTitle)); }
    private string Format(SavedReference reference) { var book = Books.FirstOrDefault(item => item.BookReferenceId == reference.BookReferenceId)?.Name ?? $"Livro {reference.BookReferenceId}"; return $"{book} {reference.Chapter}:{reference.VerseStart}" + (reference.VerseEnd == reference.VerseStart ? string.Empty : $"-{reference.VerseEnd}"); }
    private bool CanSave() => !IsBusy && SelectedBook is not null && SelectedChapter > 0 && VerseStart > 0 && VerseEnd >= VerseStart;
    private async Task RunAsync(Func<Task> action) { if (IsBusy) return; IsBusy = true; try { await action(); } catch (Exception ex) { Status = ex.Message; } finally { IsBusy = false; OnChanged(nameof(IsEmpty)); } }
    private void Notify() { LoadCommand.NotifyCanExecuteChanged(); SearchCommand.NotifyCanExecuteChanged(); NewCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged(); DeleteCommand.NotifyCanExecuteChanged(); }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; OnChanged(name); return true; } private void OnChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
