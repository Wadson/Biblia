using System.ComponentModel;
using System.Runtime.CompilerServices;
using Biblia.Application.Interfaces;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settings;
    private bool _darkMode;
    private string _status = "";

    public SettingsViewModel(ISettingsService settings)
    {
        _settings = settings;
        LoadCommand = new AsyncCommand(LoadAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
    }

    public bool DarkMode
    {
        get => _darkMode;
        set
        {
            if (!Set(ref _darkMode, value)) return;
            Microsoft.Maui.Controls.Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
        }
    }

    public string Status { get => _status; private set => Set(ref _status, value); }
    public AsyncCommand LoadCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync()
    {
        DarkMode = "dark".Equals(await _settings.GetAsync("appearance"), StringComparison.OrdinalIgnoreCase);
        Status = "Preferências carregadas.";
    }

    private async Task SaveAsync()
    {
        await _settings.SetAsync("appearance", DarkMode ? "dark" : "light");
        Status = "Preferências salvas.";
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
