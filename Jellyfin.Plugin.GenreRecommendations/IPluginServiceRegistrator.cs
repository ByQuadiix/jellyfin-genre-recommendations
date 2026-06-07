using MediaBrowser.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.GenreRecommendations;

public interface IPluginServiceRegistrator
{
    void RegisterServices(IServiceCollection serviceCollection, IApplicationHost applicationHost);
}
