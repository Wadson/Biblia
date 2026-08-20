using System.Text;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;

namespace Biblia.Infrastructure.Files;

public sealed class PdfService(IAppPaths paths) : IPdfService
{
    public async Task<string> CreateMessagePdfAsync(MessageReport report, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.Combine(paths.CacheDirectory, "exports"));
        var path = Path.Combine(paths.CacheDirectory, "exports", $"mensagem-{report.Message.Id}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf");
        var lines = BuildLines(report); var pageLines = lines.Chunk(44).ToArray(); var objects = new List<string>();
        objects.Add("<< /Type /Catalog /Pages 2 0 R >>"); objects.Add($"<< /Type /Pages /Kids [{string.Join(' ', Enumerable.Range(0, pageLines.Length).Select(i => $"{3 + i * 2} 0 R"))}] /Count {pageLines.Length} >>");
        for (var i = 0; i < pageLines.Length; i++) { var content = string.Join("\n", pageLines[i].Select((line, index) => $"BT /F1 {(index == 0 ? 16 : 10)} Tf 50 {790 - index * 16} Td ({Escape(line)}) Tj ET")); objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {4 + i * 2} 0 R >> >> /Contents {5 + i * 2} 0 R >>"); objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"); objects.Add($"<< /Length {Encoding.Latin1.GetByteCount(content)} >>\nstream\n{content}\nendstream"); }
        await using var stream = File.Create(path); await using var writer = new StreamWriter(stream, Encoding.Latin1, leaveOpen: true); await writer.WriteAsync("%PDF-1.4\n"); var offsets = new List<long>{0}; for(var i=0;i<objects.Count;i++){offsets.Add(stream.Position);await writer.WriteAsync($"{i+1} 0 obj\n{objects[i]}\nendobj\n");await writer.FlushAsync();}var xref=stream.Position;await writer.WriteAsync($"xref\n0 {objects.Count+1}\n0000000000 65535 f \n");foreach(var offset in offsets.Skip(1))await writer.WriteAsync($"{offset:D10} 00000 n \n");await writer.WriteAsync($"trailer << /Size {objects.Count+1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");await writer.FlushAsync();return path;
    }
    private static string[] BuildLines(MessageReport report) { var lines=new List<string>{report.Message.Title.ToUpperInvariant(),report.Message.Description??"",$"Versão: {report.BibleVersion?.Code??"padrão"}","", "INTRODUÇÃO",report.Message.Introduction??""};foreach(var topic in report.Topics){lines.Add("");lines.Add(topic.Topic.Title.ToUpperInvariant());lines.Add(topic.Topic.Content??"");foreach(var item in topic.References){var r=item.SavedReference.Reference;lines.Add($"Referência: {r.BookReferenceId} {r.Chapter}:{r.VerseStart}-{r.VerseEnd}");lines.AddRange(item.Passage?.Verses.Select(v=>$"{v.Verse} {v.Text}")??[]);if(!string.IsNullOrWhiteSpace(r.Comment))lines.Add($"Comentário: {r.Comment}");if(!string.IsNullOrWhiteSpace(item.Link.Observation))lines.Add($"Observação: {item.Link.Observation}");}}lines.Add("");lines.Add("CONCLUSÃO");lines.Add(report.Message.Conclusion??"");return lines.SelectMany(l=>Wrap(l,95)).ToArray(); }
    private static IEnumerable<string> Wrap(string value,int width){while(value.Length>width){yield return value[..width];value=value[width..];}yield return value;}
    private static string Escape(string text)=>text.Replace("\\","\\\\").Replace("(","\\(").Replace(")","\\)").Replace("\r","").Replace("\n"," ");
}
