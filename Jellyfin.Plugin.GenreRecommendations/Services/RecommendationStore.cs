using System.Text.Json;
using Jellyfin.Plugin.GenreRecommendations.Models;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.GenreRecommendations.Services;

public class RecommendationStore
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public RecommendationStore(IApplicationPaths appPaths)
    {
        _filePath = Path.Combine(appPaths.DataPath, "genre_recommendations.json");
    }

    public RecommendationResponse Load()
    {
        if (!File.Exists(_filePath))
            return new RecommendationResponse();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<RecommendationResponse>(json) ?? new RecommendationResponse();
        }
        catch
        {
            return new RecommendationResponse();
        }
    }

    public void Save(RecommendationResponse data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
