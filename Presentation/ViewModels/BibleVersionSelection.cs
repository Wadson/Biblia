using System.ComponentModel;
using Biblia.Domain.Entities;

namespace Biblia.Presentation.ViewModels;

public sealed class BibleVersionSelection(BibleVersionCatalogEntry version) : INotifyPropertyChanged
{
    private bool _isSelected;
    public BibleVersionCatalogEntry Version { get; } = version;
    public string Code => Version.Code;
    public string DisplayName => Version.DisplayName;
    public bool IsSource { get; init; }
    public bool IsSelected { get => _isSelected; set { if(_isSelected==value)return;_isSelected=value;PropertyChanged?.Invoke(this,new(nameof(IsSelected))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
}
