using Biblia.Application.Interfaces;

namespace Biblia.Presentation.Navigation;

internal sealed class ShellAppNavigator : IAppNavigator
{
    public Task GoToAsync(string route, IReadOnlyDictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        cancellationToken.ThrowIfCancellationRequested();
        var shell = Shell.Current ?? throw new InvalidOperationException("O Shell ainda não está disponível para navegação.");
        return parameters is null ? shell.GoToAsync(route) : shell.GoToAsync(route, new Dictionary<string, object>(parameters));
    }

    public Task GoBackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var shell = Shell.Current ?? throw new InvalidOperationException("O Shell ainda não está disponível para navegação.");
        return shell.GoToAsync("..");
    }
}
