using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static ServiceCollection AddApplication(this ServiceCollection services)
    {
        services.AddScoped<IDocumentConverterService, DocumentConverterService>();

        services.AddScoped<IFraudDetectionAppService, IFraudDetectionAppService>();
        
        return services;
    }
}