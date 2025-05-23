using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static ServiceCollection AddApplication(this ServiceCollection services)
    {
        services.AddScoped<IDocumentConverterService, DocumentConverterService>();
        
        return services;
    }
}