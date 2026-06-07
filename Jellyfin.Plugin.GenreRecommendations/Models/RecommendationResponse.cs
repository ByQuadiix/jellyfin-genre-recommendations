using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.GenreRecommendations.Models;

public class GenreSection
{
    public required string Key { get; set; }
    public required string DisplayName { get; set; }
    public List<RecommendationItem> Items { get; set; } = new();
}

public class RecommendationResponse
{
    public List<GenreSection> Genres { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
