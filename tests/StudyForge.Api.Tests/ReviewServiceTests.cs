using StudyForge.Api.Dtos;
using StudyForge.Api.Entities;
using StudyForge.Api.Services;
using StudyForge.Domain;

namespace StudyForge.Api.Tests;

/// <summary>
/// Testes de unidade para <see cref="ReviewService"/>: seleção de cards devidos
/// (<c>nextReview &lt;= agora</c>), sessão vazia, deck/card inexistente, validação da grade
/// (0..5) e delegação ao SM-2 com persistência do novo agendamento. Usa SQLite in-memory
/// e o SM-2 real (serviço puro e determinístico).
/// </summary>
public class ReviewServiceTests
{
    private readonly SM2SchedulingService _spacedRepetition = new();

    private ReviewService CreateService(SqliteInMemoryContext context) =>
        new ReviewService(context.Db, _spacedRepetition);

    /// <summary>Insere um deck e devolve seu id.</summary>
    private static async Task<int> SeedDeckAsync(SqliteInMemoryContext context, string name = "Deck")
    {
        var deck = new Deck { Name = name };
        context.Db.Decks.Add(deck);
        await context.Db.SaveChangesAsync();
        return deck.Id;
    }

    /// <summary>Insere um card com o agendamento informado e devolve seu id.</summary>
    private static async Task<int> SeedCardAsync(
        SqliteInMemoryContext context,
        int deckId,
        DateTime nextReview,
        double easeFactor = 2.5,
        int interval = 0,
        int repetitions = 0)
    {
        var card = new Card
        {
            DeckId = deckId,
            Question = "P",
            Answer = "R",
            EaseFactor = easeFactor,
            Interval = interval,
            Repetitions = repetitions,
            NextReview = nextReview,
        };
        context.Db.Cards.Add(card);
        await context.Db.SaveChangesAsync();
        return card.Id;
    }

    // Requirement 3.1 — retorna apenas os cards com nextReview <= agora.
    [Fact]
    public async Task GetDueCardsAsync_RetornaApenasCardsDevidos()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var dueId = await SeedCardAsync(context, deckId, DateTime.UtcNow.AddDays(-1)); // devido
        await SeedCardAsync(context, deckId, DateTime.UtcNow.AddDays(5));              // futuro

        // Act
        var due = await service.GetDueCardsAsync(deckId);

        // Assert
        Assert.NotNull(due);
        var card = Assert.Single(due!);
        Assert.Equal(dueId, card.Id);
    }

    // Requirement 3.4 — sem cards devidos, a sessão retorna lista vazia (não null).
    [Fact]
    public async Task GetDueCardsAsync_SemCardsDevidos_RetornaListaVazia()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        await SeedCardAsync(context, deckId, DateTime.UtcNow.AddDays(3));

        // Act
        var due = await service.GetDueCardsAsync(deckId);

        // Assert
        Assert.NotNull(due);
        Assert.Empty(due!);
    }

    // Requirement 6.2 — deck inexistente retorna null (404 pelo controller).
    [Fact]
    public async Task GetDueCardsAsync_DeckInexistente_RetornaNull()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var due = await service.GetDueCardsAsync(123);

        // Assert
        Assert.Null(due);
    }

    // Requirement 3.1 — não vaza cards de outros decks.
    [Fact]
    public async Task GetDueCardsAsync_IgnoraCardsDeOutrosDecks()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckA = await SeedDeckAsync(context, "A");
        var deckB = await SeedDeckAsync(context, "B");
        var service = CreateService(context);
        var devidoA = await SeedCardAsync(context, deckA, DateTime.UtcNow.AddDays(-1));
        await SeedCardAsync(context, deckB, DateTime.UtcNow.AddDays(-1));

        // Act
        var due = await service.GetDueCardsAsync(deckA);

        // Assert
        var card = Assert.Single(due!);
        Assert.Equal(devidoA, card.Id);
    }

    // Requirement 3.2 — avaliar um acerto delega ao SM-2 e persiste o novo agendamento.
    [Fact]
    public async Task GradeCardAsync_PrimeiroAcerto_AplicaSM2EPersiste()
    {
        // Arrange: card novo (repetitions 0, interval 0).
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var cardId = await SeedCardAsync(context, deckId, DateTime.UtcNow);
        var before = DateTime.UtcNow;

        // Act
        var result = await service.GradeCardAsync(cardId, new GradeDto((int)ReviewGrade.Good));

        // Assert: primeiro acerto define repetitions=1 e interval=1 (SM-2).
        Assert.NotNull(result);
        Assert.Equal(cardId, result!.CardId);
        Assert.Equal(1, result.Repetitions);
        Assert.Equal(1, result.Interval);

        // Persistência: o card no banco reflete o novo agendamento.
        context.Db.ChangeTracker.Clear();
        var persisted = await context.Db.Cards.FindAsync(cardId);
        Assert.NotNull(persisted);
        Assert.Equal(1, persisted!.Repetitions);
        Assert.Equal(1, persisted.Interval);
        Assert.Equal(before.AddDays(1).Date, persisted.NextReview.Date);
    }

    // Requirement 3.2 — avaliar um erro reinicia o ciclo do card e persiste.
    [Fact]
    public async Task GradeCardAsync_Erro_ReiniciaCicloEPersiste()
    {
        // Arrange: card com progresso acumulado.
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var cardId = await SeedCardAsync(
            context, deckId, DateTime.UtcNow.AddDays(-1),
            easeFactor: 2.5, interval: 15, repetitions: 4);

        // Act
        var result = await service.GradeCardAsync(cardId, new GradeDto((int)ReviewGrade.Again));

        // Assert: erro reinicia repetitions=0 e interval=1 (SM-2).
        Assert.NotNull(result);
        Assert.Equal(0, result!.Repetitions);
        Assert.Equal(1, result.Interval);

        context.Db.ChangeTracker.Clear();
        var persisted = await context.Db.Cards.FindAsync(cardId);
        Assert.Equal(0, persisted!.Repetitions);
        Assert.Equal(1, persisted.Interval);
    }

    // Requirement 3.3 — grade fora de 0..5 lança ValidationException (400) e não persiste.
    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public async Task GradeCardAsync_GradeInvalida_LancaValidationException(int grade)
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var cardId = await SeedCardAsync(context, deckId, DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => service.GradeCardAsync(cardId, new GradeDto(grade)));

        // O card permanece intacto (estado inicial).
        context.Db.ChangeTracker.Clear();
        var persisted = await context.Db.Cards.FindAsync(cardId);
        Assert.Equal(0, persisted!.Repetitions);
    }

    // Requirement 3.3 — toda grade válida de 0 a 5 é aceita (não lança).
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task GradeCardAsync_GradeValida_NaoLanca(int grade)
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var cardId = await SeedCardAsync(context, deckId, DateTime.UtcNow);

        // Act
        var result = await service.GradeCardAsync(cardId, new GradeDto(grade));

        // Assert
        Assert.NotNull(result);
    }

    // Requirement 6.2 — card inexistente retorna null (404 pelo controller).
    [Fact]
    public async Task GradeCardAsync_CardInexistente_RetornaNull()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var result = await service.GradeCardAsync(555, new GradeDto((int)ReviewGrade.Good));

        // Assert
        Assert.Null(result);
    }
}
