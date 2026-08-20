using Biblia.Application.Interfaces;

namespace Biblia.Infrastructure.Files;

public sealed class MauiBibleFilePicker:IBibleFilePicker
{
    public async Task<string?> PickAsync(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options=new PickOptions{PickerTitle="Selecione um banco bíblico SQLite"};
        var result=await FilePicker.Default.PickAsync(options);
        cancellationToken.ThrowIfCancellationRequested();
        return result?.FullPath;
    }
}
