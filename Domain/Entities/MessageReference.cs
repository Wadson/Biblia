namespace Biblia.Domain.Entities;

public sealed record MessageReference(long Id, long MessageId, long ReferenceId, long? TopicId, int SortOrder, string? Observation, long? PreferredBibleVersionId);
