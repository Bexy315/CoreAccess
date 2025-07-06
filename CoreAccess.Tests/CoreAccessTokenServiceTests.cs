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
        
    }
}
