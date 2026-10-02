using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Data;
using StudyForge.Api.Dtos;
using StudyForge.Api.Entities;

namespace StudyForge.Api.Services;

/// <summary>
/// Implementação das regras de negócio de decks sobre o <see cref="AppDbContext"/>.
/// </summary>
public class DeckService : IDeckService
{
    private readonly AppDbContext _db;

    public DeckService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DeckSummaryDto> CreateAsync(CreateDeckDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new ValidationException("O nome do deck não pode ser vazio.");
        }

        var deck = new Deck { Name = name };
        _db.Decks.Add(deck);
        await _db.SaveChangesAsync(cancellationToken);

        // Deck recém-criado não possui cards.
        return new DeckSummaryDto(deck.Id, deck.Name, CardCount: 0);
    }

    public async Task<IReadOnlyList<DeckSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        // Projeção direta no banco para evitar carregar as entidades Card (N+1).
        return await _db.Decks
            .AsNoTracking()
            .Select(d => new DeckSummaryDto(d.Id, d.Name, d.Cards.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deck = await _db.Decks.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (deck is null)
        {
            return false;
        }

        // Os cards são removidos em cascata pela configuração do relacionamento.
        _db.Decks.Remove(deck);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProgressDto?> GetProgressAsync(int id, CancellationToken cancellationToken = default)
    {
        // Guard: deck precisa existir (404 pelo controller quando null).
        var deckExists = await _db.Decks
            .AsNoTracking()
            .AnyAsync(d => d.Id == id, cancellationToken);
        if (!deckExists)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        // Somente leitura: as contagens são computadas no banco (sem carregar as entidades Card).
        var progress = await _db.Cards
            .AsNoTracking()
            .Where(c => c.DeckId == id)
            .GroupBy(_ => 1)
            .Select(g => new ProgressDto(
                g.Count(c => c.Repetitions == 0),
                g.Count(c => c.Repetitions >= 1 && c.Repetitions <= 2),
                g.Count(c => c.Repetitions >= 3),
                g.Count(c => c.NextReview <= now)))
            .FirstOrDefaultAsync(cancellationToken);

        // Deck sem cards: o GroupBy não produz linhas, então retornamos zeros.
        return progress ?? new ProgressDto(Pending: 0, Learning: 0, Mastered: 0, Due: 0);
    }
}
