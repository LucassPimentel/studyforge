using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using StudyForge.Domain;

namespace StudyForge.Domain.Tests;

/// <summary>
/// Testes property-based (FsCheck) para <see cref="SM2SchedulingService"/>.
///
/// Enquanto os testes por exemplo em <see cref="SM2SchedulingServiceTests"/> verificam casos
/// concretos, estes testes declaram invariantes gerais do algoritmo SM-2 e deixam o FsCheck
/// gerar centenas de entradas válidas tentando violá-las. As oito propriedades correspondem
/// às invariantes descritas em design.md e são complementares — não substituem — aos testes
/// por exemplo da tarefa 7.
/// </summary>
public class SM2SchedulingServicePropertyTests
{
    private static readonly SM2SchedulingService Service = new();

    /// <summary>
    /// "Now" fixo em UTC usado como base das propriedades. O serviço é determinístico em
    /// relação ao "now", então um instante fixo é suficiente para verificar a coerência de
    /// <see cref="SchedulingState.NextReview"/> sem introduzir variação irrelevante.
    /// </summary>
    private static readonly DateTime Now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // --- Invariante 1: easeFactor nunca abaixo do piso (requirement 2.2) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool EaseFactor_NuncaFicaAbaixoDoPiso(ValidState state, ValidGrade grade)
    {
        SchedulingState result = Service.ApplyReview(state.Value, grade.Value, Now);
        return result.EaseFactor >= 1.3;
    }

    // --- Invariante 2: erro (grade < 3) reinicia repetitions=0 e interval=1 (requirement 1.2) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool Erro_ReiniciaRepetitionsEInterval(ValidState state, ValidGrade grade)
    {
        // Restringe ao ramo de erro (grades 0..2).
        if (grade.Value >= 3)
        {
            return true;
        }

        SchedulingState result = Service.ApplyReview(state.Value, grade.Value, Now);
        return result.Repetitions == 0 && result.Interval == 1;
    }

    // --- Invariante 3: acerto (grade >= 3) incrementa repetitions em 1 (requirement 1.3) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool Acerto_IncrementaRepetitions(ValidState state, ValidGrade grade)
    {
        // Restringe ao ramo de acerto (grades 3..5).
        if (grade.Value < 3)
        {
            return true;
        }

        SchedulingState result = Service.ApplyReview(state.Value, grade.Value, Now);
        return result.Repetitions == state.Value.Repetitions + 1;
    }

    // --- Invariante 4: interval de saída >= 1 após uma revisão (requirement 1.*) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool Interval_NaoEhNegativo(ValidState state, ValidGrade grade)
    {
        SchedulingState result = Service.ApplyReview(state.Value, grade.Value, Now);
        return result.Interval >= 1;
    }

    // --- Invariante 5: nextReview == now + interval dias (requirement 3.1) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool NextReview_EhCoerenteComOInterval(ValidState state, ValidGrade grade)
    {
        SchedulingState result = Service.ApplyReview(state.Value, grade.Value, Now);
        return result.NextReview == Now.AddDays(result.Interval);
    }

    // --- Invariante 6: monotonicidade na grade — grade maior nunca produz easeFactor menor (requirement 2.1) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool Monotonicidade_GradeMaiorNuncaReduzEaseFactor(
        ValidState state, ValidGrade gradeA, ValidGrade gradeB)
    {
        // Ordena as duas grades geradas de modo que g1 <= g2.
        int g1 = Math.Min(gradeA.Value, gradeB.Value);
        int g2 = Math.Max(gradeA.Value, gradeB.Value);

        double easeLow = Service.ApplyReview(state.Value, g1, Now).EaseFactor;
        double easeHigh = Service.ApplyReview(state.Value, g2, Now).EaseFactor;

        return easeHigh >= easeLow;
    }

    // --- Invariante 7: determinismo — mesma (estado, grade, now) produz saída idêntica (requirement 6.1) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool Determinismo_MesmaEntradaProduzSaidaIdentica(ValidState state, ValidGrade grade)
    {
        SchedulingState first = Service.ApplyReview(state.Value, grade.Value, Now);
        SchedulingState second = Service.ApplyReview(state.Value, grade.Value, Now);

        // Igualdade por valor do record.
        return first == second;
    }

    // --- Invariante 8: grade fora de 0..5 sempre lança ArgumentOutOfRangeException (requirement 5.1) ---
    [Property(Arbitrary = new[] { typeof(Sm2Arbitraries) })]
    public bool GradeInvalida_SempreLanca(ValidState state, InvalidGrade grade)
    {
        try
        {
            Service.ApplyReview(state.Value, grade.Value, Now);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }
}

/// <summary>
/// Wrapper de um <see cref="SchedulingState"/> cujos campos estão dentro das faixas válidas
/// definidas em design.md. O wrapper evita que o FsCheck gere estados inválidos (ex.: interval
/// negativo) que não fazem parte do espaço de entrada destas invariantes.
/// </summary>
public sealed record ValidState(SchedulingState Value);

/// <summary>Wrapper de uma grade válida (0..5).</summary>
public sealed record ValidGrade(int Value);

/// <summary>Wrapper de uma grade inválida (fora de 0..5).</summary>
public sealed record InvalidGrade(int Value);

/// <summary>
/// Geradores (Arbitraries) customizados do FsCheck que restringem as entradas ao espaço
/// válido descrito em design.md:
/// easeFactor ∈ [1.3, 3.0], interval ∈ [0, 3650], repetitions ∈ [0, 500],
/// grade válida ∈ {0..5}, grade inválida ∉ {0..5}.
/// </summary>
public static class Sm2Arbitraries
{
    /// <summary>Gera um <see cref="SchedulingState"/> dentro das faixas válidas.</summary>
    public static Arbitrary<ValidState> ValidStateArb()
    {
        // easeFactor ∈ [1.3, 3.0]: gera em milésimos para cobrir valores fracionários.
        Gen<double> easeFactorGen = Gen.Choose(1300, 3000).Select(x => x / 1000.0);
        Gen<int> intervalGen = Gen.Choose(0, 3650);
        Gen<int> repetitionsGen = Gen.Choose(0, 500);

        Gen<ValidState> gen =
            from ef in easeFactorGen
            from interval in intervalGen
            from reps in repetitionsGen
            select new ValidState(new SchedulingState(ef, interval, reps, DateTime.MinValue));

        return Arb.From(gen);
    }

    /// <summary>Gera uma grade válida em {0, 1, 2, 3, 4, 5}.</summary>
    public static Arbitrary<ValidGrade> ValidGradeArb()
    {
        Gen<ValidGrade> gen = Gen.Choose(0, 5).Select(g => new ValidGrade(g));
        return Arb.From(gen);
    }

    /// <summary>Gera uma grade inválida: qualquer inteiro fora da faixa 0..5.</summary>
    public static Arbitrary<InvalidGrade> InvalidGradeArb()
    {
        // Faixa ampla abaixo e acima do intervalo válido, excluindo 0..5.
        Gen<int> gen = Gen.Choose(-1000, 1000).Where(g => g < 0 || g > 5);
        return Arb.From(gen.Select(g => new InvalidGrade(g)));
    }
}
