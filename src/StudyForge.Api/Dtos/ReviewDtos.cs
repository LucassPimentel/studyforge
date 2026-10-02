namespace StudyForge.Api.Dtos;

/// <summary>Card exposto em uma sessão de revisão (sem campos de agendamento internos).</summary>
public record CardDto(int Id, string Question, string Answer);

/// <summary>Entrada de avaliação de um card: grade de lembrança (0..5).</summary>
public record GradeDto(int Grade);

/// <summary>
/// Novo estado de agendamento retornado após avaliar um card. Expõe apenas o contrato
/// necessário ao frontend (sem vazar a entidade EF).
/// </summary>
public record ReviewResultDto(
    int CardId,
    double EaseFactor,
    int Interval,
    int Repetitions,
    DateTime NextReview);
