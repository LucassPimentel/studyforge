namespace StudyForge.Domain;

/// <summary>
/// Estado de agendamento (SM-2) de um card em um dado momento. Imutável e puro:
/// não conhece banco de dados, HTTP nem UI.
/// </summary>
/// <param name="EaseFactor">Fator de facilidade do card. Começa em 2.5 e nunca fica abaixo de 1.3.</param>
/// <param name="Interval">Número de dias até a próxima revisão.</param>
/// <param name="Repetitions">Número de acertos consecutivos (grade &gt;= 3).</param>
/// <param name="NextReview">Data/hora (UTC) em que o card deve ser revisado novamente.</param>
public record SchedulingState(double EaseFactor, int Interval, int Repetitions, DateTime NextReview);
