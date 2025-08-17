using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreAccess.Workers;

public class CommonWorkerService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                using var scope = _serviceProvider.CreateScope();
                var initialSetupService = scope.ServiceProvider.GetRequiredService<IInitialSetupService>();
                
                if (!initialSetupService.IsSetupCompleted())
                {
                    var request = new InitialSetupRequest
                    {
                        GeneralInitialSettings = new GeneralInitialSetupRequest
                        {
                            BaseUri = "http://localhost:5000",
                            SystemLogLevel = "Debug",
                            DisableRegistration = "false",
                        },
                        JwtInitialSettings = new JwtInitialSetupRequest
                        {
                            Audience = "coreaccess-client",
                            Issuer = "coreaccess",
                            ExpiresIn = "3600"
                        },
                        UserInitialSettings = new UserInitialSetupRequest
                        {
                            Admin = new UserCreateRequest
                            {
                                Username = "Bexy",
                                Password = "admin123",
                                Email = "bexy315@mail.com",
                            }
                        }
                    };
                    
                    await initialSetupService.RunSetupAsync(request);
                }
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}