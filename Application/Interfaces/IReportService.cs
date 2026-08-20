using Biblia.Domain.Entities;

namespace Biblia.Application.Interfaces;

public interface IReportService
{
    Task<ReportsOverview> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<MessageReport> BuildMessageAsync(long messageId, CancellationToken cancellationToken = default);
}
