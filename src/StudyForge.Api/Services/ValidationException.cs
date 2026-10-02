namespace StudyForge.Api.Services;

/// <summary>
/// Erro de regra de negócio por entrada inválida (ex.: nome de deck vazio).
/// Os controllers traduzem esta exceção para uma resposta HTTP 400.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
