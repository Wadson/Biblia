using System.Text.Json;
using System.Text.Json.Serialization;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;

namespace Biblia.Infrastructure.Files;

public sealed class MauiBibleVersionManifestProvider : IBibleVersionManifestProvider
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<BibleVersionManifest> GetManifestAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("bible-versions.manifest.json");
        return await JsonSerializer.DeserializeAsync<BibleVersionManifest>(stream, Options, cancellationToken)
            ?? throw new InvalidDataException("O manifesto de versões bíblicas está vazio ou inválido.");
    }
}
