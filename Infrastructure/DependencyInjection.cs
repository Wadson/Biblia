using Biblia.Application.Interfaces;
using Biblia.Infrastructure.Files;
using Biblia.Infrastructure.Time;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Infrastructure.Repositories;
using Biblia.Infrastructure.BibleDatabases;
using Microsoft.Extensions.DependencyInjection;

namespace Biblia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IAppPaths, MauiAppPaths>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<AppDatabase.AppDatabase>();
        services.AddSingleton<IAppDatabase>(provider => provider.GetRequiredService<AppDatabase.AppDatabase>());
        services.AddSingleton<IThemeRepository, ThemeRepository>();
        services.AddSingleton<ISavedReferenceRepository, SavedReferenceRepository>();
        services.AddSingleton<IMessageRepository, MessageRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<IBibleVersionCatalogRepository, BibleVersionCatalogRepository>();
        services.AddSingleton<IBibleValidationService, BibleValidationService>();
        services.AddSingleton<IBibleRepository, BibleRepository>();
        services.AddSingleton<IBibleVersionManifestProvider, MauiBibleVersionManifestProvider>();
        services.AddSingleton<IPackagedBibleSource, MauiPackagedBibleSource>();
        services.AddSingleton<IBibleFilePicker, MauiBibleFilePicker>();
        services.AddSingleton<IClipboardService, MauiClipboardService>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IBackupService, BackupService>();
        return services;
    }
}
