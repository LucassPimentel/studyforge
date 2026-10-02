using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Dtos;
using StudyForge.Api.Entities;
using StudyForge.Api.Services;
using StudyForge.Domain;

namespace StudyForge.Api.Tests;

/// <summary>
/// Testes de unidade para <see cref="CardService.GenerateFromTextAsync"/>: parsing do
/// formato <c>pergunta :: resposta</c>, trim, descarte de linhas inválidas/vazias,
/// contagem de cards criados, estado inicial de agendamento, validação de texto vazio
/// e tratamento de deck inexistente. Usa SQLite in-memory e o SM-2 real (serviço puro).
/// </summary>
public class CardServiceTests
{
    private readonly SM2SchedulingService _spacedRepetition = new();

    /// <summary>Cria o service sobre o contexto informado, com o SM-2 real.</summary>
    private CardService CreateService(SqliteInMemoryContext context) =>
        new CardService(context.Db, _spacedRepetition);

    /// <summary>Insere um deck e devolve seu id.</summary>
    private static async Task<int> SeedDeckAsync(SqliteInMemoryContext context, string name = "Deck")
    {
        var deck = new Deck { Name = name };
        context.Db.Decks.Add(deck);
        await context.Db.SaveChangesAsync();
        return deck.Id;
    }

    // Requirement 2.1, 2.5 — linhas válidas viram cards e a contagem é retornada.
    [Fact]
    public async Task GenerateFromTextAsync_LinhasValidas_CriaCardsERetornaContagem()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var text = "Capital do Brasil :: Brasília\nMaior planeta :: Júpiter";

        // Act
        var result = await service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Created);
        Assert.Equal(2, await context.Db.Cards.CountAsync(c => c.DeckId == deckId));
    }

    // Requirement 2.6 — pergunta e resposta recebem trim nos dois lados.
    [Fact]
    public async Task GenerateFromTextAsync_ComEspacos_FazTrimDePerguntaEResposta()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var text = "   Pergunta com espaços   ::   Resposta com espaços   ";

        // Act
        var result = await service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text));

        // Assert
        Assert.Equal(1, result!.Created);
        var card = await context.Db.Cards.SingleAsync(c => c.DeckId == deckId);
        Assert.Equal("Pergunta com espaços", card.Question);
        Assert.Equal("Resposta com espaços", card.Answer);
    }

    // Requirement 2.2 — linhas sem o separador "::" são ignoradas.
    [Fact]
    public async Task GenerateFromTextAsync_LinhaSemSeparador_EhIgnorada()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var text = "Linha válida :: ok\nlinha sem separador\nOutra válida :: também";

        // Act
        var result = await service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text));

        // Assert
        Assert.Equal(2, result!.Created);
    }

    // Requirement 2.3 — linhas vazias ou só com espaços são ignoradas.
    [Fact]
    public async Task GenerateFromTextAsync_LinhasVaziasOuEmBranco_SaoIgnoradas()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var text = "Única :: válida\n\n   \n\t\n";

        // Act
        var result = await service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text));

        // Assert
        Assert.Equal(1, result!.Created);
    }

    // Requirement 2.1 — apenas o primeiro "::" separa; os seguintes ficam na resposta.
    [Fact]
    public async Task GenerateFromTextAsync_MultiplosSeparadores_UsaOPrimeiro()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var text = "Operador :: a :: b";

        // Act
        var result = await service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text));

        // Assert
        Assert.Equal(1, result!.Created);
        var card = await context.Db.Cards.SingleAsync(c => c.DeckId == deckId);
        Assert.Equal("Operador", card.Question);
        Assert.Equal("a :: b", card.Answer);
    }

    // Requirement 2.2, 2.3 — texto sem nenhuma linha válida cria zero cards.
    [Fact]
    public async Task GenerateFromTextAsync_SemLinhasValidas_CriaZeroCards()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var text = "linha um\nlinha dois\n   ";

        // Act
        var result = await service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text));

        // Assert
        Assert.Equal(0, result!.Created);
        Assert.Equal(0, await context.Db.Cards.CountAsync(c => c.DeckId == deckId));
    }

    // Requirement 2.4 — cada card inicia com o estado de agendamento inicial do SM-2.
    [Fact]
    public async Task GenerateFromTextAsync_DefineEstadoInicialDeAgendamento()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);
        var before = DateTime.UtcNow;

        // Act
        await service.GenerateFromTextAsync(deckId, new GenerateCardsDto("P :: R"));

        // Assert
        var after = DateTime.UtcNow;
        var card = await context.Db.Cards.SingleAsync(c => c.DeckId == deckId);
        Assert.Equal(2.5, card.EaseFactor);
        Assert.Equal(0, card.Interval);
        Assert.Equal(0, card.Repetitions);
        // Card novo fica devido imediatamente (nextReview ~ agora).
        Assert.InRange(card.NextReview, before, after);
    }

    // Requirement 2 (guarda) — texto vazio ou só espaços lança ValidationException.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t  ")]
    public async Task GenerateFromTextAsync_TextoVazio_LancaValidationException(string text)
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var deckId = await SeedDeckAsync(context);
        var service = CreateService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => service.GenerateFromTextAsync(deckId, new GenerateCardsDto(text)));
    }

    // Requirement 6.2 — deck inexistente retorna null (404 pelo controller).
    [Fact]
    public async Task GenerateFromTextAsync_DeckInexistente_RetornaNull()
    {
        // Arrange
        using var context = SqliteInMemoryContext.Create();
        var service = CreateService(context);

        // Act
        var result = await service.GenerateFromTextAsync(999, new GenerateCardsDto("P :: R"));

        // Assert
        Assert.Null(result);
    }
}
