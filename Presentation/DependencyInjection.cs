using Biblia.Application.Interfaces;
using Biblia.Presentation.Navigation;
using Biblia.Presentation.ViewModels;
using Biblia.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Biblia.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Navegação
        services.AddSingleton<IAppNavigator, ShellAppNavigator>();

        // Tela principal / Dashboard
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainPage>();

        // Gerenciamento de versões da Bíblia
        services.AddTransient<BibleVersionsViewModel>();
        services.AddTransient<BibleVersionsPage>();

        // Leitor bíblico
        services.AddTransient<BibleReaderViewModel>();
        services.AddTransient<BibleReaderPage>();
        services.AddTransient<BibleSearchViewModel>();
        services.AddTransient<BibleSearchPage>();
        services.AddTransient<BibleComparisonViewModel>();
        services.AddTransient<BibleComparisonPage>();

        services.AddTransient<ThemesViewModel>();
        services.AddTransient<ThemesPage>();

        services.AddTransient<SavedReferencesViewModel>();
        services.AddTransient<SavedReferencesPage>();
        services.AddTransient<MessagesViewModel>();
        services.AddTransient<MessagesPage>();
        services.AddTransient<MessageTopicsViewModel>();
        services.AddTransient<MessageTopicsPage>();
        services.AddTransient<MessageReferencesViewModel>();
        services.AddTransient<MessageReferencesPage>();
        services.AddTransient<MessageBibleReferencesPage>();
        services.AddTransient<MessageDuplicationViewModel>();
        services.AddTransient<MessageDuplicationPage>();
        services.AddTransient<GlobalSearchViewModel>();
        services.AddTransient<GlobalSearchPage>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<ReportsPage>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        return services;
    }
}
