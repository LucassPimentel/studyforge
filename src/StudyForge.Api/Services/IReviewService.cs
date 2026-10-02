using StudyForge.Api.Dtos;

namespace StudyForge.Api.Services;

/// <summary>
/// Regras de negócio da sessão de revisão: obter os cards devidos de um deck e aplicar
/// a avaliação (SM-2) a um card, persistindo o novo agendamento.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Retorna os cards devidos agora (<c>nextReview &lt;= now</c>) de um deck. Retorna uma
    /// lista vazia quando não houver cards devidos.
    /// </summary>
    /// <param name="deckId">Id do deck a revisar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Os cards devidos; <c>null</c> se o deck não existir.</returns>
    Task<IReadOnlyList<CardDto>?> GetDueCardsAsync(
        int deckId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica uma avaliação (grade 0..5) a um card via SM-2 e persiste o novo estado de
    /// agendamento.
    /// </summary>
    /// <param name="cardId">Id do card avaliado.</param>
    /// <param name="dto">Grade de lembrança do usuário (0..5).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O novo agendamento; <c>null</c> se o card não existir.</returns>
    /// <exception cref="ValidationException">Se a grade estiver fora do intervalo 0..5.</exception>
    Task<ReviewResultDto?> GradeCardAsync(
        int cardId,
        GradeDto dto,
        CancellationToken cancellationToken = default);
}
