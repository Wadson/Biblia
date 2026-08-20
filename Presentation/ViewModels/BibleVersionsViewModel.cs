using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleVersionsViewModel:INotifyPropertyChanged
{
    private readonly IBibleVersionManager _manager;private readonly IBibleVersionImportService _importer;private readonly IBibleFilePicker _picker;
    private BibleVersionCatalogEntry? _selected;private string _status="";private bool _busy;
    public BibleVersionsViewModel(IBibleVersionManager manager,IBibleVersionImportService importer,IBibleFilePicker picker)
    {
        _manager=manager;_importer=importer;_picker=picker;
        LoadCommand=new AsyncCommand(LoadAsync);AddCommand=new AsyncCommand(AddAsync,()=>!IsBusy);
        ValidateCommand=new AsyncCommand(ValidateAsync,()=>SelectedVersion is not null&&!IsBusy);
        ToggleCommand=new AsyncCommand(ToggleAsync,()=>SelectedVersion is not null&&!IsBusy);
        SetDefaultCommand=new AsyncCommand(SetDefaultAsync,()=>SelectedVersion is not null&&!IsBusy);
        RemoveCommand=new AsyncCommand(RemoveAsync,()=>SelectedVersion is{IsBundled:false}&&!IsBusy);
    }
    public ObservableCollection<BibleVersionCatalogEntry> Versions{get;}=[];
    public BibleVersionCatalogEntry? SelectedVersion{get=>_selected;set{if(Set(ref _selected,value))NotifyCommands();}}
    public string Status{get=>_status;private set=>Set(ref _status,value);}
    public bool IsBusy{get=>_busy;private set{if(Set(ref _busy,value))NotifyCommands();}}
    public AsyncCommand LoadCommand{get;}public AsyncCommand AddCommand{get;}public AsyncCommand ValidateCommand{get;}public AsyncCommand ToggleCommand{get;}public AsyncCommand SetDefaultCommand{get;}public AsyncCommand RemoveCommand{get;}
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync()=>await RunAsync(async()=>{await _manager.InitializeCatalogAsync();var selectedCode=SelectedVersion?.Code;Versions.Clear();foreach(var version in await _manager.GetVersionsAsync())Versions.Add(version);SelectedVersion=Versions.FirstOrDefault(x=>x.Code==selectedCode)??Versions.FirstOrDefault();Status=$"{Versions.Count} versões no catálogo.";});
    private async Task AddAsync()=>await RunAsync(async()=>{var path=await _picker.PickAsync();if(path is null){Status="Importação cancelada.";return;}var result=await _importer.ImportAsync(path);Status=result.Message;await LoadCoreAsync(result.Version?.Code);});
    private async Task ValidateAsync()=>await RunAsync(async()=>{var result=await _manager.ValidateAsync(SelectedVersion!.Code);Status=result.Message;await LoadCoreAsync(SelectedVersion.Code);});
    private async Task ToggleAsync()=>await RunAsync(async()=>{await _manager.SetEnabledAsync(SelectedVersion!.Code,!SelectedVersion.IsEnabled);Status=SelectedVersion.IsEnabled?"Versão desativada.":"Versão ativada.";await LoadCoreAsync(SelectedVersion.Code);});
    private async Task SetDefaultAsync()=>await RunAsync(async()=>{await _manager.SetActiveVersionAsync(SelectedVersion!.Code);Status=$"{SelectedVersion.Code} definida como padrão.";});
    private async Task RemoveAsync()=>await RunAsync(async()=>{var code=SelectedVersion!.Code;await _importer.RemoveImportedAsync(code);Status=$"{code} removida.";await LoadCoreAsync(null);});
    private async Task LoadCoreAsync(string? selectedCode){Versions.Clear();foreach(var version in await _manager.GetVersionsAsync())Versions.Add(version);SelectedVersion=Versions.FirstOrDefault(x=>x.Code==selectedCode)??Versions.FirstOrDefault();}
    private async Task RunAsync(Func<Task> action){if(IsBusy)return;IsBusy=true;try{await action();}catch(Exception ex){Status=ex.Message;}finally{IsBusy=false;}}
    private void NotifyCommands(){AddCommand.NotifyCanExecuteChanged();ValidateCommand.NotifyCanExecuteChanged();ToggleCommand.NotifyCanExecuteChanged();SetDefaultCommand.NotifyCanExecuteChanged();RemoveCommand.NotifyCanExecuteChanged();}
    private bool Set<T>(ref T field,T value,[CallerMemberName]string? name=null){if(EqualityComparer<T>.Default.Equals(field,value))return false;field=value;PropertyChanged?.Invoke(this,new(name));return true;}
}
