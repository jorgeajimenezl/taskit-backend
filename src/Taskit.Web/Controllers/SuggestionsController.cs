using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskit.Application.Services;

namespace Taskit.Web.Controllers;

[Authorize]
public class SuggestionsController(RecommendationService recommendationService) : ApiControllerBase
{
    private readonly RecommendationService _recommendationService = recommendationService;

    [HttpGet("tasks/{taskId}/assignees", Name = "GetSuggestedAssignees")]
    public async Task<ActionResult<IEnumerable<Application.DTOs.UserProfileDto>>> GetSuggestedAssignees(int taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var suggestions = await _recommendationService.GetAssigneeSuggestionsAsync(taskId, userId, count: 3);

        if (suggestions == null)
        {
            return NoContent();
        }

        return Ok(suggestions);
    }
}
