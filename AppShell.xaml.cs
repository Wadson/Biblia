using Biblia.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Biblia;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        BuildNavigation(services);
        RegisterRoutes();
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

        // ── MENSAGENS E PREGAÇÕES ──
        Items.Add(CreateSection("MENSAGENS E PREGAÇÕES"));
        Items.Add(CreateFlyoutItem<MessageBibleReferencesPage>(services, "Mensagens e Pregações", "MessageBibleReferences", "icon_preach.png"));

        // ── SISTEMA E CONFIGURAÇÕES ──
        Items.Add(CreateSection("SISTEMA"));
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
        Routing.RegisterRoute("Messages",typeof(MessagesPage));
        Routing.RegisterRoute("MessageTopics",typeof(MessageTopicsPage));
        Routing.RegisterRoute("MessageReferences",typeof(MessageReferencesPage));
        Routing.RegisterRoute("MessageDuplication",typeof(MessageDuplicationPage));
    }
}
