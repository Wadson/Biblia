using Biblia.Application.Interfaces;
using Biblia.Application.Services;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Infrastructure.AppDatabase;
using Biblia.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Biblia.Tests.Application;

#pragma warning disable xUnit1051
public sealed class MessageReferenceFlowTests
{
    [Fact]
    public async Task CanonicalReuse_UnionsThemes_AndRemovingLinkPreservesReference()
    {
        var directory=Path.Combine(Path.GetTempPath(),"BibliaTema.Tests",Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        try
        {
            var clock=new FixedClock(DateTimeOffset.Parse("2026-08-19T12:00:00Z")); var database=new AppDatabase(Path.Combine(directory,"app.db"),NullLogger<AppDatabase>.Instance); await database.InitializeAsync();
            var themeRepository=new ThemeRepository(database,clock); var salvation=await themeRepository.CreateAsync("Salvação","#336699",null); var love=await themeRepository.CreateAsync("Amor","#AA3355",null);
            var savedRepository=new SavedReferenceRepository(database,clock); var savedService=new SavedReferenceService(savedRepository);
            var first=await savedService.GetOrCreateCanonicalAsync(43,3,16,18,null,[salvation.Id]);
            var reused=await savedService.GetOrCreateCanonicalAsync(43,3,16,18,null,[love.Id]);
            Assert.Equal(first.Reference.Id,reused.Reference.Id); Assert.Equal(2,reused.Themes.Count);

            var messageRepository=new MessageRepository(database); var messageService=new MessageService(messageRepository,clock); var message=await messageService.SaveAsync(null,"O Amor de Deus",null,MessageType.Sermon,preferredBibleVersionId:null);
            var links=new MessageReferenceService(messageRepository); var link=await links.AddAsync(message.Id,reused.Reference.Id,null,"Texto principal",null);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>links.AddAsync(message.Id,reused.Reference.Id,null,null,null));
            await links.DeleteAsync(link.Id);
            Assert.NotNull(await savedService.GetDetailsAsync(reused.Reference.Id)); Assert.Equal(2,(await savedService.GetDetailsAsync(reused.Reference.Id))!.Themes.Count);
        }
        finally { SqliteConnection.ClearAllPools(); if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }

    [Fact]
    public async Task CompleteMessageLink_PersistsCommentTopicAndIndependentObservations()
    {
        var directory=Path.Combine(Path.GetTempPath(),"BibliaTema.Tests",Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        try
        {
            var clock=new FixedClock(DateTimeOffset.Parse("2026-08-20T12:00:00Z")); var database=new AppDatabase(Path.Combine(directory,"app.db"),NullLogger<AppDatabase>.Instance); await database.InitializeAsync();
            var themeRepository=new ThemeRepository(database,clock); var promises=await themeRepository.CreateAsync("Promessas","#D4AF37",null); var holiness=await themeRepository.CreateAsync("Santidade","#0066CC",null);
            var savedService=new SavedReferenceService(new SavedReferenceRepository(database,clock));
            var initial=await savedService.SaveAsync(null,1,1,29,31,null,null,[promises.Id]);
            var updated=await savedService.SaveAsync(initial.Reference.Id,1,1,29,31,"Deus estabelece a provisão.",null,initial.Themes.Select(x=>x.Id).Append(holiness.Id).ToArray());
            Assert.Equal("Deus estabelece a provisão.",updated.Reference.Comment); Assert.Equal(["Promessas","Santidade"],updated.Themes.Select(x=>x.Name).Order().ToArray());

            var messageRepository=new MessageRepository(database); var messages=new MessageService(messageRepository,clock); var topics=new MessageTopicService(messageRepository); var links=new MessageReferenceService(messageRepository);
            var firstMessage=await messages.SaveAsync(null,"Sermão de Teste",null,MessageType.Sermon,preferredBibleVersionId:null); var topic=await topics.SaveAsync(firstMessage.Id,null,"Introdução",null);
            var firstLink=await links.AddAsync(firstMessage.Id,updated.Reference.Id,topic.Id,"Usar na abertura.",null);
            var secondMessage=await messages.SaveAsync(null,"Outra mensagem",null,MessageType.Sermon,preferredBibleVersionId:null); var secondLink=await links.AddAsync(secondMessage.Id,updated.Reference.Id,null,"Observação independente.",null);

            Assert.Equal(firstLink.ReferenceId,secondLink.ReferenceId); Assert.Equal("Usar na abertura.",(await links.GetAsync(firstMessage.Id)).Single().Observation); Assert.Equal(topic.Id,(await links.GetAsync(firstMessage.Id)).Single().TopicId); Assert.Equal("Observação independente.",(await links.GetAsync(secondMessage.Id)).Single().Observation);
            await links.DeleteAsync(firstLink.Id);
            Assert.Empty(await links.GetAsync(firstMessage.Id)); Assert.NotNull(await savedService.GetDetailsAsync(updated.Reference.Id)); Assert.Equal(2,(await savedService.GetDetailsAsync(updated.Reference.Id))!.Themes.Count);
        }
        finally { SqliteConnection.ClearAllPools(); if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }

    [Fact]
    public async Task TopicFlow_CreatesUpdatesReordersAndDeletesWithinMessage()
    {
        var directory=Path.Combine(Path.GetTempPath(),"BibliaTema.Tests",Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        try
        {
            var clock=new FixedClock(DateTimeOffset.Parse("2026-08-20T15:00:00Z")); var database=new AppDatabase(Path.Combine(directory,"app.db"),NullLogger<AppDatabase>.Instance); await database.InitializeAsync();
            var repository=new MessageRepository(database); var messages=new MessageService(repository,clock); var topics=new MessageTopicService(repository);
            var message=await messages.SaveAsync(null,"Mensagem com tópicos",null,MessageType.Sermon,preferredBibleVersionId:null);
            var introduction=await topics.SaveAsync(message.Id,null,"Introdução","Conteúdo inicial"); var conclusion=await topics.SaveAsync(message.Id,null,"Conclusão","Aplicação");
            var updated=await topics.SaveAsync(message.Id,introduction.Id,"Abertura","Conteúdo atualizado");
            Assert.Equal("Abertura",updated.Title); Assert.Equal("Conteúdo atualizado",updated.Content);
            await topics.MoveAsync(message.Id,await topics.GetTopicsAsync(message.Id),conclusion.Id,-1);
            Assert.Equal([conclusion.Id,introduction.Id],(await topics.GetTopicsAsync(message.Id)).Select(x=>x.Id).ToArray());
            await topics.DeleteAsync(conclusion.Id);
            var remaining=Assert.Single(await topics.GetTopicsAsync(message.Id)); Assert.Equal(introduction.Id,remaining.Id);
        }
        finally { SqliteConnection.ClearAllPools(); if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }
    private sealed class FixedClock(DateTimeOffset value):IClock { public DateTimeOffset UtcNow{get;}=value; }
}
#pragma warning restore xUnit1051
