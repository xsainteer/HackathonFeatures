using Application.Interfaces;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.Vision.Face;
using Infrastructure.Azure.BlobStorage;
using Infrastructure.Azure.FaceRecognizer;
using Infrastructure.Azure.FormRecognizer;
using Infrastructure.Database.Entities;
using Infrastructure.Email;
using Infrastructure.FraudDetection;
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
        //Document Intelligence
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

        services.AddScoped<IFraudDetectionService, FraudDetectionService>();
        
        //Face Recognizer
        services.AddOptions<FaceRecognizerSettings>()
            .BindConfiguration("FaceRecognizer")
            .ValidateDataAnnotations()
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.ApiKey)
                            && !string.IsNullOrWhiteSpace(settings.Endpoint)
                            && !string.IsNullOrWhiteSpace(settings.PersonGroupId), 
                "Specify FaceRecognizer settings in dotnet user-secrets");

        services.AddSingleton<FaceClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<FaceRecognizerSettings>>().Value;
            return new FaceClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        });
        
        services.AddSingleton<LargePersonGroupClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<FaceRecognizerSettings>>().Value;
            return new FaceAdministrationClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey)).GetLargePersonGroupClient(settings.PersonGroupId);
        });

        services.AddScoped<IFaceRecognizerService, FaceRecognizerService>();
        
        //Blob storage
        services.AddOptions<BlobStorageSettings>()
            .BindConfiguration("BlobStorage")
            .ValidateDataAnnotations()
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.ApiKey)
                            && !string.IsNullOrWhiteSpace(settings.Endpoint), 
                "Specify BlobStorage settings in dotnet user-secrets");
        
        return services;
    }
}