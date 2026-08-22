using Biblia.Application.Interfaces;
using CommunityToolkit.Maui.Storage;

namespace Biblia.Infrastructure.Files;

public sealed class MauiFileTransferService:IFileTransferService
{
    public async Task SaveCopyAsync(string path,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(!File.Exists(path))throw new FileNotFoundException("Arquivo não encontrado.",path);
        await using var stream=File.OpenRead(path);
        var result=await FileSaver.Default.SaveAsync(Path.GetFileName(path),stream,cancellationToken);
        if(!result.IsSuccessful)
            throw result.Exception??new IOException("Não foi possível salvar a cópia do arquivo.");
    }
    public Task ShareAsync(string path,CancellationToken cancellationToken=default)=>OpenShareAsync(path,Path.GetExtension(path).ToLowerInvariant() switch{".pdf"=>"Compartilhar pregação BíbliaTema",".png" or ".jpg" or ".jpeg"=>"Compartilhar card BíbliaTema",_=>"Compartilhar backup BíbliaTema"},cancellationToken);
    private static async Task OpenShareAsync(string path,string title,CancellationToken token){token.ThrowIfCancellationRequested();if(!File.Exists(path))throw new FileNotFoundException("Arquivo não encontrado.",path);await Share.Default.RequestAsync(new ShareFileRequest{Title=title,File=new ShareFile(path)});}
    public async Task<string?> PickBackupAsync(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var types=new FilePickerFileType(new Dictionary<DevicePlatform,IEnumerable<string>>{{DevicePlatform.Android,["application/zip","application/octet-stream"]},{DevicePlatform.WinUI,[".zip"]},{DevicePlatform.iOS,["public.zip-archive"]},{DevicePlatform.MacCatalyst,["public.zip-archive"]}});
        var result=await FilePicker.Default.PickAsync(new PickOptions{PickerTitle="Selecionar backup do BíbliaTema",FileTypes=types});
        return result?.FullPath;
    }
}

public sealed class MauiAppInfoService:IAppInfoService
{
    public string Version=>AppInfo.Current.VersionString;
    public string Build=>AppInfo.Current.BuildString;
}
