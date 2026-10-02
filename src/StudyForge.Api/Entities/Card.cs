namespace StudyForge.Api.Entities;

/// <summary>
/// Par pergunta/resposta com o estado de agendamento SM-2 embutido.
/// Os campos de agendamento refletem o <c>SchedulingState</c> do domínio.
/// </summary>
public class Card
{
    public int Id { get; set; }

    public int DeckId { get; set; }

    public string Question { get; set; } = "";

    public string Answer { get; set; } = "";

    // Estado de agendamento (SM-2). Valores padrão representam o estado inicial de um card.
    public double EaseFactor { get; set; } = 2.5;

    public int Interval { get; set; } = 0;

    public int Repetitions { get; set; } = 0;

    /// <summary>Data/hora (UTC) em que o card deve ser revisado novamente.</summary>
    public DateTime NextReview { get; set; }

    /// <summary>Deck ao qual o card pertence (propriedade de navegação).</summary>
    public Deck? Deck { get; set; }
}
