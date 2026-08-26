using Biblia.Application.Interfaces;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace Biblia.Infrastructure.Files;

public sealed class MauiFileTransferService(ILogger<MauiFileTransferService> logger):IFileTransferService
{
    public async Task<FileSaveOutcome> SaveCopyAsync(string path,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();
        if(!File.Exists(path))throw new FileNotFoundException("Arquivo não encontrado.",path);
        if(DeviceInfo.Platform==DevicePlatform.WinUI)return await SaveWithSystemPickerAsync(path,"Windows",cancellationToken);
        if(DeviceInfo.Platform==DevicePlatform.Android)return await SaveWithSystemPickerAsync(path,"Android",cancellationToken);
        return await SaveWithSystemPickerAsync(path,DeviceInfo.Platform.ToString(),cancellationToken);
    }
    private async Task<FileSaveOutcome> SaveWithSystemPickerAsync(string path,string platform,CancellationToken cancellationToken)
    {
        await using var stream=File.OpenRead(path);
        try
        {
            var result=await FileSaver.Default.SaveAsync(Path.GetFileName(path),stream,cancellationToken);
            if(result.IsSuccessful){logger.LogInformation("Arquivo exportado pelo seletor do sistema em {Platform}: {FileName}",platform,Path.GetFileName(path));return FileSaveOutcome.Saved;}
            if(result.Exception is null or OperationCanceledException){logger.LogInformation("Exportação cancelada pelo usuário em {Platform}",platform);return FileSaveOutcome.Cancelled;}
            throw result.Exception;
        }
        catch(OperationCanceledException) when(!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Seletor de salvamento fechado pelo usuário em {Platform}",platform);
            return FileSaveOutcome.Cancelled;
        }
    }
    public Task ShareAsync(string path,CancellationToken cancellationToken=default)=>OpenShareAsync(path,Path.GetExtension(path).ToLowerInvariant() switch{".pdf"=>"Compartilhar pregação BíbliaTema",".png" or ".jpg" or ".jpeg"=>"Compartilhar card BíbliaTema",_=>"Compartilhar backup BíbliaTema"},cancellationToken);
    private static async Task OpenShareAsync(string path,string title,CancellationToken token){token.ThrowIfCancellationRequested();if(!File.Exists(path))throw new FileNotFoundException("Arquivo não encontrado.",path);await Share.Default.RequestAsync(new ShareFileRequest{Title=title,File=new ShareFile(path)});}
    public async Task<string?> PickBackupAsync(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var types=new FilePickerFileType(new Dictionary<DevicePlatform,IEnumerable<string>>{{DevicePlatform.Android,["application/zip","application/octet-stream"]},{DevicePlatform.WinUI,[".zip"]},{DevicePlatform.iOS,["public.zip-archive"]},{DevicePlatform.MacCatalyst,["public.zip-archive"]}});
        var result=await FilePicker.Default.PickAsync(new PickOptions{PickerTitle="Selecionar backup do BíbliaTema",FileTypes=types});
        if(result is null)return null;
        if(DeviceInfo.Platform!=DevicePlatform.Android||(!string.IsNullOrWhiteSpace(result.FullPath)&&File.Exists(result.FullPath)))return result.FullPath;
        var folder=Path.Combine(FileSystem.CacheDirectory,"backup-imports");Directory.CreateDirectory(folder);
        var name=Path.GetFileName(result.FileName);if(string.IsNullOrWhiteSpace(name)||!name.EndsWith(".zip",StringComparison.OrdinalIgnoreCase))name=$"bibliatema-import-{Guid.NewGuid():N}.zip";
        var local=Path.Combine(folder,name);await using var source=await result.OpenReadAsync();await using var target=new FileStream(local,FileMode.Create,FileAccess.Write,FileShare.None,81920,true);await source.CopyToAsync(target,cancellationToken);await target.FlushAsync(cancellationToken);return local;
    }
}

public sealed class MauiAppInfoService:IAppInfoService
{
    public string Version=>AppInfo.Current.VersionString;
    public string Build=>AppInfo.Current.BuildString;
}
