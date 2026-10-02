namespace StudyForge.Domain;

/// <summary>
/// Avaliação de lembrança informada pelo usuário ao revisar um card, no estilo Anki.
/// O valor numérico corresponde à grade SM-2 (0..5) usada no cálculo de agendamento.
/// </summary>
public enum ReviewGrade
{
    /// <summary>Errei: o usuário não lembrou do card.</summary>
    Again = 0,

    /// <summary>Difícil: lembrou com muito esforço.</summary>
    Hard = 3,

    /// <summary>Bom: lembrou com algum esforço.</summary>
    Good = 4,

    /// <summary>Fácil: lembrou sem esforço.</summary>
    Easy = 5
}
