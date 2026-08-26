using System.ComponentModel;
using Biblia.Domain.Entities;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleVerseItemViewModel(BibleVerse verse, bool isAlreadyAdded = false) : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isAlreadyAdded = isAlreadyAdded;

    public BibleVerse Verse { get; } = verse;
    public int Number => Verse.Verse;
    public string DisplayReference => $"{Verse.BookName} {Verse.Chapter}:{Verse.Verse}";
    public string Text => Verse.Text;
    public bool IsSelected => _isSelected;
    public bool IsAlreadyAdded => _isAlreadyAdded;
    public string AddedStatus => IsAlreadyAdded ? "Já adicionada a um tema" : "";

    public void SetSelected(bool value)
    {
        if (_isSelected == value) return;
        _isSelected = value;
        PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
    }

    public void MarkAlreadyAdded()
    {
        if (_isAlreadyAdded) return;
        _isAlreadyAdded = true;
        PropertyChanged?.Invoke(this, new(nameof(IsAlreadyAdded)));
        PropertyChanged?.Invoke(this, new(nameof(AddedStatus)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
