using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Data;
using StudyForge.Api.Dtos;
using StudyForge.Api.Entities;
using StudyForge.Domain;

namespace StudyForge.Api.Services;

/// <summary>
/// Implementação das regras de geração de cards sobre o <see cref="AppDbContext"/>.
/// O estado de agendamento inicial é delegado ao <see cref="ISpacedRepetitionService"/>.
/// </summary>
public class CardService : ICardService
{
    /// <summary>Separador que divide pergunta e resposta em cada linha do texto.</summary>
    private const string Separator = "::";

    private readonly AppDbContext _db;
    private readonly ISpacedRepetitionService _spacedRepetition;

    public CardService(AppDbContext db, ISpacedRepetitionService spacedRepetition)
    {
        _db = db;
        _spacedRepetition = spacedRepetition;
    }

    public async Task<GenerateResultDto?> GenerateFromTextAsync(
        int deckId,
        GenerateCardsDto dto,
        CancellationToken cancellationToken = default)
    {
        // Guard: texto obrigatório.
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new ValidationException("O texto para geração de cards não pode ser vazio.");
        }

        // Guard: deck precisa existir (404 pelo controller quando null).
        var deckExists = await _db.Decks
            .AsNoTracking()
            .AnyAsync(d => d.Id == deckId, cancellationToken);
        if (!deckExists)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var cards = ParseCards(deckId, dto.Text, now);

        if (cards.Count == 0)
        {
            // Nenhuma linha válida: nada a persistir.
            return new GenerateResultDto(Created: 0);
        }

        _db.Cards.AddRange(cards);
        await _db.SaveChangesAsync(cancellationToken);

        return new GenerateResultDto(cards.Count);
    }

    /// <summary>
    /// Interpreta o texto em cards, uma linha por card no formato <c>pergunta :: resposta</c>.
    /// Linhas vazias ou sem o separador <c>::</c> são ignoradas; ambos os lados recebem trim.
    /// </summary>
    private List<Card> ParseCards(int deckId, string text, DateTime now)
    {
        var cards = new List<Card>();
        var lines = text.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var separatorIndex = line.IndexOf(Separator, StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                continue;
            }

            var question = line[..separatorIndex].Trim();
            var answer = line[(separatorIndex + Separator.Length)..].Trim();

            // Estado de agendamento inicial via SM-2 (card fica devido imediatamente).
            var state = _spacedRepetition.CreateInitialState(now);

            cards.Add(new Card
            {
                DeckId = deckId,
                Question = question,
                Answer = answer,
                EaseFactor = state.EaseFactor,
                Interval = state.Interval,
                Repetitions = state.Repetitions,
                NextReview = state.NextReview,
            });
        }

        return cards;
    }
}
