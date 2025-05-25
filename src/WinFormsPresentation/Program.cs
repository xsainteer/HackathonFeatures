using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsPresentation;

static class Program
{
    [STAThread]
    static void Main()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        
        ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        var mainForm = serviceProvider.GetRequiredService<MainForm>();
        System.Windows.Forms.Application.Run(mainForm);
    }
    
    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddLogging(configure => configure.AddConsole())
            .AddInfrastructure()
            .AddScoped<MainForm>();
    }  
}