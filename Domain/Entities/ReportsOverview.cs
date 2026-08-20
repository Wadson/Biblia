namespace Biblia.Domain.Entities;

public sealed record ReportsOverview(
    int Messages,
    int Sermons,
    int Studies,
    int Devotionals,
    int Themes,
    int References,
    int ReferencesWithComments);
