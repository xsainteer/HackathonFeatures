using Infrastructure.Azure.FormRecognizer;
using Infrastructure.Email;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
        
        services.AddOptions<FormRecognizerSettings>()
            .BindConfiguration("Azure")
            .ValidateDataAnnotations()
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.ApiKey)
                            && !string.IsNullOrWhiteSpace(settings.Endpoint), 
                "Specify Azure settings in dotnet user-secrets");
        
        return services;
    }
}