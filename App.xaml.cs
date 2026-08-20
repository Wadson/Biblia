using Biblia.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Biblia;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IAppInitializationService _initializationService;
    private readonly ILogger<App> _logger;
    private readonly AppShell _shell;

    public App(IAppInitializationService initializationService, ILogger<App> logger, AppShell shell)
    {
        _initializationService = initializationService;
        _logger = logger;
        _shell = shell;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_shell);
        window.Created += async (_, _) =>
        {
            try
            {
                await _initializationService.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A inicialização do BibliaTema falhou.");
            }
        };
        return window;
    }
}
