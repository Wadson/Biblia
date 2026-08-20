using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleComparisonCardViewModel(BibleComparisonItem item, string displayName, bool isSource)
{
    public string VersionCode => item.VersionCode;
    public string DisplayName { get; } = displayName;
    public bool IsSource { get; } = isSource;
    public IReadOnlyList<BibleVerse> Verses => item.Verses;
    public string StatusText => item.Status switch
    {
        BibleComparisonStatus.Available => "Encontrado",
        BibleComparisonStatus.Missing => "Referência não encontrada nesta versão.",
        BibleComparisonStatus.Ambiguous => "Versificação ambígua nesta versão.",
        BibleComparisonStatus.Error => "Erro ao carregar esta versão.",
        _ => "Status indisponível."
    };
    public string? Message => item.Message;
}

public sealed class BibleComparisonViewModel : INotifyPropertyChanged
{
    private readonly IBibleVersionManager _versions;
    private readonly IBibleComparisonService _comparison;
    private int _bookReferenceId, _chapter, _verseStart, _verseEnd;
    private string _bookName = string.Empty, _sourceVersionCode = string.Empty, _status = string.Empty;
    private bool _busy, _hasError;

    public BibleComparisonViewModel(IBibleVersionManager versions, IBibleComparisonService comparison)
    {
        _versions = versions;
        _comparison = comparison;
        CompareCommand = new AsyncCommand(CompareAsync, () => !IsBusy && Versions.Count(x => x.IsSelected) >= 2);
    }

    public ObservableCollection<BibleVersionSelection> Versions { get; } = [];
    public ObservableCollection<BibleComparisonCardViewModel> Items { get; } = [];
    public int BookReferenceId { get => _bookReferenceId; private set => Set(ref _bookReferenceId, value); }
    public string BookName { get => _bookName; private set { if(Set(ref _bookName,value)) OnChanged(nameof(ReferenceText)); } }
    public int Chapter { get => _chapter; private set { if(Set(ref _chapter,value)) OnChanged(nameof(ReferenceText)); } }
    public int VerseStart { get => _verseStart; private set { if(Set(ref _verseStart,value)) OnChanged(nameof(ReferenceText)); } }
    public int VerseEnd { get => _verseEnd; private set { if(Set(ref _verseEnd,value)) OnChanged(nameof(ReferenceText)); } }
    public string SourceVersionCode { get => _sourceVersionCode; private set => Set(ref _sourceVersionCode,value); }
    public string ReferenceText => BookReferenceId <= 0 ? string.Empty : $"{BookName} {Chapter}:{VerseStart}" + (VerseEnd == VerseStart ? string.Empty : $"–{VerseEnd}");
    public string Status { get => _status; private set => Set(ref _status, value); }
    public bool IsBusy { get => _busy; private set { if(Set(ref _busy,value)) CompareCommand.NotifyCanExecuteChanged(); } }
    public bool HasError { get => _hasError; private set => Set(ref _hasError,value); }
    public bool IsEmpty => !IsBusy && Items.Count == 0;
    public AsyncCommand CompareCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task InitializeAsync(IReadOnlyDictionary<string, object> query)
    {
        BookReferenceId = ReadInt(query, "BookReferenceId"); BookName = ReadString(query, "BookName"); Chapter = ReadInt(query, "Chapter");
        VerseStart = ReadInt(query, "VerseStart"); VerseEnd = ReadInt(query, "VerseEnd"); SourceVersionCode = ReadString(query, "SourceVersionCode");
        OnChanged(nameof(ReferenceText));
        if(BookReferenceId <= 0 || string.IsNullOrWhiteSpace(BookName) || Chapter <= 0 || VerseStart <= 0 || VerseEnd < VerseStart)
        { HasError = true; Status = "A referência recebida para comparação é inválida."; OnChanged(nameof(IsEmpty)); return; }

        await RunAsync(async () =>
        {
            Versions.Clear();
            foreach(var version in (await _versions.GetVersionsAsync()).Where(x => x.IsInstalled && x.IsEnabled))
            {
                var option=new BibleVersionSelection(version) { IsSelected = true, IsSource = string.Equals(version.Code, SourceVersionCode, StringComparison.OrdinalIgnoreCase) };
                option.PropertyChanged+=(_,e)=>{if(e.PropertyName==nameof(BibleVersionSelection.IsSelected))CompareCommand.NotifyCanExecuteChanged();};
                Versions.Add(option);
            }
            CompareCommand.NotifyCanExecuteChanged();
            if(Versions.Count < 2) { HasError = true; Status = "Instale e habilite ao menos duas versões para comparar."; return; }
            await CompareCoreAsync();
        });
    }

    private Task CompareAsync() => RunAsync(CompareCoreAsync);
    private async Task CompareCoreAsync()
    {
        var selected = Versions.Where(x => x.IsSelected).Select(x => x.Code).ToArray();
        if(selected.Length < 2) { Status = "Selecione ao menos duas versões."; return; }
        var names = Versions.ToDictionary(x => x.Code, x => x.DisplayName, StringComparer.OrdinalIgnoreCase);
        Items.Clear();
        foreach(var item in await _comparison.CompareAsync(BookReferenceId, Chapter, VerseStart, VerseEnd, selected))
            Items.Add(new(item, names.GetValueOrDefault(item.VersionCode, item.VersionCode), string.Equals(item.VersionCode, SourceVersionCode, StringComparison.OrdinalIgnoreCase)));
        HasError = Items.Any(x => x.StatusText != "Encontrado");
        Status = Items.Count == 0 ? "Nenhuma comparação disponível." : $"Comparação em {Items.Count} versões.";
    }
    private async Task RunAsync(Func<Task> action){if(IsBusy)return;IsBusy=true;HasError=false;try{await action();}catch(Exception){HasError=true;Status="Não foi possível comparar as versões.";}finally{IsBusy=false;OnChanged(nameof(IsEmpty));}}
    private static int ReadInt(IReadOnlyDictionary<string,object> query,string key)=>query.TryGetValue(key,out var value)&&int.TryParse(value?.ToString(),out var result)?result:0;
    private static string ReadString(IReadOnlyDictionary<string,object> query,string key)=>query.TryGetValue(key,out var value)?value?.ToString()??string.Empty:string.Empty;
    private bool Set<T>(ref T field,T value,[CallerMemberName]string? name=null){if(EqualityComparer<T>.Default.Equals(field,value))return false;field=value;OnChanged(name);return true;}
    private void OnChanged(string? name)=>PropertyChanged?.Invoke(this,new(name));
}
