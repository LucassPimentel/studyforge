using StudyForge.Domain;

namespace StudyForge.Domain.Tests;

/// <summary>
/// Testes de unidade para <see cref="SM2SchedulingService"/>, cobrindo o algoritmo SM-2:
/// estado inicial, progressão de intervalos em acertos, reinício em erro, piso do easeFactor,
/// cálculo da próxima revisão, validação de entrada e determinismo.
/// </summary>
public class SM2SchedulingServiceTests
{
    private static readonly DateTime Now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const double EaseFactorPrecision = 10;

    private readonly SM2SchedulingService _service = new();

    /// <summary>
    /// Calcula o easeFactor esperado pela fórmula SM-2, aplicando o piso de 1.3,
    /// de modo a derivar os valores esperados sem literais mágicos.
    /// </summary>
    private static double ExpectedEaseFactor(double currentEaseFactor, int grade)
    {
        double gradeDelta = 5 - grade;
        double raw = currentEaseFactor + (0.1 - gradeDelta * (0.08 + gradeDelta * 0.02));
        return Math.Max(raw, 1.3);
    }

    // Requirement 4.1, 4.2 — estado inicial de um card novo.
    [Fact]
    public void CreateInitialState_RetornaEstadoPadraoDeCardNovo()
    {
        // Act
        SchedulingState state = _service.CreateInitialState(Now);

        // Assert
        Assert.Equal(2.5, state.EaseFactor);
        Assert.Equal(0, state.Interval);
        Assert.Equal(0, state.Repetitions);
        Assert.Equal(Now, state.NextReview);
    }

    // Requirement 1.3, 1.4 — primeiro acerto define repetitions=1 e interval=1.
    [Fact]
    public void ApplyReview_PrimeiroAcerto_DefineIntervalUmERepetitionUm()
    {
        // Arrange
        SchedulingState initial = _service.CreateInitialState(Now);

        // Act
        SchedulingState result = _service.ApplyReview(initial, (int)ReviewGrade.Good, Now);

        // Assert
        Assert.Equal(1, result.Repetitions);
        Assert.Equal(1, result.Interval);
    }

    // Requirement 1.5 — segundo acerto consecutivo define interval=6.
    [Fact]
    public void ApplyReview_SegundoAcerto_DefineIntervalSeis()
    {
        // Arrange: estado após o primeiro acerto (repetitions=1, interval=1).
        SchedulingState afterFirst = new SchedulingState(2.5, 1, 1, Now);

        // Act
        SchedulingState result = _service.ApplyReview(afterFirst, (int)ReviewGrade.Good, Now);

        // Assert
        Assert.Equal(2, result.Repetitions);
        Assert.Equal(6, result.Interval);
    }

    // Requirement 1.6 — terceiro acerto define interval = round(interval_anterior * ef').
    [Fact]
    public void ApplyReview_TerceiroAcerto_DefineIntervalArredondado()
    {
        // Arrange: estado após o segundo acerto (repetitions=2, interval=6).
        const double currentEaseFactor = 2.5;
        const int currentInterval = 6;
        SchedulingState afterSecond = new SchedulingState(currentEaseFactor, currentInterval, 2, Now);

        double expectedEaseFactor = ExpectedEaseFactor(currentEaseFactor, (int)ReviewGrade.Good);
        int expectedInterval = (int)Math.Round(currentInterval * expectedEaseFactor);

        // Act
        SchedulingState result = _service.ApplyReview(afterSecond, (int)ReviewGrade.Good, Now);

        // Assert
        Assert.Equal(3, result.Repetitions);
        Assert.Equal(expectedInterval, result.Interval);
        Assert.Equal(expectedEaseFactor, result.EaseFactor, EaseFactorPrecision);
    }

    // Requirement 1.2 — qualquer erro (grade < 3) reinicia repetitions=0 e interval=1.
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ApplyReview_Erro_ReiniciaRepetitionsEInterval(int grade)
    {
        // Arrange: card com progresso acumulado.
        SchedulingState progressed = new SchedulingState(2.5, 15, 4, Now);

        // Act
        SchedulingState result = _service.ApplyReview(progressed, grade, Now);

        // Assert
        Assert.Equal(0, result.Repetitions);
        Assert.Equal(1, result.Interval);
    }

    // Requirement 2.2 — easeFactor nunca cai abaixo de 1.3, mesmo com erros repetidos.
    [Fact]
    public void ApplyReview_ErrosRepetidos_EaseFactorNuncaAbaixoDoPiso()
    {
        // Arrange
        SchedulingState state = _service.CreateInitialState(Now);

        // Act & Assert: aplica dez erros seguidos; o piso deve sempre ser respeitado.
        for (int i = 0; i < 10; i++)
        {
            state = _service.ApplyReview(state, (int)ReviewGrade.Again, Now);
            Assert.True(state.EaseFactor >= 1.3);
        }
    }

    // Requirement 2.3 — easeFactor é atualizado tanto em acerto quanto em erro.
    [Fact]
    public void ApplyReview_Acerto_AtualizaEaseFactor()
    {
        // Arrange
        SchedulingState initial = _service.CreateInitialState(Now);
        double expected = ExpectedEaseFactor(initial.EaseFactor, (int)ReviewGrade.Easy);

        // Act
        SchedulingState result = _service.ApplyReview(initial, (int)ReviewGrade.Easy, Now);

        // Assert
        Assert.Equal(expected, result.EaseFactor, EaseFactorPrecision);
        Assert.NotEqual(initial.EaseFactor, result.EaseFactor);
    }

    // Requirement 2.3 — em erro o easeFactor também é recalculado (fixado no piso neste caso).
    [Fact]
    public void ApplyReview_Erro_AtualizaEaseFactorFixandoNoPiso()
    {
        // Arrange: estado próximo do piso para que o erro empurre o ef para 1.3.
        SchedulingState state = new SchedulingState(1.4, 10, 3, Now);
        double expected = ExpectedEaseFactor(state.EaseFactor, (int)ReviewGrade.Again);

        // Act
        SchedulingState result = _service.ApplyReview(state, (int)ReviewGrade.Again, Now);

        // Assert
        Assert.Equal(expected, result.EaseFactor, EaseFactorPrecision);
        Assert.Equal(1.3, result.EaseFactor, EaseFactorPrecision);
    }

    // Requirement 3.1, 3.2 — nextReview = now + interval dias, com "now" por parâmetro.
    [Fact]
    public void ApplyReview_DefineNextReviewComoNowMaisInterval()
    {
        // Arrange: estado após o segundo acerto, cujo próximo interval será 6.
        SchedulingState afterFirst = new SchedulingState(2.5, 1, 1, Now);

        // Act
        SchedulingState result = _service.ApplyReview(afterFirst, (int)ReviewGrade.Good, Now);

        // Assert
        Assert.Equal(Now.AddDays(result.Interval), result.NextReview);
        Assert.Equal(Now.AddDays(6), result.NextReview);
    }

    // Requirement 5.1 — grade fora de 0..5 lança ArgumentOutOfRangeException.
    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void ApplyReview_GradeInvalida_LancaArgumentOutOfRange(int grade)
    {
        // Arrange
        SchedulingState state = _service.CreateInitialState(Now);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.ApplyReview(state, grade, Now));
    }

    // Requirement 5.2 — interval negativo no estado de entrada lança ArgumentException.
    [Fact]
    public void ApplyReview_IntervalNegativo_LancaArgumentException()
    {
        // Arrange
        SchedulingState invalid = new SchedulingState(2.5, -1, 0, Now);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.ApplyReview(invalid, (int)ReviewGrade.Good, Now));
    }

    // Requirement 5.2 — repetitions negativo no estado de entrada lança ArgumentException.
    [Fact]
    public void ApplyReview_RepetitionsNegativo_LancaArgumentException()
    {
        // Arrange
        SchedulingState invalid = new SchedulingState(2.5, 1, -1, Now);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.ApplyReview(invalid, (int)ReviewGrade.Good, Now));
    }

    // Requirement 6.1 — mesma (estado, grade, now) produz saídas idênticas (igualdade por valor).
    [Fact]
    public void ApplyReview_MesmaEntrada_ProduzSaidaIdentica()
    {
        // Arrange
        SchedulingState state = new SchedulingState(2.5, 6, 2, Now);

        // Act
        SchedulingState first = _service.ApplyReview(state, (int)ReviewGrade.Good, Now);
        SchedulingState second = _service.ApplyReview(state, (int)ReviewGrade.Good, Now);

        // Assert
        Assert.Equal(first, second);
    }
}
