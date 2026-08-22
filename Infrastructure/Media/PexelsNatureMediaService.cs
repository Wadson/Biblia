using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;

namespace Biblia.Infrastructure.Media;

public sealed class PexelsNatureMediaService(IHttpClientFactory clients, IAppPaths paths) : INatureMediaService
{
    private const string ConnectionMessage = "Sem internet ou conexão com o serviço de imagens. Use um dos fundos disponíveis.";
    private readonly ConcurrentDictionary<string, IReadOnlyList<NaturePhoto>> _searchCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<NaturePhoto> Offline =
    [
        new(-1,"","","BíbliaTema","","","#0D1E30",true,"#D4AF37"), new(-2,"","","BíbliaTema","","","#064B85",true,"#0D1E30"),
        new(-3,"","","BíbliaTema","","","#D97346",true,"#5C2749"), new(-4,"","","BíbliaTema","","","#202632",true,"#6C757D")
    ];
    public IReadOnlyList<NaturePhoto> GetOfflineBackgrounds() => Offline;

    public async Task<IReadOnlyList<NaturePhoto>> SearchAsync(string query, int page = 1, int pageSize = 8, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 6, 15);
        var cacheKey = $"{query}|{page}|{pageSize}";
        if (_searchCache.TryGetValue(cacheKey, out var cached)) return cached;
        var apiKey = Environment.GetEnvironmentVariable("BIBLIATEMA_PEXELS_API_KEY");
        var proxy = Environment.GetEnvironmentVariable("BIBLIATEMA_MEDIA_BASE_URL");
        if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(proxy))
        {
            var curated = await CachePreviewsAsync(CuratedPexels(query, page, pageSize), cancellationToken);
            _searchCache[cacheKey] = curated;
            return curated;
        }
        var client = clients.CreateClient("NatureMedia");
        var baseUrl = string.IsNullOrWhiteSpace(proxy) ? "https://api.pexels.com/v1/search" : proxy.TrimEnd('/') + "/v1/search";
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?query={Uri.EscapeDataString(query)}&orientation=portrait&per_page={pageSize}&page={page}&locale=en-US");
        if (!string.IsNullOrWhiteSpace(apiKey)) request.Headers.TryAddWithoutValidation("Authorization", apiKey);
        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(ConnectionMessage, ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(ConnectionMessage, ex);
        }
        using (response)
        {
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new InvalidOperationException("Acesso às fotos não autorizado. Verifique a configuração de mídia.");
        if ((int)response.StatusCode == 429) throw new InvalidOperationException("Limite de fotos atingido. Tente novamente mais tarde.");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PexelsResponse>(cancellationToken: cancellationToken) ?? new([]);
        var remoteResult = payload.Photos.Select(x => new NaturePhoto(x.Id, x.Src.Medium, string.IsNullOrWhiteSpace(x.Src.Portrait) ? x.Src.Large : x.Src.Portrait,
            x.Photographer, x.PhotographerUrl, x.Url, x.AvgColor)).ToArray();
        var result = await CachePreviewsAsync(remoteResult, cancellationToken);
        _searchCache[cacheKey] = result;
        return result;
        }
    }

    public async Task<string?> GetRenderFileAsync(NaturePhoto photo, CancellationToken cancellationToken = default)
    {
        if (photo.IsLocal) return null; var folder = Path.Combine(paths.CacheDirectory, "verse-cards", "backgrounds"); Directory.CreateDirectory(folder); var target = Path.Combine(folder, $"pexels-{photo.Id}.jpg");
        if (File.Exists(target) && new FileInfo(target).Length > 0) return target;
        try
        {
            var bytes = await clients.CreateClient("NatureMedia").GetByteArrayAsync(photo.RenderUrl, cancellationToken);
            await File.WriteAllBytesAsync(target, bytes, cancellationToken);
            return target;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(ConnectionMessage, ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(ConnectionMessage, ex);
        }
    }

    private sealed record PexelsResponse([property: JsonPropertyName("photos")] IReadOnlyList<PexelsPhoto> Photos);
    private sealed record PexelsPhoto(long Id, string Url, string Photographer, [property: JsonPropertyName("photographer_url")] string PhotographerUrl, [property: JsonPropertyName("avg_color")] string? AvgColor, PexelsSource Src);
    private sealed record PexelsSource(string Medium, string Large, string Portrait);

    private static IReadOnlyList<NaturePhoto> CuratedPexels(string query, int page, int pageSize)
    {
        // Catálogo remoto seguro para o aplicativo funcionar sem distribuir a chave privada da API.
        // Uma instalação com proxy/chave continua usando a pesquisa oficial acima.
        var ids = query switch
        {
            "flowers" or "flower field" => new long[] { 56866, 462118, 1166869, 1301856, 931177, 736230 },
            "forest" or "green vegetation" => new long[] { 167698, 4827, 15286, 1173777, 9754, 1582519 },
            "mountains landscape" => new long[] { 417173, 355747, 618833, 1366919, 547115, 691668 },
            "river nature" or "lake nature" or "waterfall" => new long[] { 709552, 158063, 414061, 1287145, 261949, 1450082 },
            "sunrise landscape" or "sunset landscape" or "sky clouds" => new long[] { 36717, 355465, 189349, 3225889, 462024, 2080960 },
            "wheat field" or "countryside field" or "countryside" or "sheep field" or "cattle field" => new long[] { 158163, 440731, 326082, 235725, 248280, 2132227 },
            "ocean nature" => new long[] { 457882, 248797, 1032650, 189349, 1295138, 1078983 },
            _ => new long[] { 417173, 709552, 56866, 167698, 457882, 158163 }
        };
        var additional = new long[] { 417173,709552,56866,167698,457882,158163,462118,1166869,4827,15286,355747,618833,158063,414061,36717,355465,440731,326082,248797,1032650,1366919,1173777,1287145,261949,189349,3225889,235725,248280,1295138,1078983 };
        var catalog = ids.Concat(additional).Distinct().ToArray();
        var offset = (page - 1) * pageSize;
        var pageIds = Enumerable.Range(offset, pageSize).Select(index => catalog[index % catalog.Length]);
        return pageIds.Where(id => id > 0).Select(id =>
        {
            var baseUrl = $"https://images.pexels.com/photos/{id}/pexels-photo-{id}.jpeg?auto=compress&cs=tinysrgb";
            return new NaturePhoto(id, $"{baseUrl}&w=600", $"{baseUrl}&w=1600", "Comunidade Pexels", "https://www.pexels.com", $"https://www.pexels.com/photo/{id}/", "#0D1E30");
        }).ToArray();
    }

    private async Task<IReadOnlyList<NaturePhoto>> CachePreviewsAsync(IReadOnlyList<NaturePhoto> photos, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(paths.CacheDirectory, "verse-cards", "previews");
        Directory.CreateDirectory(folder);
        var client = clients.CreateClient("NatureMedia");
        var downloads = photos.Select(async photo =>
        {
            var target = Path.Combine(folder, $"pexels-preview-{photo.Id}.jpg");
            if (File.Exists(target) && IsJpeg(await File.ReadAllBytesAsync(target, cancellationToken))) return photo with { PreviewUrl = new Uri(target).AbsoluteUri };
            try
            {
                var bytes = await client.GetByteArrayAsync(photo.PreviewUrl, cancellationToken);
                if (!IsJpeg(bytes)) throw new InvalidDataException("A resposta da imagem não contém um JPEG válido.");
                await File.WriteAllBytesAsync(target, bytes, cancellationToken);
                return photo with { PreviewUrl = new Uri(target).AbsoluteUri };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch { return null; }
        });
        var downloaded = (await Task.WhenAll(downloads)).Where(x => x is not null).Cast<NaturePhoto>().ToArray();
        if (downloaded.Length == 0 && photos.Count > 0) throw new InvalidOperationException(ConnectionMessage);
        return downloaded;
    }

    private static bool IsJpeg(byte[] bytes) => bytes.Length > 4 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[^2] == 0xFF && bytes[^1] == 0xD9;
}
