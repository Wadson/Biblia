using System.Globalization;
using System.Reflection;
using System.Text;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using Font = MigraDoc.DocumentObjectModel.Font;
using Colors = MigraDoc.DocumentObjectModel.Colors;
using VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment;

namespace Biblia.Infrastructure.Files;

public sealed class PdfService : IPdfService
{
    private const string Header = "#0D1E30", Gold = "#D4AF37", Primary = "#0066CC", Text = "#0A0A0A", Secondary = "#6C757D", Surface = "#F4F6F9", Border = "#E0E0E0";
    private static readonly object FontLock = new();
    private readonly IAppPaths _paths;
    public PdfService(IAppPaths paths) { _paths = paths; EnsureFontResolver(); }

    public Task<string> CreateMessagePdfAsync(MessageReport source, CancellationToken cancellationToken = default)
    {
        var sections = source.Topics.Select(x => new SermonReportSection(null, x.Topic.Title, x.Topic.Content, x.References.Select(Map).ToArray())).ToList();
        if (source.UnassignedReferences.Count > 0) sections.Add(new(null, "Referências utilizadas", null, source.UnassignedReferences.Select(Map).ToArray()));
        var codes = source.References.Select(x => x.BibleVersion?.Code).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
        var report = new SermonReport("PREGAÇÃO", source.Message.Title, source.Message.Description, source.Message.Introduction, source.Message.Conclusion,
            codes.Length == 1 ? $"{source.References[0].BibleVersion!.Code} — {source.References[0].BibleVersion!.DisplayName}" : "Versões indicadas em cada referência", DateTimeOffset.UtcNow, sections);
        return CreateSermonPdfAsync(report, cancellationToken);
    }

    public Task<string> CreateSermonPdfAsync(SermonReport report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); Directory.CreateDirectory(Path.Combine(_paths.CacheDirectory, "exports"));
        var path = Path.Combine(_paths.CacheDirectory, "exports", $"{Slug(report.Title)}-{report.GeneratedAt:yyyy-MM-dd}.pdf");
        var document = BuildDocument(report); var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument(); cancellationToken.ThrowIfCancellationRequested(); renderer.PdfDocument.Save(path); return Task.FromResult(path);
    }

    private static Document BuildDocument(SermonReport report)
    {
        var document = new Document { Info = { Title = report.Title, Subject = "Pregação gerada pelo BíbliaTema Premium Edition", Author = "WR Soft Serviços OnLine" } };
        var normal = document.Styles[StyleNames.Normal]!; normal.Font.Name = "Open Sans"; normal.Font.Size = 10.5; normal.Font.Color = C(Text); normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(5); normal.ParagraphFormat.LineSpacing = 1.18;
        var section = document.AddSection(); section.PageSetup.PageFormat = PageFormat.A4; section.PageSetup.TopMargin = Unit.FromCentimeter(2.4); section.PageSetup.BottomMargin = Unit.FromCentimeter(2.1); section.PageSetup.LeftMargin = Unit.FromCentimeter(1.8); section.PageSetup.RightMargin = Unit.FromCentimeter(1.8);
        AddHeader(section, report.DocumentType); AddFooter(section, report.GeneratedAt.Year);
        var eyebrow = section.AddParagraph(report.DocumentType); eyebrow.Format.SpaceBefore = Unit.FromPoint(8); eyebrow.Format.SpaceAfter = Unit.FromPoint(5); eyebrow.Format.Font = new Font("Open Sans", 9) { Bold = true, Color = C(Gold) };
        var title = section.AddParagraph(report.Title); title.Format.Font = new Font("Open Sans", 24) { Bold = true, Color = C(Header) }; title.Format.SpaceAfter = Unit.FromPoint(7); title.Format.KeepWithNext = true;
        if (!string.IsNullOrWhiteSpace(report.Subtitle)) { var subtitle = section.AddParagraph(report.Subtitle); subtitle.Format.Font = new Font("Open Sans", 12) { Color = C(Secondary) }; subtitle.Format.SpaceAfter = Unit.FromPoint(10); }
        var meta = section.AddParagraph(); meta.AddFormattedText(report.VersionLabel, TextFormat.Bold); meta.AddText($"   •   {report.GeneratedAt.ToLocalTime():dd/MM/yyyy}"); meta.Format.Font.Color = C(Secondary); meta.Format.SpaceAfter = Unit.FromPoint(16);
        if (!string.IsNullOrWhiteSpace(report.Introduction)) AddEditorialBlock(section, "INTRODUÇÃO", report.Introduction!, Gold);
        foreach (var reportSection in report.Sections)
        {
            var accent = SafeColor(reportSection.Theme?.ColorHex); var heading = section.AddParagraph(reportSection.Title.ToUpperInvariant()); heading.Format.Font = new Font("Open Sans", 15) { Bold = true, Color = C(Header) }; heading.Format.SpaceBefore = Unit.FromPoint(15); heading.Format.SpaceAfter = Unit.FromPoint(2); heading.Format.KeepWithNext = true;
            var rule = section.AddParagraph("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"); rule.Format.Font.Color = C(accent); rule.Format.Font.Size = 7; rule.Format.SpaceAfter = Unit.FromPoint(7); rule.Format.KeepWithNext = true;
            if (!string.IsNullOrWhiteSpace(reportSection.Content)) { var content = section.AddParagraph(reportSection.Content); content.Format.Font.Color = C(Secondary); content.Format.SpaceAfter = Unit.FromPoint(9); }
            if (reportSection.References.Count == 0) { var empty = section.AddParagraph("Nenhuma referência vinculada a este tema."); empty.Format.Font.Color = C(Secondary); empty.Format.Font.Italic = true; }
            foreach (var reference in reportSection.References) AddReference(section, reference, accent);
        }
        if (!string.IsNullOrWhiteSpace(report.Conclusion)) AddEditorialBlock(section, "CONCLUSÃO", report.Conclusion!, Gold);
        return document;
    }

    private static void AddHeader(Section section, string type)
    {
        var table = section.Headers.Primary.AddTable(); table.AddColumn(Unit.FromCentimeter(10.5)); table.AddColumn(Unit.FromCentimeter(5.7));
        var row = table.AddRow(); row.Shading.Color = C(Header); row.Height = Unit.FromCentimeter(.75); row.VerticalAlignment = VerticalAlignment.Center;
        var brand = row.Cells[0].AddParagraph(); brand.AddFormattedText("BÍBLIATEMA", TextFormat.Bold); brand.AddText("  Premium Edition"); brand.Format.Font.Color = Colors.White; brand.Format.Font.Size = 9;
        var label = row.Cells[1].AddParagraph(type); label.Format.Alignment = ParagraphAlignment.Right; label.Format.Font = new Font("Open Sans", 9) { Bold = true, Color = C(Gold) };
    }

    private static void AddFooter(Section section, int year)
    {
        var footer = section.Footers.Primary.AddParagraph(); footer.AddText($"BíbliaTema Premium Edition  •  © {year} WR Soft Serviços OnLine"); footer.AddTab(); footer.AddText("Página "); footer.AddPageField(); footer.AddText(" de "); footer.AddNumPagesField();
        footer.Format.TabStops.AddTabStop(Unit.FromCentimeter(16.2), TabAlignment.Right); footer.Format.Font = new Font("Open Sans", 8) { Color = C(Secondary) }; footer.Format.Borders.Top.Color = C(Border); footer.Format.Borders.Top.Width = Unit.FromPoint(.6); footer.Format.SpaceBefore = Unit.FromPoint(5);
    }

    private static void AddReference(Section section, SermonReportReference reference, string accent)
    {
        var heading = section.AddParagraph();
        var referenceText = heading.AddFormattedText(reference.FormattedReference.ToUpperInvariant()); referenceText.Font = new Font("Open Sans", 9.5) { Bold = true, Color = C(Primary) };
        heading.AddText("   ");
        var versionText = heading.AddFormattedText($"Versão: {reference.VersionCode}"); versionText.Font = new Font("Open Sans", 7.5) { Color = C(Secondary) };
        heading.Format.SpaceBefore = Unit.FromPoint(9); heading.Format.SpaceAfter = Unit.FromPoint(1); heading.Format.KeepWithNext = true;
        var passage = section.AddParagraph(); foreach (var verse in reference.Passage.Verses) { passage.AddFormattedText($"{verse.Verse} ", TextFormat.Bold); passage.AddText(verse.Text + " "); } passage.Format.SpaceBefore = Unit.FromPoint(0); passage.Format.SpaceAfter = Unit.FromPoint(4);
        if (reference.Themes.Count > 1) { var badges = section.AddParagraph("Temas: " + string.Join(" • ", reference.Themes.Select(x => x.Name))); badges.Format.Font = new Font("Open Sans", 8) { Bold = true, Color = C(accent) }; }
        if (!string.IsNullOrWhiteSpace(reference.Comment)) AddNote(section, "Comentário", reference.Comment!, accent);
        if (!string.IsNullOrWhiteSpace(reference.Observation)) AddNote(section, "Observação", reference.Observation!, Border);
    }

    private static void AddEditorialBlock(Section section, string title, string value, string accent)
    {
        var heading = section.AddParagraph(title); heading.Format.Font = new Font("Open Sans", 12) { Bold = true, Color = C(Header) }; heading.Format.SpaceBefore = Unit.FromPoint(12); heading.Format.KeepWithNext = true; AddNote(section, string.Empty, value, accent);
    }

    private static void AddNote(Section section, string title, string value, string accent)
    {
        var table = section.AddTable(); table.AddColumn(Unit.FromCentimeter(16.2)); table.Format.SpaceAfter = Unit.FromPoint(7); var cell = table.AddRow().Cells[0]; cell.Shading.Color = C(Surface); cell.Borders.Left.Color = C(accent); cell.Borders.Left.Width = Unit.FromPoint(3); cell.Borders.Top.Color = cell.Borders.Bottom.Color = cell.Borders.Right.Color = C(Border); cell.Borders.Top.Width = cell.Borders.Bottom.Width = cell.Borders.Right.Width = Unit.FromPoint(.4); cell.Format.SpaceBefore = cell.Format.SpaceAfter = Unit.FromPoint(5);
        var paragraph = cell.AddParagraph(); if (!string.IsNullOrEmpty(title)) { paragraph.AddFormattedText(title + "\n", TextFormat.Bold); } paragraph.AddText(value); paragraph.Format.Font.Size = 9.5;
    }

    private static SermonReportReference Map(MessageReportReference item) => new(item.SavedReference.Reference.Id, item.BookName, item.SavedReference.Reference.Chapter,
        item.SavedReference.Reference.VerseStart, item.SavedReference.Reference.VerseEnd, item.FormattedReference, item.Passage ?? throw new InvalidOperationException($"Texto indisponível para {item.FormattedReference}."),
        item.SavedReference.Reference.Comment, item.Link.Observation, item.BibleVersion?.Code ?? string.Empty, item.SavedReference.Themes);
    private static MigraDoc.DocumentObjectModel.Color C(string hex) => MigraDoc.DocumentObjectModel.Color.Parse(hex);
    private static string SafeColor(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length == 7 && value[0] == '#' && value[1..].All(Uri.IsHexDigit) ? value : Primary;
    private static string Slug(string value) { var normalized = value.Normalize(NormalizationForm.FormD); var builder = new StringBuilder("pregacao-"); foreach (var character in normalized) { if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue; if (char.IsLetterOrDigit(character)) builder.Append(char.ToLowerInvariant(character)); else if (builder.Length > 0 && builder[^1] != '-') builder.Append('-'); } return builder.ToString().TrimEnd('-'); }
    private static void EnsureFontResolver() { lock (FontLock) { if (GlobalFontSettings.FontResolver is null) GlobalFontSettings.FontResolver = new EmbeddedFontResolver(); } }
}

internal sealed class EmbeddedFontResolver : IFontResolver
{
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) => new(isBold ? "OpenSans-Semibold" : "OpenSans-Regular");
    public byte[]? GetFont(string faceName) { var resource = faceName == "OpenSans-Semibold" ? "Biblia.Fonts.OpenSans-Semibold.ttf" : "Biblia.Fonts.OpenSans-Regular.ttf"; using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) ?? throw new InvalidOperationException($"Fonte incorporada não encontrada: {resource}"); using var memory = new MemoryStream(); stream.CopyTo(memory); return memory.ToArray(); }
}
