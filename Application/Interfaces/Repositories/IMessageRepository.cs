using Biblia.Domain.Entities;

namespace Biblia.Application.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);
    Task<Message?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
    Task<MessageTopic> AddTopicAsync(MessageTopic topic, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageTopic>> GetTopicsAsync(long messageId, CancellationToken cancellationToken = default);
    Task UpdateTopicAsync(MessageTopic topic, CancellationToken cancellationToken = default);
    Task DeleteTopicAsync(long id, CancellationToken cancellationToken = default);
    Task ReorderTopicsAsync(long messageId, IReadOnlyList<long> orderedTopicIds, CancellationToken cancellationToken = default);
    Task<MessageReference> AddReferenceAsync(MessageReference reference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageReference>> GetReferencesAsync(long messageId, CancellationToken cancellationToken = default);
    Task UpdateReferenceAsync(MessageReference reference, CancellationToken cancellationToken = default);
    Task DeleteReferenceAsync(long id, CancellationToken cancellationToken = default);
    Task ReorderReferencesAsync(long messageId, IReadOnlyList<long> orderedReferenceIds, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Reordenação não implementada pelo repositório.");
    Task<int> GetNextReferenceOrderAsync(long messageId,CancellationToken cancellationToken=default);
    Task<Message> DuplicateAsync(long messageId, string? title = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Duplicação não implementada pelo repositório.");
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
