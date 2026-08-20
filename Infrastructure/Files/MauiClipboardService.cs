using Biblia.Application.Interfaces;

namespace Biblia.Infrastructure.Files;

public sealed class MauiClipboardService:IClipboardService
{
    public async Task SetTextAsync(string text,CancellationToken cancellationToken=default){cancellationToken.ThrowIfCancellationRequested();await Clipboard.Default.SetTextAsync(text);}
}
