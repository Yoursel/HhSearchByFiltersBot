using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HhBot.DI;

public static class OptionsRegistrationExtensions
{
    public static OptionsBuilder<TOptions> AddConfiguredOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        return services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateOnStart();
    }
}