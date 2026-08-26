using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class ReportsViewModel : INotifyPropertyChanged
{
    private readonly IReportService _reports; private readonly IMessageService _messages; private readonly IThemeService _themes; private readonly IBibleVersionManager _versions; private readonly IPdfService _pdf; private readonly IFileTransferService _files;
    private Message? _selectedMessage; private BibleVersionCatalogEntry? _selectedVersion; private ReportsOverview? _overview; private SermonReport? _report; private string _status = "", _themeSearch = "", _title = "Pregação", _subtitle = "", _introduction = "", _conclusion = "", _generatedPdfPath = ""; private bool _busy, _themeMode, _exportVisible;
    private readonly List<ReportThemeOption> _allThemes = []; private bool _isSermon = true;

    public ObservableCollection<Message> Messages { get; } = []; public ObservableCollection<ReportThemeOption> Themes { get; } = []; public ObservableCollection<BibleVersionCatalogEntry> BibleVersions { get; } = [];
    public Message? SelectedMessage { get => _selectedMessage; set { if (Set(ref _selectedMessage, value)) { if (!ThemeMode) UpdateAutomaticTitle(); Invalidate(); } } }
    public BibleVersionCatalogEntry? SelectedVersion { get => _selectedVersion; set { if (Set(ref _selectedVersion, value)) Invalidate(); } }
    public ReportsOverview? Overview { get => _overview; private set => Set(ref _overview, value); }
    public SermonReport? Report { get => _report; private set { if (Set(ref _report, value)) { On(nameof(HasPreview)); On(nameof(ShowEmptyPreview)); PdfCommand.NotifyCanExecuteChanged(); } } }
    public bool HasPreview => Report is not null; public bool ShowEmptyPreview => !HasPreview; public bool IsMessageMode => !ThemeMode; public bool IsSermon { get => _isSermon; set { if (Set(ref _isSermon, value)) { On(nameof(IsMessageDocument)); Invalidate(); UpdateAutomaticTitle(); } } } public bool IsMessageDocument => !IsSermon;
    public bool ThemeMode { get => _themeMode; set { if (Set(ref _themeMode, value)) { On(nameof(IsMessageMode)); Invalidate(); UpdateAutomaticTitle(); } } }
    public string ThemeSearch { get => _themeSearch; set { if (Set(ref _themeSearch, value)) RefreshThemeFilter(); } }
    public string Title { get => _title; set { if (Set(ref _title, value)) Invalidate(); } } public string Subtitle { get => _subtitle; set { if (Set(ref _subtitle, value)) Invalidate(); } }
    public string Introduction { get => _introduction; set { if (Set(ref _introduction, value)) Invalidate(); } } public string Conclusion { get => _conclusion; set { if (Set(ref _conclusion, value)) Invalidate(); } }
    public string Status { get => _status; private set => Set(ref _status, value); } public string GeneratedPdfPath { get => _generatedPdfPath; private set { if (Set(ref _generatedPdfPath, value)) NotifyCommands(); } }
    public bool ExportVisible { get => _exportVisible; set => Set(ref _exportVisible, value); }
    public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) NotifyCommands(); } }
    public string SelectedThemesSummary { get { var names = _allThemes.Where(x => x.IsSelected).Select(x => x.Theme.Name).ToArray(); return names.Length == 0 ? "Nenhum tema selecionado" : string.Join(" • ", names); } }
    public AsyncCommand LoadCommand { get; } public AsyncCommand BuildCommand { get; } public AsyncCommand PdfCommand { get; } public AsyncCommand SaveCommand { get; } public AsyncCommand ShareCommand { get; } public ICommand ToggleThemeCommand { get; } public ICommand SelectSermonCommand { get; } public ICommand SelectMessageCommand { get; } public ICommand SelectRegisteredCommand { get; } public ICommand SelectThemesCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public ReportsViewModel(IReportService reports, IMessageService messages, IThemeService themes, IBibleVersionManager versions, IPdfService pdf, IFileTransferService files)
    {
        _reports = reports; _messages = messages; _themes = themes; _versions = versions; _pdf = pdf; _files = files;
        LoadCommand = new(LoadAsync, () => !IsBusy); BuildCommand = new(BuildAsync, CanBuild); PdfCommand = new(CreatePdfAsync, () => Report is not null && !IsBusy);
        SaveCommand = new(SaveAsync, HasPdf); ShareCommand = new(ShareAsync, HasPdf); ToggleThemeCommand = new Command<ReportThemeOption>(ToggleTheme); SelectSermonCommand = new Command(() => IsSermon = true); SelectMessageCommand = new Command(() => IsSermon = false); SelectRegisteredCommand = new Command(() => ThemeMode = false); SelectThemesCommand = new Command(() => ThemeMode = true);
    }

    private async Task LoadAsync() => await RunAsync(async () =>
    {
        Overview = await _reports.GetOverviewAsync(); Messages.Clear(); foreach (var message in await _messages.SearchAsync(null)) Messages.Add(message); SelectedMessage = Messages.FirstOrDefault();
        _allThemes.Clear(); foreach (var theme in await _themes.SearchAsync(null)) _allThemes.Add(new(theme)); RefreshThemeFilter();
        BibleVersions.Clear(); foreach (var version in (await _versions.GetVersionsAsync()).Where(x => x.IsInstalled && x.IsEnabled)) BibleVersions.Add(version);
        SelectedVersion = await _versions.GetActiveVersionAsync() ?? BibleVersions.FirstOrDefault(); Status = "Escolha uma pregação cadastrada ou selecione um ou mais temas.";
    });

    private bool CanBuild() => !IsBusy && !string.IsNullOrWhiteSpace(Title) && (ThemeMode ? _allThemes.Any(x => x.IsSelected) && SelectedVersion is not null : SelectedMessage is not null);
    private async Task BuildAsync() => await RunAsync(async () =>
    {
        var built = ThemeMode
            ? await _reports.BuildFromThemesAsync(new(_allThemes.Where(x => x.IsSelected).Select(x => x.Theme.Id).ToArray(), Title, Subtitle, Introduction, Conclusion, SelectedVersion!.Code))
            : await _reports.BuildFromMessageAsync(SelectedMessage!.Id, Title, Subtitle, Introduction, Conclusion);
        Report = built with { DocumentType = IsSermon ? "PREGAÇÃO" : "MENSAGEM" };
        Status = Report.References.Count == 0 ? "Prévia criada, mas não há referências vinculadas à origem selecionada." : $"Prévia pronta com {Report.References.Count} referência(s).";
    });
    private async Task CreatePdfAsync() => await RunAsync(async () => { GeneratedPdfPath = await _pdf.CreateSermonPdfAsync(Report!); ExportVisible = true; Status = "PDF gerado com sucesso."; });
    private async Task SaveAsync() => await RunAsync(async () => { var outcome=await _files.SaveCopyAsync(GeneratedPdfPath); if(outcome==FileSaveOutcome.Saved){ExportVisible = false; Status = "PDF salvo no destino escolhido.";}else Status="Salvamento cancelado."; });
    private async Task ShareAsync() => await RunAsync(async () => { await _files.ShareAsync(GeneratedPdfPath); ExportVisible = false; Status = "Compartilhamento aberto."; });
    public void CancelExport() => ExportVisible = false;

    private void ToggleTheme(ReportThemeOption? option) { if (option is null) return; option.IsSelected = !option.IsSelected; On(nameof(SelectedThemesSummary)); UpdateAutomaticTitle(); Invalidate(); }
    private void UpdateAutomaticTitle() { var kind = IsSermon ? "Pregação" : "Mensagem"; if (!ThemeMode) { if (SelectedMessage is not null) _title = SelectedMessage.Title; } else { var names = _allThemes.Where(x => x.IsSelected).Select(x => x.Theme.Name).ToArray(); _title = names.Length == 0 ? kind : kind + " — " + JoinNames(names); } On(nameof(Title)); }
    private static string JoinNames(string[] values) => values.Length switch { 0 => "", 1 => values[0], 2 => $"{values[0]} e {values[1]}", _ => string.Join(", ", values[..^1]) + " e " + values[^1] };
    private void RefreshThemeFilter() { var selected = _allThemes.Where(x => x.IsSelected).ToArray(); var filtered = string.IsNullOrWhiteSpace(ThemeSearch) ? _allThemes : _allThemes.Where(x => x.Theme.Name.Contains(ThemeSearch.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList(); Themes.Clear(); foreach (var item in filtered.OrderByDescending(x => x.IsSelected).ThenBy(x => x.Theme.Name)) Themes.Add(item); foreach (var item in selected) if (!Themes.Contains(item)) Themes.Insert(0, item); }
    private void Invalidate() { Report = null; GeneratedPdfPath = ""; ExportVisible = false; NotifyCommands(); }
    private bool HasPdf() => !IsBusy && !string.IsNullOrWhiteSpace(GeneratedPdfPath) && File.Exists(GeneratedPdfPath);
    private void NotifyCommands() { LoadCommand.NotifyCanExecuteChanged(); BuildCommand.NotifyCanExecuteChanged(); PdfCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged(); ShareCommand.NotifyCanExecuteChanged(); }
    private async Task RunAsync(Func<Task> action) { if (IsBusy) return; IsBusy = true; try { await action(); } catch (OperationCanceledException) { Status = "Operação cancelada."; } catch (Exception exception) { Status = exception.Message; } finally { IsBusy = false; } }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; On(name); return true; } private void On([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public sealed class ReportThemeOption(Theme theme) : INotifyPropertyChanged
{
    private bool _selected; public Theme Theme { get; } = theme; public bool IsSelected { get => _selected; set { if (_selected == value) return; _selected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } } public event PropertyChangedEventHandler? PropertyChanged;
}
