using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Biblia.Application.Interfaces;
using Biblia.Domain.Entities;
using Biblia.Presentation.Commands;

namespace Biblia.Presentation.ViewModels;

public sealed class ReportsViewModel : INotifyPropertyChanged
{
    private readonly IReportService _reports;
    private readonly IMessageService _messages;
    private Message? _selectedMessage;
    private ReportsOverview? _overview;
    private string _preview = "Selecione uma mensagem para montar a visualização.", _status = "";
    private bool _busy;
    private MessageReport? _report;

    public ObservableCollection<Message> Messages { get; } = [];
    public Message? SelectedMessage { get => _selectedMessage; set { if (Set(ref _selectedMessage, value)) BuildCommand.NotifyCanExecuteChanged(); } }
    public ReportsOverview? Overview { get => _overview; private set => Set(ref _overview, value); }
    public string Preview { get => _preview; private set => Set(ref _preview, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public bool IsBusy { get => _busy; private set { if (Set(ref _busy, value)) { LoadCommand.NotifyCanExecuteChanged(); BuildCommand.NotifyCanExecuteChanged(); } } }
    public AsyncCommand LoadCommand { get; }
    public AsyncCommand BuildCommand { get; }
    public AsyncCommand PdfCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly IPdfService _pdf;
    public ReportsViewModel(IReportService reports, IMessageService messages, IPdfService pdf)
    {
        _reports = reports;
        _messages = messages;
        _pdf = pdf;
        LoadCommand = new AsyncCommand(LoadAsync, () => !IsBusy);
        BuildCommand = new AsyncCommand(BuildAsync, () => SelectedMessage is not null && !IsBusy);
        PdfCommand = new AsyncCommand(CreatePdfAsync, () => _report is not null && !IsBusy);
    }

    private async Task LoadAsync() => await RunAsync(async () =>
    {
        Overview = await _reports.GetOverviewAsync();
        var selectedId = SelectedMessage?.Id;
        Messages.Clear();
        foreach (var message in await _messages.SearchAsync(null)) Messages.Add(message);
        SelectedMessage = Messages.FirstOrDefault(x => x.Id == selectedId) ?? Messages.FirstOrDefault();
        Status = "Relatórios atualizados.";
    });

    private async Task BuildAsync() => await RunAsync(async () =>
    {
        _report = await _reports.BuildMessageAsync(SelectedMessage!.Id);
        Preview = Format(_report);
        Status = "Relatório de mensagem preparado.";
        PdfCommand.NotifyCanExecuteChanged();
    });
    private async Task CreatePdfAsync() => await RunAsync(async () => { var path = await _pdf.CreateMessagePdfAsync(_report!); Status = $"PDF salvo em {path}"; });

    private static string Format(MessageReport report)
    {
        var output = new StringBuilder();
        output.AppendLine(report.Message.Title.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(report.Message.Description)) output.AppendLine().AppendLine(report.Message.Description);
        if (report.BibleVersion is not null) output.AppendLine().AppendLine($"Versão bíblica: {report.BibleVersion.DisplayName} ({report.BibleVersion.Code})");
        if (!string.IsNullOrWhiteSpace(report.Message.Introduction)) output.AppendLine().AppendLine("INTRODUÇÃO").AppendLine(report.Message.Introduction);
        foreach (var topic in report.Topics)
        {
            output.AppendLine().AppendLine(topic.Topic.Title.ToUpperInvariant());
            if (!string.IsNullOrWhiteSpace(topic.Topic.Content)) output.AppendLine(topic.Topic.Content);
            AppendReferences(output, topic.References);
        }
        if (!string.IsNullOrWhiteSpace(report.Message.Conclusion)) output.AppendLine().AppendLine("CONCLUSÃO").AppendLine(report.Message.Conclusion);
        if (report.UnassignedReferences.Count > 0) { output.AppendLine().AppendLine("REFERÊNCIAS UTILIZADAS"); AppendReferences(output, report.UnassignedReferences); }
        return output.ToString().Trim();
    }

    private static void AppendReferences(StringBuilder output, IReadOnlyList<MessageReportReference> references)
    {
        foreach (var item in references)
        {
            var reference = item.SavedReference.Reference;
            output.AppendLine().AppendLine($"Referência: livro {reference.BookReferenceId}, {reference.Chapter}:{reference.VerseStart}-{reference.VerseEnd}");
            if (item.Passage is not null) output.AppendLine(string.Join(Environment.NewLine, item.Passage.Verses.Select(verse => $"{verse.Verse} {verse.Text}")));
            if (!string.IsNullOrWhiteSpace(reference.Comment)) output.AppendLine($"Comentário: {reference.Comment}");
            if (!string.IsNullOrWhiteSpace(item.Link.Observation)) output.AppendLine($"Observação: {item.Link.Observation}");
        }
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await action(); }
        catch (Exception exception) { Status = exception.Message; }
        finally { IsBusy = false; PdfCommand.NotifyCanExecuteChanged(); }
    }
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; PropertyChanged?.Invoke(this, new(name)); return true; }
}
