using Biblia.Domain.Enums;

namespace Biblia.Domain.Entities;

public sealed record Message(
    long Id,
    string Title,
    string? Description,
    MessageType Type,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Introduction = null,
    string? Conclusion = null,
    long? PreferredBibleVersionId = null);
