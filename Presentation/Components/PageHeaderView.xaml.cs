using System.Windows.Input;
using Biblia.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Biblia.Presentation.Components;

public partial class PageHeaderView : ContentView
{
    private int _navigationInProgress;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(PageHeaderView), string.Empty);

    public static readonly BindableProperty ShowBackButtonProperty = BindableProperty.Create(
        nameof(ShowBackButton), typeof(bool), typeof(PageHeaderView), true);

    public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(
        nameof(BackCommand), typeof(ICommand), typeof(PageHeaderView));

    public static readonly BindableProperty FallbackRouteProperty = BindableProperty.Create(
        nameof(FallbackRoute), typeof(string), typeof(PageHeaderView), null);

    public PageHeaderView()
    {
        InternalBackCommand = new Command(async () => await NavigateBackAsync(), () => Volatile.Read(ref _navigationInProgress) == 0);
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public string? FallbackRoute
    {
        get => (string?)GetValue(FallbackRouteProperty);
        set => SetValue(FallbackRouteProperty, value);
    }

    public Command InternalBackCommand { get; }

    private async Task NavigateBackAsync()
    {
        if (Interlocked.CompareExchange(ref _navigationInProgress, 1, 0) != 0)
            return;

        InternalBackCommand.ChangeCanExecute();
        try
        {
            if (BackCommand is { } command && command.CanExecute(null))
            {
                command.Execute(null);
                return;
            }

            var navigator = Handler?.MauiContext?.Services.GetService<IAppNavigator>();
            var canPop = Shell.Current?.Navigation.NavigationStack.Count > 1;
            if (canPop && navigator is not null)
                await navigator.GoBackAsync();
            else if (!string.IsNullOrWhiteSpace(FallbackRoute) && navigator is not null)
                await navigator.GoToAsync(FallbackRoute);
            else if (Shell.Current is not null)
                await Shell.Current.GoToAsync("..");
        }
        finally
        {
            Interlocked.Exchange(ref _navigationInProgress, 0);
            InternalBackCommand.ChangeCanExecute();
        }
    }
}
