namespace StudyForge.Domain;

/// <summary>
/// Serviço puro e determinístico que encapsula o algoritmo de repetição espaçada SM-2.
/// O instante atual ("now") é sempre recebido por parâmetro para evitar dependência de
/// relógio global, mantendo o cálculo testável e reprodutível.
/// </summary>
public interface ISpacedRepetitionService
{
    /// <summary>
    /// Cria o estado de agendamento de um card recém-criado, que fica devido imediatamente.
    /// </summary>
    /// <param name="now">Instante atual (UTC) considerado como criação do card.</param>
    /// <returns>O estado inicial do card.</returns>
    SchedulingState CreateInitialState(DateTime now);

    /// <summary>
    /// Aplica uma avaliação de revisão ao estado atual e retorna o novo estado de agendamento.
    /// </summary>
    /// <param name="current">Estado atual de agendamento do card.</param>
    /// <param name="grade">Avaliação de lembrança do usuário (0..5).</param>
    /// <param name="now">Instante atual (UTC) em que a revisão ocorre.</param>
    /// <returns>O novo estado de agendamento após a revisão.</returns>
    SchedulingState ApplyReview(SchedulingState current, int grade, DateTime now);
}
