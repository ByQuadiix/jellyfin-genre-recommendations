using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.GenreRecommendations.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string SourceLibraryId { get; set; } = string.Empty;
    public string SourceLibraryName { get; set; } = string.Empty;
    public int ItemsPerGenre { get; set; } = 8;
    public DateTime LastRefresh { get; set; } = DateTime.MinValue;
}
