namespace Biblia.Domain.Entities;

public sealed record SermonReportRequest(
    IReadOnlyCollection<long> ThemeIds,
    string Title,
    string? Subtitle,
    string? Introduction,
    string? Conclusion,
    string BibleVersionCode,
    bool UsePreferredReferenceVersions = false);

public sealed record SermonReport(
    string DocumentType,
    string Title,
    string? Subtitle,
    string? Introduction,
    string? Conclusion,
    string VersionLabel,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<SermonReportSection> Sections)
{
    public IReadOnlyList<SermonReportReference> References => Sections.SelectMany(x => x.References).ToArray();
    public bool HasIntroduction => !string.IsNullOrWhiteSpace(Introduction);
    public bool HasConclusion => !string.IsNullOrWhiteSpace(Conclusion);
}

public sealed record SermonReportSection(
    Theme? Theme,
    string Title,
    string? Content,
    IReadOnlyList<SermonReportReference> References);

public sealed record SermonReportReference(
    long SavedReferenceId,
    string BookName,
    int Chapter,
    int VerseStart,
    int VerseEnd,
    string FormattedReference,
    BiblePassage Passage,
    string? Comment,
    string? Observation,
    string VersionCode,
    IReadOnlyList<Theme> Themes)
{
    public bool HasComment => !string.IsNullOrWhiteSpace(Comment);
    public bool HasObservation => !string.IsNullOrWhiteSpace(Observation);
    public string PreviewPassageText => string.Join(" ", Passage.Verses.Select(x => $"{x.Verse} {x.Text}"));
}
