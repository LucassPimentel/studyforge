namespace StudyForge.Api.Dtos;

/// <summary>Dados de entrada para criação de um deck.</summary>
public record CreateDeckDto(string Name);

/// <summary>Resumo de um deck para listagem: id, nome e contagem de cards.</summary>
public record DeckSummaryDto(int Id, string Name, int CardCount);

/// <summary>
/// Progresso de um deck em contagens de cards por estágio de aprendizado:
/// pendentes (nunca revisados), aprendendo, dominados e devidos agora.
/// </summary>
public record ProgressDto(int Pending, int Learning, int Mastered, int Due);
