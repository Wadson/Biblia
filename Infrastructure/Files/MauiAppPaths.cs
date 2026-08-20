using Biblia.Application.Interfaces;

namespace Biblia.Infrastructure.Files;

internal sealed class MauiAppPaths : IAppPaths
{
    public string AppDataDirectory => FileSystem.AppDataDirectory;
    public string CacheDirectory => FileSystem.CacheDirectory;

    public string GetPrivateFilePath(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (!string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal))
            throw new ArgumentException("O nome não pode conter componentes de caminho.", nameof(fileName));
        return Path.Combine(AppDataDirectory, fileName);
    }
}
