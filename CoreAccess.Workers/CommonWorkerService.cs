using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreAccess.Workers;

public class CommonWorkerService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" || 
           Environment.GetEnvironmentVariable("COREACCESS_DEBUGMODE") == "True")
        {
            using var scope = serviceProvider.CreateScope();
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
                    
                await initialSetupService.RunSetupAsync(request, cancellationToken: stoppingToken);
            }
                            
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                
            var existingTestUser = await userService.SearchUsersAsync(new UserSearchOptions
            {
                Username = "TestUser",
                PageSize = 1
            }, stoppingToken);

            if (existingTestUser.Items.FirstOrDefault() == null)
            {
                var newTestUser = await userService.CreateUserAsync(new UserCreateRequest()
                {
                    Username = "TestUser",
                    Password = "admin123",
                    Email = "testuser123@mail.com",
                    FirstName = "Test",
                    LastName = "User"
                }, cancellationToken: stoppingToken);
                await userService.AddRoleToUserAsync(newTestUser.Id.ToString(), "User", cancellationToken: stoppingToken);
            }
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}