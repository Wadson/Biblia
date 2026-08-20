using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleSearchViewModel : INotifyPropertyChanged
{
    private readonly IBibleSearchService _search; private readonly IBibleVersionManager _versions;
    private string _text = string.Empty, _status = string.Empty; private bool _busy; private int? _book, _chapter;
    public BibleSearchViewModel(IBibleSearchService search, IBibleVersionManager versions) { _search = search; _versions = versions; LoadCommand = new AsyncCommand(LoadAsync, () => !IsBusy); SearchCommand = new AsyncCommand(SearchAsync, () => !IsBusy && Text.Trim().Length >= 2); ClearCommand = new AsyncCommand(ClearAsync, () => !IsBusy); LoadMoreCommand = new AsyncCommand(LoadMoreAsync, () => HasMore && !IsBusy); }
    public ObservableCollection<BibleVersionSelection> Versions { get; } = []; public ObservableCollection<BibleVerse> Results { get; } = [];
    public string Text { get => _text; set { if (Set(ref _text, value)) SearchCommand.NotifyCanExecuteChanged(); } }
    public int? BookReferenceId { get => _book; set => Set(ref _book, value); } public int? Chapter { get => _chapter; set => Set(ref _chapter, value); }
    public string Status { get => _status; private set => Set(ref _status, value); } public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) { LoadCommand.NotifyCanExecuteChanged(); SearchCommand.NotifyCanExecuteChanged(); ClearCommand.NotifyCanExecuteChanged(); LoadMoreCommand.NotifyCanExecuteChanged(); } } }
    public bool IsEmpty => !IsBusy && Results.Count == 0 && !string.IsNullOrWhiteSpace(Text); public bool HasMore { get; private set; } public int Offset { get; private set; }
    public AsyncCommand LoadCommand { get; } public AsyncCommand SearchCommand { get; } public AsyncCommand ClearCommand { get; } public AsyncCommand LoadMoreCommand { get; } public event PropertyChangedEventHandler? PropertyChanged;
    private async Task LoadAsync() => await RunAsync(async () => { await _versions.InitializeCatalogAsync(); var active = await _versions.GetActiveVersionAsync(); Versions.Clear(); foreach (var version in (await _versions.GetVersionsAsync()).Where(x => x.IsInstalled && x.IsEnabled)) Versions.Add(new BibleVersionSelection(version) { IsSelected = version.Code == active?.Code }); if (Versions.All(x => !x.IsSelected)) Versions.FirstOrDefault()?.IsSelected = true; Status = "Digite ao menos dois caracteres para pesquisar."; });
    private async Task SearchAsync() { Offset = 0; Results.Clear(); await QueryAsync(); }
    private async Task LoadMoreAsync() => await QueryAsync();
    private Task ClearAsync() { Text = string.Empty; Results.Clear(); Offset = 0; HasMore = false; Status = "Pesquisa limpa."; OnChanged(nameof(IsEmpty)); return Task.CompletedTask; }
    private async Task QueryAsync() => await RunAsync(async () => { var selected = Versions.Where(x => x.IsSelected).Select(x => x.Code).ToArray(); if (selected.Length == 0) { Status = "Selecione ao menos uma versão."; return; } var response = await _search.SearchAsync(new BibleSearchQuery(Text.Trim(), selected, BookReferenceId, Chapter, Offset, 50)); foreach (var verse in response.Items) Results.Add(verse); Offset += response.Items.Count; HasMore = response.Items.Count == 50; OnChanged(nameof(HasMore)); Status = response.Items.Count == 0 && Offset == 0 ? $"Nenhum versículo encontrado para “{Text}”." : $"{Results.Count} resultado(s) carregado(s)."; OnChanged(nameof(IsEmpty)); LoadMoreCommand.NotifyCanExecuteChanged(); });
    private async Task RunAsync(Func<Task> action) { if (IsBusy) return; IsBusy = true; try { await action(); } catch { Status = "Não foi possível concluir a pesquisa. Tente novamente."; } finally { IsBusy = false; OnChanged(nameof(IsEmpty)); } }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; OnChanged(name); return true; } private void OnChanged(string? name) => PropertyChanged?.Invoke(this, new(name));
}
