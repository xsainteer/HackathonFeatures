using Application.Interfaces;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Infrastructure.Azure.FormRecognizer;
using Infrastructure.Email;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Email
        services.AddOptions<SmtpSettings>()
            .BindConfiguration("Smtp")
            .ValidateDataAnnotations()
            .Validate(settings => 
            {
                // Validate SMTP settings (Port and Host)
                if (settings.Port <= 0 || settings.Port > 65535)
                    return false;
                return !string.IsNullOrWhiteSpace(settings.Host);
            }, "specify SMTP settings in dotnet user-secrets");
        
        services.AddScoped<IEmailSender<ApplicationUser>, SmtpEmailSender>();
        
        // Azure
        
        services.AddOptions<DocumentIntelligenceSettings>()
            .BindConfiguration("Azure")
            .ValidateDataAnnotations()
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.ApiKey)
                            && !string.IsNullOrWhiteSpace(settings.Endpoint), 
                "Specify Azure settings in dotnet user-secrets");

        services.AddScoped<IDocumentIntelligenceService, DocumentIntelligenceService>();

        services.AddSingleton<DocumentIntelligenceClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DocumentIntelligenceSettings>>().Value;
            return new DocumentIntelligenceClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        });
        
        services.AddSingleton<DocumentAnalysisClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DocumentIntelligenceSettings>>().Value;
            return new DocumentAnalysisClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        });
        
        return services;
    }
}