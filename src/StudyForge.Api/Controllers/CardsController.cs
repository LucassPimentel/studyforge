using Microsoft.AspNetCore.Mvc;
using StudyForge.Api.Dtos;
using StudyForge.Api.Services;

namespace StudyForge.Api.Controllers;

/// <summary>
/// Endpoints REST de revisão de cards. Controller fino: delega a regra de negócio ao
/// <see cref="IReviewService"/>.
/// </summary>
[ApiController]
[Route("api/cards")]
public class CardsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public CardsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Avalia um card com uma grade (0..5), aplica o SM-2 e persiste o novo agendamento.
    /// Retorna 400 se a grade for inválida e 404 se o card não existir.
    /// </summary>
    [HttpPost("{id:int}/review")]
    public async Task<ActionResult<ReviewResultDto>> Review(
        int id,
        [FromBody] GradeDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _reviewService.GradeCardAsync(id, dto, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
