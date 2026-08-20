using System.IO.Compression;
using System.Text.Json;
using Biblia.Application.Interfaces;

namespace Biblia.Infrastructure.Files;

public sealed class BackupService(IAppDatabase database, IAppPaths paths) : IBackupService
{
    public async Task<BackupInfo> CreateAsync(CancellationToken cancellationToken = default)
    {
        await database.InitializeAsync(cancellationToken); var schema = await database.GetSchemaVersionAsync(cancellationToken); var created = DateTimeOffset.UtcNow;
        var directory = Path.Combine(paths.AppDataDirectory, "backups"); Directory.CreateDirectory(directory); var path = Path.Combine(directory, $"bibliatema-{created:yyyyMMddHHmmss}.zip");
        await using var output = File.Create(path); using var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: false);
        var manifest = archive.CreateEntry("manifest.json"); await using (var manifestStream = manifest.Open()) await JsonSerializer.SerializeAsync(manifestStream, new Manifest(created, schema), cancellationToken: cancellationToken);
        var entry = archive.CreateEntry("bibliatema.db", CompressionLevel.Optimal); await using var input = File.OpenRead(database.DatabasePath); await using var target = entry.Open(); await input.CopyToAsync(target, cancellationToken);
        return new BackupInfo(path, created, schema, new FileInfo(path).Length);
    }
    public async Task<BackupInfo> ValidateAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        await using var file = File.OpenRead(backupPath); using var archive = new ZipArchive(file, ZipArchiveMode.Read); var manifest=archive.GetEntry("manifest.json")??throw new InvalidDataException("Backup sem manifesto."); if(archive.GetEntry("bibliatema.db") is null)throw new InvalidDataException("Backup sem banco do usuário."); await using var stream=manifest.Open();var data=await JsonSerializer.DeserializeAsync<Manifest>(stream,cancellationToken:cancellationToken)??throw new InvalidDataException("Manifesto inválido.");if(data.SchemaVersion>await database.GetSchemaVersionAsync(cancellationToken))throw new InvalidDataException("Backup usa schema mais novo.");return new BackupInfo(backupPath,data.CreatedAt,data.SchemaVersion,new FileInfo(backupPath).Length);
    }
    public async Task RestoreAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(backupPath,cancellationToken); var safety=await CreateAsync(cancellationToken); var temporary=database.DatabasePath+".restore";
        try { await using var file=File.OpenRead(backupPath);using var archive=new ZipArchive(file,ZipArchiveMode.Read);await using var source=archive.GetEntry("bibliatema.db")!.Open();await using var destination=File.Create(temporary);await source.CopyToAsync(destination,cancellationToken);await destination.FlushAsync(cancellationToken);Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();File.Move(temporary,database.DatabasePath,true); }
        catch { if(File.Exists(safety.Path)) { try { await RestoreFromSafetyAsync(safety.Path,cancellationToken); } catch { } } throw; }
        finally { if(File.Exists(temporary))File.Delete(temporary); }
    }
    private async Task RestoreFromSafetyAsync(string path,CancellationToken token){await using var file=File.OpenRead(path);using var archive=new ZipArchive(file,ZipArchiveMode.Read);await using var source=archive.GetEntry("bibliatema.db")!.Open();await using var target=File.Create(database.DatabasePath);await source.CopyToAsync(target,token);}
    private sealed record Manifest(DateTimeOffset CreatedAt,int SchemaVersion);
}
