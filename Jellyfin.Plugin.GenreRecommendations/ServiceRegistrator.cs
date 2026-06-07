using Jellyfin.Plugin.GenreRecommendations.Services;
using MediaBrowser.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.GenreRecommendations;

public class ServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<RecommendationStore>();
        serviceCollection.AddSingleton<RecommendationService>();
    }
}
