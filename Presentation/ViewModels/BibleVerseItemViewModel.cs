using System.ComponentModel;
using Biblia.Domain.Entities;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleVerseItemViewModel(BibleVerse verse) : INotifyPropertyChanged
{
    private bool _isSelected;

    public BibleVerse Verse { get; } = verse;
    public int Number => Verse.Verse;
    public string Text => Verse.Text;
    public bool IsSelected => _isSelected;

    public void SetSelected(bool value)
    {
        if (_isSelected == value) return;
        _isSelected = value;
        PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
