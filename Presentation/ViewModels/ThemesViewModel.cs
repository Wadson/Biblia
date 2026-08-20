using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;
using Biblia.Presentation.Models;

namespace Biblia.Presentation.ViewModels;

public sealed class ThemesViewModel : INotifyPropertyChanged
{
    private readonly IThemeService _themes; private Theme? _selectedTheme; private string _searchText="",_name="",_description="",_status="",_validationError="";
    private string _colorHex="#173A63"; private ThemeColorOption? _selectedColor; private bool _isBusy,_deleteConfirmationVisible;
    public ObservableCollection<Theme> Items { get; }=[]; public ObservableCollection<ThemeColorOption> ColorOptions { get; }=[];
    public event PropertyChangedEventHandler? PropertyChanged;
    public ThemesViewModel(IThemeService themes)
    {
        _themes=themes;
        foreach(var color in Palette) ColorOptions.Add(color);
        SelectColorForHex(_colorHex);
        LoadCommand=new AsyncCommand(LoadAsync,()=>!IsBusy); SearchCommand=new AsyncCommand(SearchAsync,()=>!IsBusy); NewCommand=new AsyncCommand(NewAsync,()=>!IsBusy);
        SaveCommand=new AsyncCommand(SaveAsync,CanSave); CancelCommand=new AsyncCommand(NewAsync,()=>!IsBusy); DeleteCommand=new AsyncCommand(RequestDeleteAsync,()=>IsEditMode&&!IsBusy);
        ConfirmDeleteCommand=new AsyncCommand(DeleteAsync,()=>IsEditMode&&!IsBusy); CancelDeleteCommand=new AsyncCommand(CancelDeleteAsync);
    }
    private static IReadOnlyList<ThemeColorOption> Palette { get; }=[
        new("Azul profundo","#173A63"),new("Azul","#2563EB"),new("Azul claro","#3B82F6"),new("Azul petróleo","#0F766E"),new("Verde","#2E9B63"),new("Esmeralda","#059669"),new("Verde oliva","#6B7A2A"),new("Verde água","#14B8A6"),new("Amarelo","#EAB308"),new("Dourado","#D4A72C"),new("Laranja","#F97316"),new("Coral","#F97370"),new("Vermelho","#DC2626"),new("Vinho","#9F1239"),new("Rosa","#EC4899"),new("Rosa escuro","#BE185D"),new("Roxo","#7C3AED"),new("Lilás","#A78BFA"),new("Marrom","#92400E"),new("Cinza","#64748B"),new("Grafite","#334155")];
    public Theme? SelectedTheme { get=>_selectedTheme; set { if(Set(ref _selectedTheme,value)){Notify();_ = LoadDetailsAsync();OnPropertyChanged(nameof(EditorTitle));OnPropertyChanged(nameof(IsEditMode));} } }
    public string SearchText {get=>_searchText;set=>Set(ref _searchText,value);}
    public string Name {get=>_name;set{if(Set(ref _name,value)){ValidationError=string.Empty;Notify();}}}
    public string Description {get=>_description;set=>Set(ref _description,value);}
    public string ColorHex {get=>_colorHex; private set=>Set(ref _colorHex,value);}
    public ThemeColorOption? SelectedColor {get=>_selectedColor;set{if(Set(ref _selectedColor,value)&&value is not null){ColorHex=value.Hex;OnPropertyChanged(nameof(SelectedColorName));}}}
    public string Status {get=>_status;private set=>Set(ref _status,value);} public string ValidationError {get=>_validationError;private set=>Set(ref _validationError,value);}
    public bool IsBusy {get=>_isBusy;private set{if(Set(ref _isBusy,value))Notify();}} public bool IsEditMode=>SelectedTheme is not null; public bool DeleteConfirmationVisible {get=>_deleteConfirmationVisible;private set=>Set(ref _deleteConfirmationVisible,value);}
    public string EditorTitle=>IsEditMode?"Editar tema":"Novo tema"; public string SelectedColorName=>SelectedColor?.Name??"Cor personalizada";
    public AsyncCommand LoadCommand{get;} public AsyncCommand SearchCommand{get;} public AsyncCommand NewCommand{get;} public AsyncCommand SaveCommand{get;} public AsyncCommand CancelCommand{get;} public AsyncCommand DeleteCommand{get;} public AsyncCommand ConfirmDeleteCommand{get;} public AsyncCommand CancelDeleteCommand{get;}
    public async Task LoadAsync()=>await Run(async()=>{await LoadItemsAsync(SelectedTheme?.Id);Status=Items.Count==0?"Nenhum tema cadastrado.":"";});
    private async Task SearchAsync()=>await Run(()=>LoadItemsAsync(SelectedTheme?.Id));
    private Task NewAsync(){SelectedTheme=null;Name="";Description="";ColorHex="#173A63";SelectColorForHex(ColorHex);DeleteConfirmationVisible=false;Status="Preencha os dados do novo tema.";return Task.CompletedTask;}
    private async Task SaveAsync()=>await Run(async()=>{if(string.IsNullOrWhiteSpace(Name)){ValidationError="Informe o nome do tema.";return;}var saved=await _themes.SaveAsync(SelectedTheme?.Id,Name,ColorHex,Description);await LoadItemsAsync(saved.Id);Status="Tema salvo.";});
    private Task RequestDeleteAsync(){DeleteConfirmationVisible=true;return Task.CompletedTask;} private Task CancelDeleteAsync(){DeleteConfirmationVisible=false;return Task.CompletedTask;}
    private async Task DeleteAsync()=>await Run(async()=>{var name=SelectedTheme!.Name;await _themes.DeleteAsync(SelectedTheme.Id);await LoadItemsAsync(null);await NewAsync();Status=$"Tema '{name}' excluído.";});
    private async Task LoadItemsAsync(long? id){var entries=await _themes.SearchAsync(SearchText);Items.Clear();foreach(var e in entries)Items.Add(e);var selected=id is null?null:Items.FirstOrDefault(x=>x.Id==id);if(selected is not null)SelectedTheme=selected;else if(SelectedTheme is not null)await NewAsync();}
    private async Task LoadDetailsAsync(){var selected=SelectedTheme;if(selected is null)return;try{var details=await _themes.GetDetailsAsync(selected.Id);if(details is null||SelectedTheme?.Id!=selected.Id)return;Name=details.Theme.Name;Description=details.Theme.Description??"";ColorHex=details.Theme.ColorHex??"#173A63";SelectColorForHex(ColorHex);}catch(Exception ex){Status=ex.Message;}}
    private void SelectColorForHex(string hex){var option=ColorOptions.FirstOrDefault(x=>string.Equals(x.Hex,hex,StringComparison.OrdinalIgnoreCase));if(option is null){option=new ThemeColorOption("Cor personalizada",hex,true);ColorOptions.Insert(0,option);}SelectedColor=option;OnPropertyChanged(nameof(SelectedColorName));}
    private bool CanSave()=>!IsBusy&&!string.IsNullOrWhiteSpace(Name); private async Task Run(Func<Task> action){if(IsBusy)return;IsBusy=true;try{await action();}catch(Exception ex){Status=ex.Message;}finally{IsBusy=false;}}
    private void Notify(){LoadCommand.NotifyCanExecuteChanged();SearchCommand.NotifyCanExecuteChanged();NewCommand.NotifyCanExecuteChanged();SaveCommand.NotifyCanExecuteChanged();CancelCommand.NotifyCanExecuteChanged();DeleteCommand.NotifyCanExecuteChanged();ConfirmDeleteCommand.NotifyCanExecuteChanged();}
    private bool Set<T>(ref T field,T value,[CallerMemberName]string? propertyName=null){if(EqualityComparer<T>.Default.Equals(field,value))return false;field=value;OnPropertyChanged(propertyName);return true;} private void OnPropertyChanged([CallerMemberName]string? n=null)=>PropertyChanged?.Invoke(this,new(n));
}
