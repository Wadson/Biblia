using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;

namespace Biblia.Application.Services;

public sealed class ReportService(
    IMessageRepository messages,
    ISavedReferenceRepository references,
    IThemeRepository themes,
    IBibleVersionCatalogRepository versions,
    IBibleRepository bibleRepository) : IReportService
{
    public async Task<ReportsOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var allMessages = await messages.GetAllAsync(cancellationToken);
        var allReferences = await references.SearchAsync(null, cancellationToken);
        var allThemes = await themes.GetAllAsync(cancellationToken);
        return new ReportsOverview(
            allMessages.Count(x => x.Type == MessageType.Message),
            allMessages.Count(x => x.Type == MessageType.Sermon),
            allMessages.Count(x => x.Type == MessageType.Study),
            allMessages.Count(x => x.Type == MessageType.Devotional),
            allThemes.Count,
            allReferences.Count,
            allReferences.Count(x => !string.IsNullOrWhiteSpace(x.Reference.Comment)));
    }

    public async Task<MessageReport> BuildMessageAsync(long messageId, CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(messageId, cancellationToken)
            ?? throw new KeyNotFoundException("Mensagem não encontrada.");
        var version = await ResolveVersionAsync(message.PreferredBibleVersionId, cancellationToken);
        var topics = await messages.GetTopicsAsync(messageId, cancellationToken);
        var links = await messages.GetReferencesAsync(messageId, cancellationToken);
        var reportReferences = new List<MessageReportReference>();

        foreach (var link in links)
        {
            var saved = await references.GetDetailsAsync(link.ReferenceId, cancellationToken)
                ?? throw new InvalidOperationException("A referência vinculada à mensagem não está disponível.");
            var selectedVersion = await ResolveVersionAsync(link.PreferredBibleVersionId, cancellationToken) ?? version;
            BiblePassage? passage = null;
            if (selectedVersion is { IsInstalled: true, IsEnabled: true })
                passage = await bibleRepository.GetPassageAsync(selectedVersion.Code, saved.Reference.BookReferenceId, saved.Reference.Chapter, saved.Reference.VerseStart, saved.Reference.VerseEnd, cancellationToken);
            reportReferences.Add(new MessageReportReference(link, saved, passage));
        }

        var reportTopics = topics.Select(topic => new MessageReportTopic(topic,
            reportReferences.Where(reference => reference.Link.TopicId == topic.Id).OrderBy(reference => reference.Link.SortOrder).ToArray())).ToArray();
        var unassigned = reportReferences.Where(reference => reference.Link.TopicId is null).OrderBy(reference => reference.Link.SortOrder).ToArray();
        return new MessageReport(message, version, reportTopics, unassigned);
    }

    private async Task<BibleVersionCatalogEntry?> ResolveVersionAsync(long? versionId, CancellationToken cancellationToken)
    {
        if (versionId is null) return null;
        return (await versions.GetAllAsync(cancellationToken)).FirstOrDefault(version => version.Id == versionId);
    }
}
