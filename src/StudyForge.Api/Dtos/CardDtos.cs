namespace StudyForge.Api.Dtos;

/// <summary>Dados de entrada para geração de cards a partir de um bloco de texto.</summary>
public record GenerateCardsDto(string Text);

/// <summary>Resultado da geração de cards: quantidade de cards criados.</summary>
public record GenerateResultDto(int Created);
