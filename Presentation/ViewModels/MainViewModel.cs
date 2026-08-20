using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IBibleVersionManager _versionManager;
    private readonly IThemeRepository _themeRepository;
    private readonly ISavedReferenceRepository _savedReferenceRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IDailyVerseService _dailyVerseService;
    private readonly IAppNavigator _navigator;

    private bool _isBusy;
    private string _status = string.Empty;

    private string _activeVersionCode = "—";
    private string _activeVersionName = "Nenhuma versão selecionada";

    private int _installedVersionsCount;
    private int _themesCount;
    private int _referencesCount;
    private int _messagesCount;

    private bool _initialized;
    private DailyVerse? _dailyVerse;
    private bool _isDailyVerseLoading;
    private string _dailyVerseError=string.Empty;

    public MainViewModel(
        IBibleVersionManager versionManager,
        IThemeRepository themeRepository,
        ISavedReferenceRepository savedReferenceRepository,
        IMessageRepository messageRepository,
        IDailyVerseService dailyVerseService,
        IAppNavigator navigator)
    {
        _versionManager = versionManager;
        _themeRepository = themeRepository;
        _savedReferenceRepository = savedReferenceRepository;
        _messageRepository = messageRepository;
        _dailyVerseService=dailyVerseService;
        _navigator=navigator;

        RefreshCommand =
            new AsyncCommand(
                RefreshAsync,
                () => !IsBusy);

        OpenBibleCommand =
            new AsyncCommand(
                OpenBibleAsync,
                () => !IsBusy);

        OpenVersionsCommand =
            new AsyncCommand(
                OpenVersionsAsync,
                () => !IsBusy);

        OpenThemesCommand =
            new AsyncCommand(
                OpenThemesAsync,
                () => !IsBusy);

        OpenMessagesCommand =
            new AsyncCommand(
                OpenMessagesAsync,
                () => !IsBusy);

        OpenSearchCommand =
            new AsyncCommand(
                OpenSearchAsync,
                () => !IsBusy);

        OpenComparisonCommand =
            new AsyncCommand(
                OpenComparisonAsync,
                () => !IsBusy);

        OpenReportsCommand =
            new AsyncCommand(
                OpenReportsAsync,
                () => !IsBusy);
        OpenReferencesCommand=new AsyncCommand(()=>NavigateAsync("//SavedReferences"));
        OpenDailyVerseChapterCommand=new AsyncCommand(OpenDailyVerseChapterAsync,()=>DailyVerse is not null);
        RetryDailyVerseCommand=new AsyncCommand(LoadDailyVerseAsync,()=>!IsDailyVerseLoading);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => _isBusy;

        private set
        {
            if (Set(ref _isBusy, value))
            {
                NotifyCommands();
            }
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            if (Set(ref _status, value))
            {
                OnPropertyChanged(nameof(HasStatus));
            }
        }
    }

    public bool HasStatus =>
        !string.IsNullOrWhiteSpace(Status);
    public DailyVerse? DailyVerse{get=>_dailyVerse;private set{if(Set(ref _dailyVerse,value)){OnPropertyChanged(nameof(HasDailyVerse));OpenDailyVerseChapterCommand.NotifyCanExecuteChanged();}}}
    public bool HasDailyVerse=>DailyVerse is not null;
    public bool IsDailyVerseLoading{get=>_isDailyVerseLoading;private set{if(Set(ref _isDailyVerseLoading,value))RetryDailyVerseCommand.NotifyCanExecuteChanged();}}
    public string DailyVerseError{get=>_dailyVerseError;private set{if(Set(ref _dailyVerseError,value))OnPropertyChanged(nameof(HasDailyVerseError));}}
    public bool HasDailyVerseError=>!string.IsNullOrWhiteSpace(DailyVerseError);

    public string ActiveVersionCode
    {
        get => _activeVersionCode;
        private set => Set(ref _activeVersionCode, value);
    }

    public string ActiveVersionName
    {
        get => _activeVersionName;
        private set => Set(ref _activeVersionName, value);
    }

    public int InstalledVersionsCount
    {
        get => _installedVersionsCount;
        private set
        {
            if (Set(ref _installedVersionsCount, value))
            {
                OnPropertyChanged(
                    nameof(InstalledVersionsDescription));
            }
        }
    }

    public int ThemesCount
    {
        get => _themesCount;
        private set => Set(ref _themesCount, value);
    }

    public int ReferencesCount
    {
        get => _referencesCount;
        private set => Set(ref _referencesCount, value);
    }

    public int MessagesCount
    {
        get => _messagesCount;
        private set => Set(ref _messagesCount, value);
    }

    public string InstalledVersionsDescription =>
        InstalledVersionsCount switch
        {
            0 => "Nenhuma versão instalada.",
            1 => "1 versão instalada.",
            _ => $"{InstalledVersionsCount} versões instaladas."
        };

    public AsyncCommand RefreshCommand { get; }

    public AsyncCommand OpenBibleCommand { get; }

    public AsyncCommand OpenVersionsCommand { get; }

    public AsyncCommand OpenThemesCommand { get; }

    public AsyncCommand OpenMessagesCommand { get; }

    public AsyncCommand OpenSearchCommand { get; }

    public AsyncCommand OpenComparisonCommand { get; }

    public AsyncCommand OpenReportsCommand { get; }
    public AsyncCommand OpenReferencesCommand{get;}
    public AsyncCommand OpenDailyVerseChapterCommand{get;}
    public AsyncCommand RetryDailyVerseCommand{get;}

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        await RunAsync(
            async () =>
            {
                Status = string.Empty;
                await LoadDailyVerseAsync();

                await _versionManager.InitializeCatalogAsync();

                var versions =
                    await _versionManager.GetVersionsAsync();

                var activeVersion =
                    await _versionManager.GetActiveVersionAsync();

                InstalledVersionsCount =
                    versions.Count(
                        x => x.IsInstalled);

                if (activeVersion is not null)
                {
                    ActiveVersionCode =
                        activeVersion.Code;

                    ActiveVersionName =
                        activeVersion.DisplayName;
                }
                else
                {
                    ActiveVersionCode = "—";

                    ActiveVersionName =
                        "Nenhuma versão selecionada";
                }

                var themes =
                    await _themeRepository.GetAllAsync();

                ThemesCount =
                    themes.Count();

                var messages =
                    await _messageRepository.GetAllAsync();

                MessagesCount =
                    messages.Count();

                /*
                 * Se ISavedReferenceRepository já possuir
                 * GetAllAsync(), use diretamente.
                 *
                 * Caso ainda não exista, implemente esse método
                 * no repositório conforme mostrado mais abaixo.
                 */

                ReferencesCount = (await _savedReferenceRepository.SearchAsync(null)).Count;
            });
    }

    private async Task OpenBibleAsync()
    {
        await NavigateAsync("//BibleReader");
    }

    private async Task OpenVersionsAsync()
    {
        await NavigateAsync("//BibleVersions");
    }

    private async Task OpenThemesAsync()
    {
        await NavigateAsync("//Themes");
    }

    private async Task OpenMessagesAsync()
    {
        await NavigateAsync("//MessageBibleReferences");
    }

    private async Task OpenSearchAsync()
    {
        await NavigateAsync("//BibleSearch");
    }

    private async Task OpenComparisonAsync()
    {
        await NavigateAsync("//BibleComparison");
    }

    private async Task OpenReportsAsync()
    {
        await NavigateAsync("//Reports");
    }
    private async Task LoadDailyVerseAsync(){if(IsDailyVerseLoading)return;IsDailyVerseLoading=true;DailyVerseError=string.Empty;try{DailyVerse=await _dailyVerseService.GetDailyVerseAsync();if(DailyVerse is null)DailyVerseError="Nenhuma Bíblia disponível.";}catch(Exception){DailyVerse=null;DailyVerseError="Não foi possível carregar o versículo do dia.";}finally{IsDailyVerseLoading=false;}}
    private Task OpenDailyVerseChapterAsync()=>DailyVerse is null?Task.CompletedTask:_navigator.GoToAsync("//BibleReader",new Dictionary<string,object>{{"BookReferenceId",DailyVerse.BookReferenceId},{"Chapter",DailyVerse.Chapter},{"Verse",DailyVerse.Verse},{"VersionCode",DailyVerse.BibleVersionCode}});

    private static async Task NavigateAsync(
        string route)
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(route);
    }

    private async Task NavigateAsync(
        string route,
        string unavailableMessage)
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch
        {
            Status = unavailableMessage;
        }
    }

    private async Task RunAsync(
        Func<Task> action)
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Status =
                $"Não foi possível carregar o painel. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyCommands()
    {
        RefreshCommand.NotifyCanExecuteChanged();

        OpenBibleCommand.NotifyCanExecuteChanged();
        OpenVersionsCommand.NotifyCanExecuteChanged();

        OpenThemesCommand.NotifyCanExecuteChanged();
        OpenMessagesCommand.NotifyCanExecuteChanged();

        OpenSearchCommand.NotifyCanExecuteChanged();
        OpenComparisonCommand.NotifyCanExecuteChanged();

        OpenReportsCommand.NotifyCanExecuteChanged();
        OpenReferencesCommand.NotifyCanExecuteChanged();OpenDailyVerseChapterCommand.NotifyCanExecuteChanged();RetryDailyVerseCommand.NotifyCanExecuteChanged();
    }

    private bool Set<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(
                field,
                value))
        {
            return false;
        }

        field = value;

        OnPropertyChanged(propertyName);

        return true;
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
