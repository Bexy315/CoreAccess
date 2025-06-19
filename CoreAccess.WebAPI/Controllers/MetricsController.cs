using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/metrics")]
public class MetricsController(IUserService userService, IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    [Route("dashboard")]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var users = await userService.SearchUsersAsync(options: new CoreUserSearchOptions());
        var roles = await roleService.SearchRolesAsync(options: new CoreRoleSearchOptions());
        var metrics = new
        {
            TotalUsers = users.TotalCount,
            TotalRoles = roles.TotalCount,
        };

        return Ok(metrics);
    }
    
}