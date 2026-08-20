namespace Biblia.Domain.Entities;

public sealed record MessageReport(
    Message Message,
    BibleVersionCatalogEntry? BibleVersion,
    IReadOnlyList<MessageReportTopic> Topics,
    IReadOnlyList<MessageReportReference> UnassignedReferences)
{
    public IReadOnlyList<MessageReportReference> References =>
        Topics.SelectMany(topic => topic.References).Concat(UnassignedReferences).ToArray();
}

public sealed record MessageReportTopic(MessageTopic Topic, IReadOnlyList<MessageReportReference> References);

public sealed record MessageReportReference(
    MessageReference Link,
    SavedReferenceDetails SavedReference,
    BiblePassage? Passage);
