using System;
using System.Collections.Generic;
using Jellyfin.Plugin.GenreRecommendations.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.GenreRecommendations;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public override string Name => "Genre Recommendations";

    public override Guid Id => Guid.Parse("a3b1c5d2-e4f6-4801-b9c7-3d2e8f1a0b5c");

    public override string Description => "Wöchentlich wechselnde Genre-Empfehlungen auf der Jellyfin-Startseite.";

    public IEnumerable<PluginPageInfo> GetPages()
    {
        var prefix = GetType().Namespace;
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = $"{prefix}.Configuration.configPage.html",
                IsMainConfigPage = true
            },
            new PluginPageInfo
            {
                Name = "genrerecommendations.js",
                EmbeddedResourcePath = $"{prefix}.Web.genrerecommendations.js"
            }
        };
    }
}
