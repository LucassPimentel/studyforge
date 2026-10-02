namespace StudyForge.Domain;

/// <summary>
/// Implementação pura e determinística do algoritmo de repetição espaçada SM-2.
/// Não acessa banco de dados, rede nem relógio global: o instante atual ("now") chega
/// sempre por parâmetro, de modo que o cálculo seja reprodutível e testável isoladamente.
/// </summary>
public class SM2SchedulingService : ISpacedRepetitionService
{
    /// <summary>Fator de facilidade de um card recém-criado, conforme o SM-2.</summary>
    private const double InitialEaseFactor = 2.5;

    /// <summary>Intervalo (em dias) de um card recém-criado: devido imediatamente.</summary>
    private const int InitialInterval = 0;

    /// <summary>Número de acertos consecutivos de um card recém-criado.</summary>
    private const int InitialRepetitions = 0;

    /// <summary>Menor grade válida que o usuário pode informar em uma revisão.</summary>
    private const int MinGrade = 0;

    /// <summary>Maior grade válida que o usuário pode informar em uma revisão.</summary>
    private const int MaxGrade = 5;

    /// <summary>
    /// Piso do fator de facilidade: o easeFactor nunca pode cair abaixo de 1.3 (SM-2).
    /// </summary>
    private const double MinEaseFactor = 1.3;

    /// <summary>Menor grade considerada um acerto; abaixo disso o card reinicia.</summary>
    private const int PassingGrade = 3;

    /// <summary>Intervalo (em dias) após o primeiro acerto consecutivo.</summary>
    private const int FirstInterval = 1;

    /// <summary>Intervalo (em dias) após o segundo acerto consecutivo.</summary>
    private const int SecondInterval = 6;

    /// <inheritdoc />
    public SchedulingState CreateInitialState(DateTime now) =>
        new SchedulingState(InitialEaseFactor, InitialInterval, InitialRepetitions, now);

    /// <inheritdoc />
    public SchedulingState ApplyReview(SchedulingState current, int grade, DateTime now)
    {
        if (grade < MinGrade || grade > MaxGrade)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grade),
                grade,
                $"A grade deve estar entre {MinGrade} e {MaxGrade}.");
        }

        if (current.Interval < 0)
        {
            throw new ArgumentException(
                $"O estado atual possui um {nameof(current.Interval)} negativo ({current.Interval}); o intervalo não pode ser negativo.",
                nameof(current));
        }

        if (current.Repetitions < 0)
        {
            throw new ArgumentException(
                $"O estado atual possui um {nameof(current.Repetitions)} negativo ({current.Repetitions}); o número de repetições não pode ser negativo.",
                nameof(current));
        }

        // Atualiza o fator de facilidade segundo a fórmula SM-2 (requirement 2.1).
        // O cálculo roda sempre, tanto para acertos quanto para erros (requirement 2.3).
        double gradeDelta = MaxGrade - grade;
        double rawEaseFactor =
            current.EaseFactor + (0.1 - gradeDelta * (0.08 + gradeDelta * 0.02));

        // Fixa o piso em 1.3: o easeFactor nunca cai abaixo desse valor (requirement 2.2).
        double newEaseFactor = Math.Max(rawEaseFactor, MinEaseFactor);

        int newRepetitions;
        int newInterval;

        if (grade < PassingGrade)
        {
            // Erro: o card reinicia o ciclo (requirement 1.2).
            newRepetitions = 0;
            newInterval = FirstInterval;
        }
        else
        {
            // Acerto: incrementa o número de acertos consecutivos (requirement 1.3).
            newRepetitions = current.Repetitions + 1;

            // Define o intervalo conforme o novo repetitions (requirements 1.4, 1.5, 1.6).
            newInterval = newRepetitions switch
            {
                1 => FirstInterval,
                2 => SecondInterval,
                _ => (int)Math.Round(current.Interval * newEaseFactor),
            };
        }

        // Próxima revisão = instante atual + novo intervalo em dias (requirement 3.1).
        // O "now" chega por parâmetro, preservando o determinismo do serviço (requirement 3.2).
        DateTime nextReview = now.AddDays(newInterval);

        return new SchedulingState(newEaseFactor, newInterval, newRepetitions, nextReview);
    }
}
