namespace Biblia.Domain.Entities;

public sealed record MessageTopic(long Id, long MessageId, string Title, string? Content, int SortOrder);
