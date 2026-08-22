using Biblia.Application;
using Biblia.Infrastructure;
using Biblia.Presentation;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace Biblia;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
#if ANDROID
            .ConfigureMauiHandlers(handlers => handlers.AddHandler<Presentation.Components.CachedPhotoImage, Platforms.Android.CachedPhotoImageHandler>())
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddApplication()
            .AddInfrastructure()
            .AddPresentation();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
