using Jellyfin.Plugin.GenreRecommendations.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.GenreRecommendations.ScheduledTasks;

public class RefreshRecommendationsTask : IScheduledTask
{
    private readonly RecommendationService _recommendationService;
    private readonly ILogger<RefreshRecommendationsTask> _logger;

    public RefreshRecommendationsTask(
        RecommendationService recommendationService,
        ILogger<RefreshRecommendationsTask> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    public string Name => "Genre Empfehlungen aktualisieren";
    public string Key => "GenreRecommendationsRefresh";
    public string Description => "Wählt wöchentlich neue zufällige Genre-Empfehlungen für die Startseite.";
    public string Category => "Genre Recommendations";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerWeekly,
                DayOfWeek = DayOfWeek.Monday,
                TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
            }
        };
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null || string.IsNullOrWhiteSpace(config.SourceLibraryId))
        {
            _logger.LogWarning("[GenreRec] Keine Quell-Bibliothek konfiguriert. Task übersprungen.");
            return;
        }

        if (!Guid.TryParse(config.SourceLibraryId, out var libraryId))
        {
            _logger.LogError("[GenreRec] Ungültige Bibliotheks-ID in der Konfiguration.");
            return;
        }

        progress.Report(0);
        await _recommendationService.RefreshAsync(libraryId, config.ItemsPerGenre, cancellationToken);
        progress.Report(100);

        config.LastRefresh = DateTime.UtcNow;
        Plugin.Instance!.SaveConfiguration();
    }
}
