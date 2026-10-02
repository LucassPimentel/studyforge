using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Dtos;
using StudyForge.Api.Entities;
using StudyForge.Api.Services;

namespace StudyForge.Api.Tests;

/// <summary>
/// Testes de unidade para <see cref="DeckService"/>: criação com validação/trim do nome,
/// listagem com contagem de cards, exclusão em cascata e tratamento de deck inexistente,
/// e os buckets de progresso (pending / learning / mastered / due). Usa SQLite in-memory
/// para preservar o comportamento relacional real (ex.: exclusão em cascata).
/// </summary>
public class DeckServiceTests
{
    private static DeckService CreateService(SqliteInMemoryContext context) =>
        new DeckService(context.Db);

    /// <summary>Insere um deck e devolve seu id.</summary>
    private static async Task<int> SeedDeckAsync(SqliteInMemoryContext context, string name = "Deck")
    {
        var deck = new Deck { Name = name };
        context.Db.Decks.Add(deck);
        await context.Db.SaveChangesAsync();
        return deck.Id;
    }

    /// <summary>Insere um card com o agendamento informado.</summary>
    private static async Task SeedCardAsync(
        SqliteInMemoryContext context,
        int deckId,
        int repetitions,
        DateTime nextReview)
    {
        context.Db.Cards.Add(new Card
        {
            DeckId = deckId,
            Question = "P",
            Answer = "R",
            Repetitions = repetitions,
            NextReview = nextReview,
        });
        await context.Db.SaveChangesAsync();
    }

    // Requirement 1.1 — criar deck com nome não vazio persiste e retorna id + nome.
    [Fact]
    public async Task CreateAsync_NomeValido_PersisteERetornaResumo()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var result = await service.CreateAsync(new CreateDeckDto("Biologia"));

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("Biologia", result.Name);
        Assert.Equal(0, result.CardCount);
        Assert.True(await context.Db.Decks.AnyAsync(d => d.Id == result.Id));
    }

    // Requirement 1.1 — o nome recebe trim antes de ser persistido.
    [Fact]
    public async Task CreateAsync_NomeComEspacos_FazTrim()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var result = await service.CreateAsync(new CreateDeckDto("  História  "));

        // Assert
        Assert.Equal("História", result.Name);
    }

    // Requirement 1.2 — nome vazio, só espaços ou null lança ValidationException (400).
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task CreateAsync_NomeVazio_LancaValidationException(string name)
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(new CreateDeckDto(name)));
    }

    // Requirement 1.3 — listar retorna cada deck com nome e contagem de cards.
    [Fact]
    public async Task ListAsync_RetornaDecksComContagemDeCards()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);
        var comCards = await SeedDeckAsync(context, "Com Cards");
        var vazio = await SeedDeckAsync(context, "Vazio");
        await SeedCardAsync(context, comCards, repetitions: 0, nextReview: DateTime.UtcNow);
        await SeedCardAsync(context, comCards, repetitions: 1, nextReview: DateTime.UtcNow);

        // Act
        var decks = await service.ListAsync();

        // Assert
        Assert.Equal(2, decks.Count);
        Assert.Equal(2, decks.Single(d => d.Id == comCards).CardCount);
        Assert.Equal(0, decks.Single(d => d.Id == vazio).CardCount);
    }

    // Requirement 1.4, 5.2 — excluir um deck remove o deck e seus cards em cascata.
    [Fact]
    public async Task DeleteAsync_DeckExistente_RemoveDeckECardsEmCascata()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);
        var deckId = await SeedDeckAsync(context);
        await SeedCardAsync(context, deckId, repetitions: 0, nextReview: DateTime.UtcNow);
        await SeedCardAsync(context, deckId, repetitions: 2, nextReview: DateTime.UtcNow);

        // Act
        var removed = await service.DeleteAsync(deckId);

        // Assert
        Assert.True(removed);
        context.Db.ChangeTracker.Clear();
        Assert.False(await context.Db.Decks.AnyAsync(d => d.Id == deckId));
        Assert.Equal(0, await context.Db.Cards.CountAsync(c => c.DeckId == deckId));
    }

    // Requirement 6.2 — excluir deck inexistente retorna false (404 pelo controller).
    [Fact]
    public async Task DeleteAsync_DeckInexistente_RetornaFalse()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var removed = await service.DeleteAsync(404);

        // Assert
        Assert.False(removed);
    }

    // Requirement 4.1, 4.2 — buckets: pending=rep0, learning=1..2, mastered>=3, due=nextReview<=agora.
    [Fact]
    public async Task GetProgressAsync_ClassificaCardsNosBucketsCorretos()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);
        var deckId = await SeedDeckAsync(context);
        var past = DateTime.UtcNow.AddDays(-1);
        var future = DateTime.UtcNow.AddDays(5);

        // pending (rep 0) + devido
        await SeedCardAsync(context, deckId, repetitions: 0, nextReview: past);
        // learning (rep 1) não devido
        await SeedCardAsync(context, deckId, repetitions: 1, nextReview: future);
        // learning (rep 2) devido
        await SeedCardAsync(context, deckId, repetitions: 2, nextReview: past);
        // mastered (rep 3) não devido
        await SeedCardAsync(context, deckId, repetitions: 3, nextReview: future);
        // mastered (rep 5) devido
        await SeedCardAsync(context, deckId, repetitions: 5, nextReview: past);

        // Act
        var progress = await service.GetProgressAsync(deckId);

        // Assert
        Assert.NotNull(progress);
        Assert.Equal(1, progress!.Pending);   // 1 card com rep 0
        Assert.Equal(2, progress.Learning);   // reps 1 e 2
        Assert.Equal(2, progress.Mastered);   // reps 3 e 5
        Assert.Equal(3, progress.Due);        // três com nextReview no passado
    }

    // Requirement 4.1, 4.2 — deck sem cards retorna todos os buckets zerados.
    [Fact]
    public async Task GetProgressAsync_DeckSemCards_RetornaZeros()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);
        var deckId = await SeedDeckAsync(context);

        // Act
        var progress = await service.GetProgressAsync(deckId);

        // Assert
        Assert.NotNull(progress);
        Assert.Equal(0, progress!.Pending);
        Assert.Equal(0, progress.Learning);
        Assert.Equal(0, progress.Mastered);
        Assert.Equal(0, progress.Due);
    }

    // Requirement 6.2 — progresso de deck inexistente retorna null (404 pelo controller).
    [Fact]
    public async Task GetProgressAsync_DeckInexistente_RetornaNull()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var progress = await service.GetProgressAsync(777);

        // Assert
        Assert.Null(progress);
    }
}
