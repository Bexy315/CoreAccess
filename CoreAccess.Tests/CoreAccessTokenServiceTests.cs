using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using System.Collections.Generic;
using Xunit;
using System.Security.Claims;
using Moq;
using CoreAccess.WebAPI.Repositories;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Services.CoreAuth;

namespace CoreAccess.Tests;

public class CoreAccessTokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsValidJwt()
    {
        // Arrange
        var dummyUser = new CoreUser
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Roles = new List<CoreRole>
            {
                new CoreRole { Name = "Admin" }
            }
        };

        // AppSettingsHelper ist statisch → wir setzen explizit alle Werte
        Environment.SetEnvironmentVariable("CoreAccess_JwtSecretKey", Convert.ToBase64String(Encoding.UTF8.GetBytes("supersecretkey123supersecretkey123")));
        Environment.SetEnvironmentVariable("CoreAccess_JwtIssuer", "TestIssuer");
        Environment.SetEnvironmentVariable("CoreAccess_JwtAudience", "TestAudience");
        Environment.SetEnvironmentVariable("CoreAccess_JwtExpiresIn", "30");

        var userServiceMock = new Mock<IUserService>();
        var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        var appSettingsServiceMock = new Mock<IAppSettingsService>();

        var service = new CoreAccessTokenService(appSettingsServiceMock.Object, userServiceMock.Object, refreshTokenRepoMock.Object);

        // Act
        var token = service.GenerateAccessToken(dummyUser);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == CoreAccessClaimType.UserId && c.Value == dummyUser.Id.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == CoreAccessClaimType.UserName && c.Value == dummyUser.Username);
        Assert.Contains(jwt.Claims, c => c.Type == CoreAccessClaimType.Roles && c.Value.Contains("Admin"));
    }
}
