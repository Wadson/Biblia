using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Domain.Rules;

namespace Biblia.Application.Services;

public sealed class ReportService(IMessageRepository messages, ISavedReferenceRepository references, IThemeRepository themes,
    IBibleVersionCatalogRepository versions, IBibleRepository bibleRepository, IClock clock) : IReportService
{
    public async Task<ReportsOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var allMessages = await messages.GetAllAsync(cancellationToken); var allReferences = await references.SearchAsync(null, cancellationToken); var allThemes = await themes.GetAllAsync(cancellationToken);
        return new(allMessages.Count(x => x.Type == MessageType.Message), allMessages.Count(x => x.Type == MessageType.Sermon), allMessages.Count(x => x.Type == MessageType.Study),
            allMessages.Count(x => x.Type == MessageType.Devotional), allThemes.Count, allReferences.Count, allReferences.Count(x => !string.IsNullOrWhiteSpace(x.Reference.Comment)));
    }

    public async Task<MessageReport> BuildMessageAsync(long messageId, CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(messageId, cancellationToken) ?? throw new KeyNotFoundException("Mensagem não encontrada.");
        var version = await ResolveVersionAsync(message.PreferredBibleVersionId, cancellationToken); var topics = await messages.GetTopicsAsync(messageId, cancellationToken); var links = await messages.GetReferencesAsync(messageId, cancellationToken);
        var reportReferences = new List<MessageReportReference>();
        foreach (var link in links)
        {
            var saved = await references.GetDetailsAsync(link.ReferenceId, cancellationToken) ?? throw new InvalidOperationException("A referência vinculada à mensagem não está disponível.");
            var selectedVersion = await ResolveVersionAsync(link.PreferredBibleVersionId, cancellationToken) ?? version ?? throw new InvalidOperationException("Defina uma versão bíblica para gerar o relatório.");
            EnsureVersionAvailable(selectedVersion);
            var passage = await bibleRepository.GetPassageAsync(selectedVersion.Code, saved.Reference.BookReferenceId, saved.Reference.Chapter, saved.Reference.VerseStart, saved.Reference.VerseEnd, cancellationToken);
            var bookName = await ResolveBookNameAsync(selectedVersion.Code, saved.Reference.BookReferenceId, cancellationToken);
            reportReferences.Add(new(link, saved, passage, bookName, BibleReferenceFormatter.Format(bookName, saved.Reference.Chapter, saved.Reference.VerseStart, saved.Reference.VerseEnd), selectedVersion));
        }
        var reportTopics = topics.Select(topic => new MessageReportTopic(topic, reportReferences.Where(x => x.Link.TopicId == topic.Id).OrderBy(x => x.Link.SortOrder).ToArray())).ToArray();
        return new(message, version, reportTopics, reportReferences.Where(x => x.Link.TopicId is null).OrderBy(x => x.Link.SortOrder).ToArray());
    }

    public async Task<SermonReport> BuildFromMessageAsync(long messageId, string? title = null, string? subtitle = null, string? introduction = null, string? conclusion = null, CancellationToken cancellationToken = default)
    {
        var source = await BuildMessageAsync(messageId, cancellationToken);
        var sections = source.Topics.Select(topic => new SermonReportSection(null, topic.Topic.Title, topic.Topic.Content, topic.References.Select(ToSermonReference).ToArray())).ToList();
        if (source.UnassignedReferences.Count > 0) sections.Add(new(null, "Referências utilizadas", null, source.UnassignedReferences.Select(ToSermonReference).ToArray()));
        var codes = source.References.Select(x => x.BibleVersion?.Code).Where(x => x is not null).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new("PREGAÇÃO", Clean(title) ?? source.Message.Title, Clean(subtitle) ?? source.Message.Description, Clean(introduction) ?? source.Message.Introduction,
            Clean(conclusion) ?? source.Message.Conclusion, codes.Length == 1 ? FormatVersion(source.References[0].BibleVersion!) : "Versões indicadas em cada referência", clock.UtcNow, sections);
    }

    public async Task<SermonReport> BuildFromThemesAsync(SermonReportRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ThemeIds.Count == 0) throw new InvalidOperationException("Selecione ao menos um tema.");
        var selectedThemes = (await themes.GetAllAsync(cancellationToken)).Where(x => request.ThemeIds.Contains(x.Id)).ToArray();
        if (selectedThemes.Length != request.ThemeIds.Distinct().Count()) throw new InvalidOperationException("Um ou mais temas selecionados não existem.");
        var defaultVersion = (await versions.GetAllAsync(cancellationToken)).FirstOrDefault(x => x.Code.Equals(request.BibleVersionCode, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException("A versão bíblica selecionada não existe.");
        EnsureVersionAvailable(defaultVersion); var savedReferences = await references.GetByThemeIdsAsync(request.ThemeIds, cancellationToken); var resolved = new Dictionary<long, SermonReportReference>();
        foreach (var saved in savedReferences)
        {
            var referenceVersion = request.UsePreferredReferenceVersions ? await ResolveVersionAsync(saved.Reference.PreferredBibleVersionId, cancellationToken) ?? defaultVersion : defaultVersion;
            EnsureVersionAvailable(referenceVersion);
            var passage = await bibleRepository.GetPassageAsync(referenceVersion.Code, saved.Reference.BookReferenceId, saved.Reference.Chapter, saved.Reference.VerseStart, saved.Reference.VerseEnd, cancellationToken);
            var bookName = await ResolveBookNameAsync(referenceVersion.Code, saved.Reference.BookReferenceId, cancellationToken); var relatedThemes = saved.Themes.Where(x => request.ThemeIds.Contains(x.Id)).ToArray();
            resolved[saved.Reference.Id] = new(saved.Reference.Id, bookName, saved.Reference.Chapter, saved.Reference.VerseStart, saved.Reference.VerseEnd,
                BibleReferenceFormatter.Format(bookName, saved.Reference.Chapter, saved.Reference.VerseStart, saved.Reference.VerseEnd), passage, saved.Reference.Comment, null, referenceVersion.Code, relatedThemes);
        }
        var assigned = new HashSet<long>(); var sections = new List<SermonReportSection>();
        foreach (var theme in selectedThemes)
        {
            var items = resolved.Values.Where(x => !assigned.Contains(x.SavedReferenceId) && x.Themes.Any(t => t.Id == theme.Id)).OrderBy(x => x.BookName).ThenBy(x => x.Chapter).ThenBy(x => x.VerseStart).ToArray();
            foreach (var item in items) assigned.Add(item.SavedReferenceId); sections.Add(new(theme, theme.Name, theme.Description, items));
        }
        return new("PREGAÇÃO", request.Title.Trim(), Clean(request.Subtitle), Clean(request.Introduction), Clean(request.Conclusion),
            request.UsePreferredReferenceVersions ? "Versões indicadas em cada referência" : FormatVersion(defaultVersion), clock.UtcNow, sections);
    }

    private static SermonReportReference ToSermonReference(MessageReportReference item) => new(item.SavedReference.Reference.Id, item.BookName,
        item.SavedReference.Reference.Chapter, item.SavedReference.Reference.VerseStart, item.SavedReference.Reference.VerseEnd, item.FormattedReference,
        item.Passage ?? throw new InvalidOperationException($"Texto bíblico indisponível para {item.FormattedReference}."), item.SavedReference.Reference.Comment,
        item.Link.Observation, item.BibleVersion?.Code ?? string.Empty, item.SavedReference.Themes);
    private async Task<string> ResolveBookNameAsync(string versionCode, int bookReferenceId, CancellationToken cancellationToken) =>
        (await bibleRepository.GetBooksAsync(versionCode, cancellationToken)).SingleOrDefault(x => x.BookReferenceId == bookReferenceId)?.Name
        ?? throw new InvalidOperationException($"Não foi possível resolver o nome canônico do livro de identificador {bookReferenceId} na versão {versionCode}.");
    private async Task<BibleVersionCatalogEntry?> ResolveVersionAsync(long? versionId, CancellationToken cancellationToken) => versionId is null ? null : (await versions.GetAllAsync(cancellationToken)).FirstOrDefault(x => x.Id == versionId);
    private static void EnsureVersionAvailable(BibleVersionCatalogEntry version) { if (!version.IsInstalled || !version.IsEnabled) throw new InvalidOperationException($"A versão {version.Code} não está disponível para leitura."); }
    private static string FormatVersion(BibleVersionCatalogEntry version) => $"{version.Code} — {version.DisplayName}";
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
