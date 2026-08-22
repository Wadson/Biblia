using Biblia.Domain.Entities;
namespace Biblia.Application.Interfaces;
public interface IPdfService
{
    Task<string> CreateMessagePdfAsync(MessageReport report, CancellationToken cancellationToken = default);
    Task<string> CreateSermonPdfAsync(SermonReport report, CancellationToken cancellationToken = default);
}
