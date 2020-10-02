using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Extensions
{
    public static class BackgroundServiceExtension
    {
        public static IServiceCollection Run<T>(this IServiceCollection services)
            where T : BackgroundService
        {
            services.AddHostedService<T>();
            return services;
        }
    }
}