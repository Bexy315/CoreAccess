using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using CoreAccess.WebAPI.Decorator;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/metrics")]
public class MetricsController(IUserService userService, IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [Route("dashboard")]
    [Produces(typeof(DashboardMetrics))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var users = await userService.SearchUsersAsync(options: new UserSearchOptions());
        var roles = await roleService.SearchRolesAsync(options: new RoleSearchOptions());
        
        DashboardMetrics metrics = new ();
        metrics.TotalUsers = users.TotalCount;
        metrics.TotalRoles = roles.TotalCount;
        
        return Ok(metrics);
    }
    
}