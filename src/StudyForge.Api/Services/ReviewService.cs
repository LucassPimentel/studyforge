using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Data;
using StudyForge.Api.Dtos;
using StudyForge.Domain;

namespace StudyForge.Api.Services;

/// <summary>
/// Implementação da sessão de revisão sobre o <see cref="AppDbContext"/>. O cálculo do
/// novo agendamento é delegado ao <see cref="ISpacedRepetitionService"/> (SM-2).
/// </summary>
public class ReviewService : IReviewService
{
    /// <summary>Menor grade aceita (Errei).</summary>
    private const int MinGrade = 0;

    /// <summary>Maior grade aceita (Fácil).</summary>
    private const int MaxGrade = 5;

    private readonly AppDbContext _db;
    private readonly ISpacedRepetitionService _spacedRepetition;

    public ReviewService(AppDbContext db, ISpacedRepetitionService spacedRepetition)
    {
        _db = db;
        _spacedRepetition = spacedRepetition;
    }

    public async Task<IReadOnlyList<CardDto>?> GetDueCardsAsync(
        int deckId,
        CancellationToken cancellationToken = default)
    {
        // Guard: deck precisa existir (404 pelo controller quando null).
        var deckExists = await _db.Decks
            .AsNoTracking()
            .AnyAsync(d => d.Id == deckId, cancellationToken);
        if (!deckExists)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        // Somente leitura: AsNoTracking + projeção para DTO (não vaza a entidade EF).
        return await _db.Cards
            .AsNoTracking()
            .Where(c => c.DeckId == deckId && c.NextReview <= now)
            .Select(c => new CardDto(c.Id, c.Question, c.Answer))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReviewResultDto?> GradeCardAsync(
        int cardId,
        GradeDto dto,
        CancellationToken cancellationToken = default)
    {
        // Guard: grade dentro do intervalo válido (400 pelo controller).
        if (dto.Grade < MinGrade || dto.Grade > MaxGrade)
        {
            throw new ValidationException($"A grade deve estar entre {MinGrade} e {MaxGrade}.");
        }

        // O card é rastreado (sem AsNoTracking) para que as alterações sejam persistidas.
        var card = await _db.Cards
            .FirstOrDefaultAsync(c => c.Id == cardId, cancellationToken);
        if (card is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        // Monta o estado atual, aplica o SM-2 e grava o novo agendamento no card.
        var currentState = new SchedulingState(
            card.EaseFactor,
            card.Interval,
            card.Repetitions,
            card.NextReview);

        var newState = _spacedRepetition.ApplyReview(currentState, dto.Grade, now);

        card.EaseFactor = newState.EaseFactor;
        card.Interval = newState.Interval;
        card.Repetitions = newState.Repetitions;
        card.NextReview = newState.NextReview;

        await _db.SaveChangesAsync(cancellationToken);

        return new ReviewResultDto(
            card.Id,
            newState.EaseFactor,
            newState.Interval,
            newState.Repetitions,
            newState.NextReview);
    }
}
