using Jellyfin.Data.Enums;
using Jellyfin.Plugin.GenreRecommendations.Models;
using Jellyfin.Plugin.GenreRecommendations.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.GenreRecommendations.Api;

[ApiController]
[Route("GenreRecommendations")]
public class GenreRecommendationsController : ControllerBase
{
    private readonly RecommendationService _recommendationService;
    private readonly ILibraryManager _libraryManager;

    public GenreRecommendationsController(
        RecommendationService recommendationService,
        ILibraryManager libraryManager)
    {
        _recommendationService = recommendationService;
        _libraryManager = libraryManager;
    }

    /// <summary>
    /// Gibt die gecachten Genre-Empfehlungen zurück (für das Web-Plugin).
    /// </summary>
    [HttpGet("Sections")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RecommendationResponse), 200)]
    public ActionResult<RecommendationResponse> GetSections()
    {
        var data = _recommendationService.GetCachedRecommendations();
        return Ok(data);
    }

    /// <summary>
    /// Gibt alle verfügbaren Bibliotheken zurück (für die Konfigurationsseite).
    /// </summary>
    [HttpGet("Libraries")]
    [Authorize(Policy = "DefaultAuthorization")]
    public ActionResult<IEnumerable<object>> GetLibraries()
    {
        var folders = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.CollectionFolder }
            })
            .Select(f => new { id = f.Id.ToString("N"), name = f.Name })
            .ToList();

        return Ok(folders);
    }

    /// <summary>
    /// Löst eine sofortige Aktualisierung der Empfehlungen aus.
    /// </summary>
    [HttpPost("Refresh")]
    [Authorize(Policy = "DefaultAuthorization")]
    public async Task<ActionResult> TriggerRefresh(CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null || string.IsNullOrWhiteSpace(config.SourceLibraryId))
            return BadRequest(new { error = "Keine Quell-Bibliothek konfiguriert." });

        if (!Guid.TryParse(config.SourceLibraryId, out var libraryId))
            return BadRequest(new { error = "Ungültige Bibliotheks-ID." });

        await _recommendationService.RefreshAsync(libraryId, config.ItemsPerGenre, cancellationToken);

        config.LastRefresh = DateTime.UtcNow;
        Plugin.Instance!.SaveConfiguration();

        return Ok(new { message = "Empfehlungen erfolgreich aktualisiert.", lastUpdated = config.LastRefresh });
    }
}
