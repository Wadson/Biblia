using Biblia.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;
using Biblia.Application.Interfaces;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;

namespace Biblia;

public partial class AppShell : Shell
{
    private readonly IApplicationExitService _exitService;

    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        _exitService = services.GetRequiredService<IApplicationExitService>();
        ExitFlyoutButton.IsVisible = _exitService.CanExit;
        BuildNavigation(services);
        RegisterRoutes();
    }

    private async void OnExitTapped(object? sender, TappedEventArgs e)
    {
        if (!_exitService.CanExit || CurrentPage is not Page host)
            return;
        FlyoutIsPresented = false;
        await Task.Delay(120);
        var options = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Color.FromArgb("990D1E30"),
            Shape = new RoundRectangle { CornerRadius = 18, Stroke = Colors.Transparent, StrokeThickness = 0 }
        };
        var result = await host.ShowPopupAsync<bool>(new ExitConfirmationView(host), options);
        if (result.Result is true)
            await _exitService.ExitAsync();
    }

    private void BuildNavigation(IServiceProvider services)
    {
        // ── PRINCIPAL ──
        Items.Add(CreateSection("PRINCIPAL"));
        Items.Add(CreateFlyoutItem<MainPage>(services, "Início", "Home", "icon_home.png"));
        Items.Add(CreateFlyoutItem<BibleReaderPage>(services, "Bíblia", "BibleReader", "icon_bible.png"));
        Items.Add(CreateFlyoutItem<BibleSearchPage>(services, "Pesquisar", "BibleSearch", "icon_search.png"));

        // ── ESTUDO E RECURSOS ──
        Items.Add(CreateSection("ESTUDO E RECURSOS"));
        Items.Add(CreateFlyoutItem<BibleVersionsPage>(services, "Versões", "BibleVersions", "icon_versions.png"));
        Items.Add(CreateFlyoutItem<ThemesPage>(services, "Temas", "Themes", "icon_themes.png"));
        Items.Add(CreateFlyoutItem<SavedReferencesPage>(services, "Referências guardadas", "SavedReferences", "icon_bookmark.png"));

        // ── TEMAS E VERSÍCULOS ──
        Items.Add(CreateSection("TEMAS E VERSÍCULOS"));
        Items.Add(CreateFlyoutItem<MessageBibleReferencesPage>(services, "Vinculação de Temas", "MessageBibleReferences", "icon_preach.png"));

        // ── SISTEMA E CONFIGURAÇÕES ──
        Items.Add(CreateSection("SISTEMA E CONFIGURAÇÕES"));
        Items.Add(CreateFlyoutItem<GlobalSearchPage>(services, "Pesquisa global", "GlobalSearch", "icon_glob_search.png"));
        Items.Add(CreateFlyoutItem<ReportsPage>(services, "Relatórios", "Reports", "icon_reports.png"));
        Items.Add(CreateFlyoutItem<SettingsPage>(services, "Configurações", "Settings", "icon_settings.png"));

        CurrentItem = Items.First(item => item.IsEnabled);
    }

    private static FlyoutItem CreateSection(string title)
    {
        var section = new FlyoutItem { Title = title, IsEnabled = false, FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem };
        section.Items.Add(new ShellContent { Title = title, ContentTemplate = new DataTemplate(() => new ContentPage()) });
        return section;
    }

    private static FlyoutItem CreateFlyoutItem<TPage>(
        IServiceProvider services,
        string title,
        string route,
        string icon)
        where TPage : Page
    {
        var flyoutItem = new FlyoutItem
        {
            Title = title,
            Route = route,
            Icon = icon
        };

        flyoutItem.Items.Add(new ShellContent
        {
            Title = title,
            Route = route,
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<TPage>())
        });

        return flyoutItem;
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute("BibleComparison", typeof(BibleComparisonPage));
        Routing.RegisterRoute("VerseCardStudio",typeof(VerseCardStudioPage));
    }
}
