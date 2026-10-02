using StudyForge.Api.Dtos;

namespace StudyForge.Api.Services;

/// <summary>
/// Regras de negócio para gerenciamento de decks: criar, listar e excluir.
/// </summary>
public interface IDeckService
{
    /// <summary>
    /// Cria um deck com o nome informado (após trim) e retorna seu resumo.
    /// </summary>
    /// <exception cref="ValidationException">Se o nome for vazio ou só espaços.</exception>
    Task<DeckSummaryDto> CreateAsync(CreateDeckDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todos os decks com a contagem de cards de cada um.
    /// </summary>
    Task<IReadOnlyList<DeckSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui o deck e, em cascata, todos os seus cards.
    /// </summary>
    /// <returns><c>true</c> se o deck existia e foi excluído; <c>false</c> se não existir.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula o progresso do deck em contagens por estágio: pendentes
    /// (repetitions == 0), aprendendo (1..2), dominados (>= 3) e devidos agora
    /// (nextReview &lt;= agora).
    /// </summary>
    /// <returns>O progresso do deck, ou <c>null</c> se o deck não existir.</returns>
    Task<ProgressDto?> GetProgressAsync(int id, CancellationToken cancellationToken = default);
}
