using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Data;

namespace StudyForge.Api.Tests;

/// <summary>
/// Cria um <see cref="AppDbContext"/> isolado por teste, apoiado em um banco SQLite
/// em memória. A conexão é mantida aberta pelo tempo de vida do contexto para que o
/// banco (que existe apenas enquanto a conexão viver) não seja descartado entre operações.
/// O SQLite preserva o comportamento relacional real (ex.: exclusão em cascata), ao
/// contrário do provedor InMemory.
/// </summary>
public sealed class SqliteInMemoryContext : IDisposable
{
    private readonly SqliteConnection _connection;

    private SqliteInMemoryContext(SqliteConnection connection, AppDbContext db)
    {
        _connection = connection;
        Db = db;
    }

    /// <summary>Contexto EF Core pronto para uso, com o schema já criado.</summary>
    public AppDbContext Db { get; }

    /// <summary>
    /// Cria um novo banco SQLite em memória com o schema aplicado e retorna o wrapper
    /// responsável por manter a conexão aberta e liberá-la ao final.
    /// </summary>
    public static SqliteInMemoryContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        return new SqliteInMemoryContext(connection, db);
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
