using Biblia.Domain.Entities;
namespace Biblia.Application.Interfaces;
public interface IMessageTopicService { Task<IReadOnlyList<MessageTopic>> GetTopicsAsync(long messageId,CancellationToken cancellationToken=default); Task<MessageTopic> SaveAsync(long messageId,long? id,string? title,string? content,CancellationToken cancellationToken=default); Task DeleteAsync(long id,CancellationToken cancellationToken=default); Task MoveAsync(long messageId,IReadOnlyList<MessageTopic> current,long topicId,int direction,CancellationToken cancellationToken=default); }
