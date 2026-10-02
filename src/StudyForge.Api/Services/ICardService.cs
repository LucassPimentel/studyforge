using StudyForge.Api.Dtos;

namespace StudyForge.Api.Services;

/// <summary>
/// Regras de negócio para geração de cards a partir de texto.
/// </summary>
public interface ICardService
{
    /// <summary>
    /// Gera cards para um deck interpretando cada linha do texto no formato
    /// <c>pergunta :: resposta</c>. Linhas vazias ou sem o separador <c>::</c> são ignoradas;
    /// pergunta e resposta recebem trim. Cada card criado inicia com o estado de agendamento
    /// inicial do SM-2.
    /// </summary>
    /// <param name="deckId">Id do deck que receberá os cards.</param>
    /// <param name="dto">Bloco de texto a ser interpretado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A quantidade de cards criados; <c>null</c> se o deck não existir.</returns>
    /// <exception cref="ValidationException">Se o texto for vazio ou só espaços.</exception>
    Task<GenerateResultDto?> GenerateFromTextAsync(
        int deckId,
        GenerateCardsDto dto,
        CancellationToken cancellationToken = default);
}
