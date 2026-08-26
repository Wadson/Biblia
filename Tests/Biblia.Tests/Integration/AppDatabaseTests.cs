using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;
using Biblia.Domain.Enums;
using Biblia.Infrastructure.AppDatabase;
using Biblia.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#pragma warning disable xUnit1051 // This short local-database test has deterministic sub-second operations.

namespace Biblia.Tests.Integration;

public sealed class AppDatabaseTests
{
    [Fact]
    public async Task InitializeAndRepositories_ProvideVersionedConstrainedCrud()
    {
        var directory = Path.Combine(Path.GetTempPath(), "BibliaTema.Tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "app.db");
        Directory.CreateDirectory(directory);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero));
            var database = new AppDatabase(path, NullLogger<AppDatabase>.Instance);
            await database.InitializeAsync();
            await database.InitializeAsync();
            Assert.Equal(AppDatabase.CurrentSchemaVersion, await database.GetSchemaVersionAsync());

            var themes = new ThemeRepository(database, clock);
            var theme = await themes.CreateAsync("Graça", "#336699", "Doutrina da graça");
            Assert.Equal("Graça", (await themes.GetAsync(theme.Id))!.Name);
            await Assert.ThrowsAsync<Biblia.Domain.Exceptions.DomainValidationException>(() => themes.CreateAsync("graça", null, null));
            await Assert.ThrowsAsync<SqliteException>(() => themes.CreateAsync("Inválido", "azul", null));
            await themes.UpdateAsync(theme with { Description = "Descrição atualizada" });
            Assert.Equal("Descrição atualizada", (await themes.GetAsync(theme.Id))!.Description);

            var catalog = new BibleVersionCatalogRepository(database);
            var version = await catalog.CreateAsync(new BibleVersionCatalogEntry(0, "ACF", "Almeida Corrigida e Fiel", "pt-BR", "ACF.sqlite", null, 2, null, null, null, false, false, true, null, clock.UtcNow, BibleVersionValidationStatus.Compatible, null));
            Assert.Equal("ACF", (await catalog.GetByCodeAsync("acf"))!.Code);
            await catalog.UpdateAsync(version with { IsInstalled = true, IsEnabled = true, InstalledPath = "bibles/ACF.sqlite" });
            Assert.True((await catalog.GetByCodeAsync("ACF"))!.IsInstalled);

            var references = new SavedReferenceRepository(database, clock);
            var reference = await references.CreateAsync(new SavedReference(0, 43, 3, 16, 18, "Comentário permanente", version.Id, default, default));
            await references.AddThemeAsync(reference.Id, theme.Id);
            await Assert.ThrowsAsync<SqliteException>(() => references.AddThemeAsync(reference.Id, theme.Id));
            Assert.Single((await references.GetDetailsAsync(reference.Id))!.Themes);
            Assert.Single(await references.SearchAsync("Comentário"));
            await references.SetThemesAsync(reference.Id, []);
            Assert.Empty((await references.GetDetailsAsync(reference.Id))!.Themes);
            await references.SetThemesAsync(reference.Id, [theme.Id]);
            await references.UpdateAsync(reference with { Comment = "Comentário revisado" });
            Assert.Equal("Comentário revisado", (await references.GetAsync(reference.Id))!.Comment);
            await Assert.ThrowsAsync<SqliteException>(() => references.CreateAsync(new SavedReference(0, 67, 1, 1, 1, null, null, default, default)));

            var messages = new MessageRepository(database);
            var message = await messages.CreateAsync(new Message(0, "Servindo com fidelidade", null, MessageType.Sermon, clock.UtcNow, clock.UtcNow, "Abertura", "Apelo final", version.Id));
            var topic = await messages.AddTopicAsync(new MessageTopic(0, message.Id, "Introdução", "Conteúdo", 0));
            await messages.AddReferenceAsync(new MessageReference(0, message.Id, reference.Id, topic.Id, 0, "Usar na introdução", version.Id));
            Assert.Equal(new ThemeUsage(1, 1), await themes.GetUsageAsync(theme.Id));
            var copy = await messages.DuplicateAsync(message.Id);
            Assert.Equal("Servindo com fidelidade (cópia)", copy.Title);
            var copiedTopics = await messages.GetTopicsAsync(copy.Id);
            Assert.Single(copiedTopics);
            var copiedReferences = await messages.GetReferencesAsync(copy.Id);
            Assert.Single(copiedReferences);
            Assert.Equal(reference.Id, copiedReferences[0].ReferenceId);
            Assert.Equal(copiedTopics[0].Id, copiedReferences[0].TopicId);
            Assert.Equal("Usar na introdução", copiedReferences[0].Observation);
            Assert.Equal("Abertura", copy.Introduction);
            Assert.Equal("Apelo final", copy.Conclusion);
            Assert.Equal(version.Id, copy.PreferredBibleVersionId);
            await messages.UpdateAsync(message with { Description = "Descrição", Introduction = "Introdução revisada" });
            var updatedMessage = (await messages.GetAsync(message.Id))!;
            Assert.Equal("Descrição", updatedMessage.Description);
            Assert.Equal("Introdução revisada", updatedMessage.Introduction);

            var settings = new SettingsRepository(database, clock);
            await settings.SetAsync("theme", "dark");
            await settings.SetAsync("theme", "light");
            Assert.Equal("light", (await settings.GetAsync("THEME"))!.Value);
            await settings.DeleteAsync("theme");
            Assert.Null(await settings.GetAsync("theme"));

            await messages.DeleteAsync(message.Id);
            await messages.DeleteAsync(copy.Id);
            Assert.Equal(0, await CountAsync(database, "MessageTopic"));
            Assert.Equal(0, await CountAsync(database, "MessageReference"));
            await themes.DeleteAsync(theme.Id);
            Assert.Equal(0, await CountAsync(database, "ReferenceTheme"));
            await references.DeleteAsync(reference.Id);
            await catalog.DeleteAsync(version.Id);
            Assert.Null(await catalog.GetByCodeAsync("ACF"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static async Task<long> CountAsync(AppDatabase database, string table)
    {
        await using var connection = await database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = table switch
        {
            "MessageTopic" => "SELECT COUNT(*) FROM MessageTopic;",
            "MessageReference" => "SELECT COUNT(*) FROM MessageReference;",
            "ReferenceTheme" => "SELECT COUNT(*) FROM ReferenceTheme;",
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private sealed class FixedClock(DateTimeOffset value) : IClock
    {
        public DateTimeOffset UtcNow { get; } = value;
    }
}

#pragma warning restore xUnit1051
