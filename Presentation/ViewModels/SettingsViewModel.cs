using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed record InstalledVersionItem(string Code,string DisplayName,bool IsEnabled,bool IsActive);

public sealed class SettingsViewModel:INotifyPropertyChanged
{
    private readonly ISettingsService _settings;private readonly IBibleVersionManager _versions;private readonly IBackupService _backup;private readonly IFileTransferService _files;private readonly IAppInfoService _appInfo;private readonly IAppNavigator _navigator;
    private bool _darkMode,_busy,_aboutVisible,_restoreConfirmationVisible;private string _status="";private BackupInfo? _lastBackup;private string? _pendingRestorePath;
    public SettingsViewModel(ISettingsService settings,IBibleVersionManager versions,IBackupService backup,IFileTransferService files,IAppInfoService appInfo,IAppNavigator navigator)
    {
        _settings=settings;_versions=versions;_backup=backup;_files=files;_appInfo=appInfo;_navigator=navigator;
        LoadCommand=new(LoadAsync,()=>!IsBusy);SaveCommand=new(SaveAsync,()=>!IsBusy);SaveBackupAsCommand=new(SaveBackupAsAsync,()=>!IsBusy);ShareBackupCommand=new(ShareBackupAsync,()=>!IsBusy);RestoreBackupCommand=new(ChooseRestoreAsync,()=>!IsBusy);ConfirmRestoreCommand=new(ConfirmRestoreAsync,()=>RestoreConfirmationVisible&&!IsBusy);CancelRestoreCommand=new(()=>{RestoreConfirmationVisible=false;_pendingRestorePath=null;return Task.CompletedTask;});OpenVersionsCommand=new(()=>_navigator.GoToAsync("//BibleVersions"));OpenAboutCommand=new(()=>{AboutVisible=true;return Task.CompletedTask;});CloseAboutCommand=new(()=>{AboutVisible=false;return Task.CompletedTask;});
    }
    public ObservableCollection<InstalledVersionItem> InstalledVersions{get;}=[];
    public bool DarkMode{get=>_darkMode;set{if(Set(ref _darkMode,value))Microsoft.Maui.Controls.Application.Current!.UserAppTheme=value?AppTheme.Dark:AppTheme.Light;}}
    public bool IsBusy{get=>_busy;private set{if(Set(ref _busy,value))NotifyCommands();}}
    public string Status{get=>_status;private set=>Set(ref _status,value);}
    public BackupInfo? LastBackup{get=>_lastBackup;private set{if(Set(ref _lastBackup,value)){On(nameof(HasBackup));On(nameof(LastBackupDescription));NotifyCommands();}}}
    public bool HasBackup=>LastBackup is not null;
    public string LastBackupDescription=>LastBackup is null?"Nenhum backup criado nesta sessão.":$"{LastBackup.CreatedAt.ToLocalTime():dd/MM/yyyy • HH:mm}\n{Path.GetFileName(LastBackup.Path)} • {FormatSize(LastBackup.SizeBytes)}";
    public int InstalledVersionsCount=>InstalledVersions.Count;
    public string InstalledVersionsDescription=>InstalledVersions.Count==1?"1 versão instalada":$"{InstalledVersions.Count} versões instaladas";
    public bool AboutVisible{get=>_aboutVisible;private set=>Set(ref _aboutVisible,value);}
    public bool RestoreConfirmationVisible{get=>_restoreConfirmationVisible;private set{if(Set(ref _restoreConfirmationVisible,value))NotifyCommands();}}
    public string AppVersion=>$"Versão {_appInfo.Version}";public string AppBuild=>$"Build {_appInfo.Build}";
    public AsyncCommand LoadCommand{get;}public AsyncCommand SaveCommand{get;}public AsyncCommand SaveBackupAsCommand{get;}public AsyncCommand ShareBackupCommand{get;}public AsyncCommand RestoreBackupCommand{get;}public AsyncCommand ConfirmRestoreCommand{get;}public AsyncCommand CancelRestoreCommand{get;}public AsyncCommand OpenVersionsCommand{get;}public AsyncCommand OpenAboutCommand{get;}public AsyncCommand CloseAboutCommand{get;}
    public event PropertyChangedEventHandler? PropertyChanged;
    private async Task LoadAsync()=>await RunAsync(async()=>{DarkMode="dark".Equals(await _settings.GetAsync("appearance"),StringComparison.OrdinalIgnoreCase);await _versions.InitializeCatalogAsync();var active=await _versions.GetActiveVersionAsync();InstalledVersions.Clear();foreach(var item in (await _versions.GetVersionsAsync()).Where(x=>x.IsInstalled))InstalledVersions.Add(new(item.Code,item.DisplayName,item.IsEnabled,item.Code==active?.Code));On(nameof(InstalledVersionsCount));On(nameof(InstalledVersionsDescription));Status="Configurações carregadas.";});
    private async Task SaveAsync()=>await RunAsync(async()=>{await _settings.SetAsync("appearance",DarkMode?"dark":"light");Status="Preferência de aparência salva.";});
    private async Task SaveBackupAsAsync()=>await RunAsync(async()=>{LastBackup=await _backup.CreateAsync();var outcome=await _files.SaveCopyAsync(LastBackup.Path);Status=outcome==FileSaveOutcome.Saved?"Cópia do backup salva com sucesso.":"Salvamento cancelado. O backup interno foi preservado.";});
    private async Task ShareBackupAsync()=>await RunAsync(async()=>{LastBackup=await _backup.CreateAsync();await _files.ShareAsync(LastBackup.Path);Status="Compartilhamento aberto. Escolha WhatsApp, Telegram ou outro aplicativo compatível.";});
    private async Task ChooseRestoreAsync()=>await RunAsync(async()=>{var path=await _files.PickBackupAsync();if(path is null){Status="Operação cancelada.";return;}try{await _backup.ValidateAsync(path);_pendingRestorePath=path;RestoreConfirmationVisible=true;Status="Backup válido. Confirme a restauração.";}catch(InvalidDataException){Status="Este arquivo não é um backup válido do BíbliaTema.";}});
    private async Task ConfirmRestoreAsync()=>await RunAsync(async()=>{if(_pendingRestorePath is null)return;await _backup.RestoreAsync(_pendingRestorePath);RestoreConfirmationVisible=false;_pendingRestorePath=null;Status="Backup restaurado. As telas carregarão novamente os dados restaurados.";});
    private async Task RunAsync(Func<Task> action){if(IsBusy)return;IsBusy=true;try{await action();}catch(OperationCanceledException){Status="Operação cancelada.";}catch(Exception ex){Status=$"Não foi possível concluir a operação. {ex.Message}";}finally{IsBusy=false;}}
    private void NotifyCommands(){LoadCommand.NotifyCanExecuteChanged();SaveCommand.NotifyCanExecuteChanged();SaveBackupAsCommand.NotifyCanExecuteChanged();ShareBackupCommand.NotifyCanExecuteChanged();RestoreBackupCommand.NotifyCanExecuteChanged();ConfirmRestoreCommand.NotifyCanExecuteChanged();}
    private static string FormatSize(long bytes)=>bytes<1024*1024?$"{bytes/1024d:0.0} KB":$"{bytes/1024d/1024d:0.0} MB";
    private bool Set<T>(ref T field,T value,[CallerMemberName]string? name=null){if(EqualityComparer<T>.Default.Equals(field,value))return false;field=value;On(name);return true;}private void On(string? name)=>PropertyChanged?.Invoke(this,new(name));
}
