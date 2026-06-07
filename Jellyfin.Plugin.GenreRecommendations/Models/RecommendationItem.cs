namespace Jellyfin.Plugin.GenreRecommendations.Models;

public class RecommendationItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public int? Year { get; set; }
    public float? CommunityRating { get; set; }
}
