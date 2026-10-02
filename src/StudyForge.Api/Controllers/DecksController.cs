using Microsoft.AspNetCore.Mvc;
using StudyForge.Api.Dtos;
using StudyForge.Api.Services;

namespace StudyForge.Api.Controllers;

/// <summary>
/// Endpoints REST de gerenciamento de decks. Controller fino: delega a regra de
/// negócio ao <see cref="IDeckService"/>.
/// </summary>
[ApiController]
[Route("api/decks")]
public class DecksController : ControllerBase
{
    private readonly IDeckService _deckService;
    private readonly ICardService _cardService;
    private readonly IReviewService _reviewService;

    public DecksController(
        IDeckService deckService,
        ICardService cardService,
        IReviewService reviewService)
    {
        _deckService = deckService;
        _cardService = cardService;
        _reviewService = reviewService;
    }

    /// <summary>Lista os decks com a contagem de cards de cada um.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeckSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var decks = await _deckService.ListAsync(cancellationToken);
        return Ok(decks);
    }

    /// <summary>Cria um deck. Retorna 400 se o nome for vazio.</summary>
    [HttpPost]
    public async Task<ActionResult<DeckSummaryDto>> Create(
        [FromBody] CreateDeckDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _deckService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(List), new { id = created.Id }, created);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Exclui um deck e seus cards em cascata. Retorna 404 se não existir.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _deckService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Gera cards para um deck a partir de um bloco de texto (uma linha por card no
    /// formato <c>pergunta :: resposta</c>). Retorna 400 se o texto for vazio e 404 se o
    /// deck não existir.
    /// </summary>
    [HttpPost("{id:int}/cards/generate")]
    public async Task<ActionResult<GenerateResultDto>> GenerateCards(
        int id,
        [FromBody] GenerateCardsDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _cardService.GenerateFromTextAsync(id, dto, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Inicia uma sessão de revisão: retorna os cards devidos agora
    /// (<c>nextReview &lt;= now</c>) do deck. Retorna 404 se o deck não existir e uma lista
    /// vazia quando não houver cards devidos.
    /// </summary>
    [HttpGet("{id:int}/review")]
    public async Task<ActionResult<IReadOnlyList<CardDto>>> Review(
        int id,
        CancellationToken cancellationToken)
    {
        var dueCards = await _reviewService.GetDueCardsAsync(id, cancellationToken);
        return dueCards is null ? NotFound() : Ok(dueCards);
    }

    /// <summary>
    /// Retorna o progresso do deck: contagens de cards pendentes, aprendendo, dominados
    /// e devidos agora. Retorna 404 se o deck não existir.
    /// </summary>
    [HttpGet("{id:int}/progress")]
    public async Task<ActionResult<ProgressDto>> Progress(
        int id,
        CancellationToken cancellationToken)
    {
        var progress = await _deckService.GetProgressAsync(id, cancellationToken);
        return progress is null ? NotFound() : Ok(progress);
    }
}
