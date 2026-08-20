using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
namespace Biblia.Application.Interfaces;
public interface IMessageService
{
    Task<IReadOnlyList<Message>> SearchAsync(string? query, MessageType? type = null, CancellationToken cancellationToken = default);
    Task<Message> SaveAsync(long? id, string? title, string? description, MessageType type,
        string? introduction = null, string? conclusion = null, long? preferredBibleVersionId = null,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
