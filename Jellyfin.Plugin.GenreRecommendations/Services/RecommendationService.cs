using Jellyfin.Plugin.GenreRecommendations.Models;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.GenreRecommendations.Services;

public class RecommendationService
{
    // Genre-Mapping: Key → (Anzeigename, passende Genre-Tags aus Jellyfin)
    // "Animation" wird bewusst weggelassen (wäre fast alles in der Bibliothek)
    // "Romance & Comedy" ist eine eigene kombinierte Reihe
    private static readonly Dictionary<string, (string DisplayName, string[] Tags)> GenreMap = new()
    {
        ["action"]         = ("Action",           new[] { "Action", "Action & Adventure" }),
        ["adventure"]      = ("Abenteuer",         new[] { "Adventure", "Action & Adventure" }),
        ["comedy"]         = ("Comedy",            new[] { "Comedy", "Komödie" }),
        ["drama"]          = ("Drama",             new[] { "Drama" }),
        ["fantasy"]        = ("Fantasy",           new[] { "Fantasy", "Sci-Fi & Fantasy" }),
        ["horror"]         = ("Horror",            new[] { "Horror" }),
        ["mystery"]        = ("Mystery",           new[] { "Mystery" }),
        ["romance"]        = ("Romance",           new[] { "Romance" }),
        ["thriller"]       = ("Thriller",          new[] { "Thriller", "Suspense" }),
        ["scifi"]          = ("Science Fiction",   new[] { "Science Fiction", "Sci-Fi & Fantasy" }),
        ["crime"]          = ("Krimi",             new[] { "Krimi" }),
        ["war"]            = ("Krieg",             new[] { "War", "War & Politics" }),
        ["romance_comedy"] = ("Romance & Comedy",  new[] { "Romance", "Comedy", "Komödie" }),
    };

    private readonly ILibraryManager _libraryManager;
    private readonly RecommendationStore _store;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        ILibraryManager libraryManager,
        RecommendationStore store,
        ILogger<RecommendationService> logger)
    {
        _libraryManager = libraryManager;
        _store = store;
        _logger = logger;
    }

    public RecommendationResponse GetCachedRecommendations()
        => _store.Load();

    public async Task RefreshAsync(Guid sourceLibraryId, int itemsPerGenre, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[GenreRec] Starte Aktualisierung aus Bibliothek {Id}", sourceLibraryId);

        var allItems = _libraryManager.GetItemList(new InternalItemsQuery
        {
            ParentId = sourceLibraryId,
            IncludeItemTypes = new[] { BaseItemKind.Series, BaseItemKind.Movie },
            Recursive = true
        });

        var random = new Random();
        var sections = new List<GenreSection>();

        foreach (var (key, (displayName, tags)) in GenreMap)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var matching = allItems
                .Where(item => item.Genres != null &&
                               item.Genres.Any(g => tags.Contains(g, StringComparer.OrdinalIgnoreCase)))
                .OrderBy(_ => random.Next())
                .Take(itemsPerGenre)
                .Select(item => new RecommendationItem
                {
                    Id = item.Id.ToString("N"),
                    Name = item.Name ?? string.Empty,
                    Year = item.ProductionYear,
                    CommunityRating = item.CommunityRating
                })
                .ToList();

            if (matching.Count > 0)
            {
                sections.Add(new GenreSection
                {
                    Key = key,
                    DisplayName = displayName,
                    Items = matching
                });
            }
        }

        var response = new RecommendationResponse
        {
            Genres = sections,
            LastUpdated = DateTime.UtcNow
        };

        _store.Save(response);
        _logger.LogInformation("[GenreRec] Abgeschlossen: {Count} Genre-Reihen gespeichert", sections.Count);
        await Task.CompletedTask;
    }
}
