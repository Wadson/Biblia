using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Domain.Exceptions;
namespace Biblia.Application.Services;
public sealed class MessageService(IMessageRepository repository, IClock clock) : IMessageService
{
 public async Task<IReadOnlyList<Message>> SearchAsync(string? query, MessageType? type=null,CancellationToken cancellationToken=default){var all=await repository.GetAllAsync(cancellationToken);return all.Where(x=>(type is null||x.Type==type)&&(string.IsNullOrWhiteSpace(query)||x.Title.Contains(query.Trim(),StringComparison.OrdinalIgnoreCase)||(x.Description?.Contains(query.Trim(),StringComparison.OrdinalIgnoreCase)??false))).ToArray();}
 public async Task<Message> SaveAsync(long? id,string? title,string? description,MessageType type,string? introduction=null,string? conclusion=null,long? preferredBibleVersionId=null,CancellationToken cancellationToken=default){if(string.IsNullOrWhiteSpace(title))throw new DomainValidationException("O título da mensagem é obrigatório.");var now=clock.UtcNow;var clean=title.Trim();var desc=CleanOptional(description);var intro=CleanOptional(introduction);var ending=CleanOptional(conclusion);if(id is null or <=0)return await repository.CreateAsync(new Message(0,clean,desc,type,now,now,intro,ending,preferredBibleVersionId),cancellationToken);var current=await repository.GetAsync(id.Value,cancellationToken)??throw new KeyNotFoundException("Mensagem não encontrada.");var changed=current with{Title=clean,Description=desc,Type=type,Introduction=intro,Conclusion=ending,PreferredBibleVersionId=preferredBibleVersionId,UpdatedAt=now};await repository.UpdateAsync(changed,cancellationToken);return (await repository.GetAsync(changed.Id,cancellationToken))!;}
 private static string? CleanOptional(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
 public Task DeleteAsync(long id,CancellationToken cancellationToken=default)=>repository.DeleteAsync(id,cancellationToken);
}
