using Biblia.Application.Interfaces;
using Biblia.Application.Interfaces.Repositories;
using Biblia.Domain.Entities;

namespace Biblia.Infrastructure.Repositories;

public sealed class SavedReferenceRepository : SqliteRepositoryBase, ISavedReferenceRepository
{
    private readonly IClock _clock;
    public SavedReferenceRepository(Biblia.Infrastructure.AppDatabase.AppDatabase database, IClock clock) : base(database) => _clock = clock;

    public async Task<SavedReference> CreateAsync(SavedReference item, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        await using var c = await Database.OpenConnectionAsync(cancellationToken);
        await using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO SavedReference(BookReferenceId,Chapter,VerseStart,VerseEnd,Comment,PreferredBibleVersionId,CreatedAt,UpdatedAt) VALUES($book,$chapter,$start,$end,$comment,$version,$created,$updated); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$book", item.BookReferenceId); cmd.Parameters.AddWithValue("$chapter", item.Chapter); cmd.Parameters.AddWithValue("$start", item.VerseStart); cmd.Parameters.AddWithValue("$end", item.VerseEnd);
        AddNullable(cmd.Parameters, "$comment", item.Comment); AddNullable(cmd.Parameters, "$version", item.PreferredBibleVersionId);
        cmd.Parameters.AddWithValue("$created", now.ToString("O")); cmd.Parameters.AddWithValue("$updated", now.ToString("O"));
        var id = Convert.ToInt64(await cmd.ExecuteScalarAsync(cancellationToken));
        return item with { Id = id, CreatedAt = now, UpdatedAt = now };
    }

    public async Task<SavedReference?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var c = await Database.OpenConnectionAsync(cancellationToken); await using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id,BookReferenceId,Chapter,VerseStart,VerseEnd,Comment,PreferredBibleVersionId,CreatedAt,UpdatedAt FROM SavedReference WHERE Id=$id;"; cmd.Parameters.AddWithValue("$id", id);
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        return await r.ReadAsync(cancellationToken) ? new(r.GetInt64(0),r.GetInt32(1),r.GetInt32(2),r.GetInt32(3),r.GetInt32(4),r.IsDBNull(5)?null:r.GetString(5),r.IsDBNull(6)?null:r.GetInt64(6),ReadDate(r,7),ReadDate(r,8)) : null;
    }

    public async Task<SavedReferenceDetails?> GetDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        var reference = await GetAsync(id, cancellationToken);
        return reference is null ? null : new SavedReferenceDetails(reference, await GetThemesAsync(id, cancellationToken));
    }

    public async Task<SavedReference?> FindCanonicalAsync(int bookReferenceId, int chapter, int verseStart, int verseEnd, CancellationToken cancellationToken = default)
    {
        await using var c = await Database.OpenConnectionAsync(cancellationToken);
        await using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id,BookReferenceId,Chapter,VerseStart,VerseEnd,Comment,PreferredBibleVersionId,CreatedAt,UpdatedAt FROM SavedReference WHERE BookReferenceId=$book AND Chapter=$chapter AND VerseStart=$start AND VerseEnd=$end ORDER BY Id LIMIT 1;";
        cmd.Parameters.AddWithValue("$book", bookReferenceId); cmd.Parameters.AddWithValue("$chapter", chapter); cmd.Parameters.AddWithValue("$start", verseStart); cmd.Parameters.AddWithValue("$end", verseEnd);
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        return await r.ReadAsync(cancellationToken) ? Map(r) : null;
    }

    public async Task<IReadOnlyList<SavedReferenceDetails>> SearchAsync(string? query, CancellationToken cancellationToken = default)
    {
        var result = new List<SavedReferenceDetails>();
        var references = new List<SavedReference>();
        await using (var c = await Database.OpenConnectionAsync(cancellationToken))
        await using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = """
                SELECT Id,BookReferenceId,Chapter,VerseStart,VerseEnd,Comment,PreferredBibleVersionId,CreatedAt,UpdatedAt
                FROM SavedReference
                WHERE $query IS NULL
                   OR CAST(BookReferenceId AS TEXT) LIKE '%' || $query || '%'
                   OR CAST(Chapter AS TEXT) LIKE '%' || $query || '%'
                   OR COALESCE(Comment, '') LIKE '%' || $query || '%' COLLATE NOCASE
                ORDER BY UpdatedAt DESC;
                """;
            AddNullable(cmd.Parameters, "$query", string.IsNullOrWhiteSpace(query) ? null : query.Trim());
            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await r.ReadAsync(cancellationToken)) references.Add(Map(r));
        }
        foreach (var reference in references) result.Add(new SavedReferenceDetails(reference, await GetThemesAsync(reference.Id, cancellationToken)));
        return result;
    }

    public async Task<IReadOnlyList<SavedReferenceDetails>> GetByThemeIdsAsync(IReadOnlyCollection<long> themeIds, CancellationToken cancellationToken = default)
    {
        if (themeIds.Count == 0) return [];
        var ids = themeIds.Distinct().ToArray();
        var references = new List<SavedReference>();
        await using (var connection = await Database.OpenConnectionAsync(cancellationToken))
        await using (var command = connection.CreateCommand())
        {
            var parameters = ids.Select((_, index) => $"$theme{index}").ToArray();
            command.CommandText = $"""
                SELECT DISTINCT r.Id,r.BookReferenceId,r.Chapter,r.VerseStart,r.VerseEnd,r.Comment,r.PreferredBibleVersionId,r.CreatedAt,r.UpdatedAt
                FROM SavedReference r
                INNER JOIN ReferenceTheme rt ON rt.ReferenceId=r.Id
                WHERE rt.ThemeId IN ({string.Join(',', parameters)})
                ORDER BY r.BookReferenceId,r.Chapter,r.VerseStart,r.VerseEnd,r.Id;
                """;
            for (var index = 0; index < ids.Length; index++) command.Parameters.AddWithValue(parameters[index], ids[index]);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) references.Add(Map(reader));
        }
        var result = new List<SavedReferenceDetails>(references.Count);
        foreach (var reference in references) result.Add(new(reference, await GetThemesAsync(reference.Id, cancellationToken)));
        return result;
    }

    public async Task UpdateAsync(SavedReference item, CancellationToken cancellationToken = default)
    {
        await using var c = await Database.OpenConnectionAsync(cancellationToken); await using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE SavedReference SET BookReferenceId=$book,Chapter=$chapter,VerseStart=$start,VerseEnd=$end,Comment=$comment,PreferredBibleVersionId=$version,UpdatedAt=$updated WHERE Id=$id;";
        cmd.Parameters.AddWithValue("$book",item.BookReferenceId); cmd.Parameters.AddWithValue("$chapter",item.Chapter); cmd.Parameters.AddWithValue("$start",item.VerseStart); cmd.Parameters.AddWithValue("$end",item.VerseEnd); AddNullable(cmd.Parameters,"$comment",item.Comment); AddNullable(cmd.Parameters,"$version",item.PreferredBibleVersionId); cmd.Parameters.AddWithValue("$updated",_clock.UtcNow.ToString("O")); cmd.Parameters.AddWithValue("$id",item.Id);
        if(await cmd.ExecuteNonQueryAsync(cancellationToken)!=1) throw new KeyNotFoundException("Referência não encontrada.");
    }

    public Task AddThemeAsync(long referenceId,long themeId,CancellationToken cancellationToken=default) => ExecuteLinkAsync("INSERT INTO ReferenceTheme(ReferenceId,ThemeId) VALUES($reference,$theme);",referenceId,themeId,cancellationToken);
    public Task RemoveThemeAsync(long referenceId,long themeId,CancellationToken cancellationToken=default) => ExecuteLinkAsync("DELETE FROM ReferenceTheme WHERE ReferenceId=$reference AND ThemeId=$theme;",referenceId,themeId,cancellationToken);
    public async Task SetThemesAsync(long referenceId, IReadOnlyCollection<long> themeIds, CancellationToken cancellationToken = default)
    {
        await using var c = await Database.OpenConnectionAsync(cancellationToken);
        await using var transaction = c.BeginTransaction();
        try
        {
            await using var delete = c.CreateCommand();
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM ReferenceTheme WHERE ReferenceId=$reference;";
            delete.Parameters.AddWithValue("$reference", referenceId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
            foreach (var themeId in themeIds.Distinct())
            {
                await using var insert = c.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = "INSERT INTO ReferenceTheme(ReferenceId,ThemeId) VALUES($reference,$theme);";
                insert.Parameters.AddWithValue("$reference", referenceId);
                insert.Parameters.AddWithValue("$theme", themeId);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }
    private async Task ExecuteLinkAsync(string sql,long referenceId,long themeId,CancellationToken token){await using var c=await Database.OpenConnectionAsync(token);await using var cmd=c.CreateCommand();cmd.CommandText=sql;cmd.Parameters.AddWithValue("$reference",referenceId);cmd.Parameters.AddWithValue("$theme",themeId);await cmd.ExecuteNonQueryAsync(token);}

    public async Task DeleteAsync(long id,CancellationToken cancellationToken=default){await using var c=await Database.OpenConnectionAsync(cancellationToken);await using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM SavedReference WHERE Id=$id;";cmd.Parameters.AddWithValue("$id",id);await cmd.ExecuteNonQueryAsync(cancellationToken);}

    private async Task<IReadOnlyList<Theme>> GetThemesAsync(long referenceId, CancellationToken token)
    {
        await using var c = await Database.OpenConnectionAsync(token);
        return await GetThemesAsync(c, referenceId, token);
    }

    private static async Task<IReadOnlyList<Theme>> GetThemesAsync(Microsoft.Data.Sqlite.SqliteConnection c, long referenceId, CancellationToken token)
    {
        var themes = new List<Theme>();
        await using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT t.Id,t.Name,t.ColorHex,t.Description,t.CreatedAt,t.UpdatedAt FROM Theme t INNER JOIN ReferenceTheme rt ON rt.ThemeId=t.Id WHERE rt.ReferenceId=$reference ORDER BY t.Name COLLATE NOCASE;";
        cmd.Parameters.AddWithValue("$reference", referenceId);
        await using var r = await cmd.ExecuteReaderAsync(token);
        while (await r.ReadAsync(token)) themes.Add(new Theme(r.GetInt64(0), r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2), r.IsDBNull(3) ? null : r.GetString(3), ReadDate(r, 4), ReadDate(r, 5)));
        return themes;
    }

    private static SavedReference Map(Microsoft.Data.Sqlite.SqliteDataReader r) => new(r.GetInt64(0),r.GetInt32(1),r.GetInt32(2),r.GetInt32(3),r.GetInt32(4),r.IsDBNull(5)?null:r.GetString(5),r.IsDBNull(6)?null:r.GetInt64(6),ReadDate(r,7),ReadDate(r,8));
}
