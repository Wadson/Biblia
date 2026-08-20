using Biblia.Application.Interfaces;

namespace Biblia.Infrastructure.Files;

public sealed class MauiPackagedBibleSource:IPackagedBibleSource
{
    public async Task<Stream> OpenReadAsync(string databaseFileName,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFileName);
        if(!string.Equals(databaseFileName,Path.GetFileName(databaseFileName),StringComparison.Ordinal))throw new ArgumentException("Nome de banco inválido.",nameof(databaseFileName));
        cancellationToken.ThrowIfCancellationRequested();
        return await FileSystem.OpenAppPackageFileAsync($"Bibles/{databaseFileName}");
    }
}
