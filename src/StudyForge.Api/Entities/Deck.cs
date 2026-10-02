namespace StudyForge.Api.Entities;

/// <summary>
/// Coleção de cards de um assunto. Um deck possui de 0 a muitos cards,
/// excluídos em cascata quando o deck é removido.
/// </summary>
public class Deck
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public List<Card> Cards { get; set; } = new();
}
