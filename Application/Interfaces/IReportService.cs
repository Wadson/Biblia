using Biblia.Domain.Entities;

namespace Biblia.Application.Interfaces;

public interface IReportService
{
    Task<ReportsOverview> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<MessageReport> BuildMessageAsync(long messageId, CancellationToken cancellationToken = default);
    Task<SermonReport> BuildFromMessageAsync(long messageId, string? title = null, string? subtitle = null, string? introduction = null, string? conclusion = null, CancellationToken cancellationToken = default);
    Task<SermonReport> BuildFromThemesAsync(SermonReportRequest request, CancellationToken cancellationToken = default);
}
