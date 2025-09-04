using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/permissions")]
public class PermissionController(IPermissionService permissionService) : ControllerBase
{
    [HttpGet]
    [Produces(typeof(PagedResult<PermissionDto>))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetAllPermissions([FromQuery] PermissionSearchOptions options)
    {
        try
        {
            var permissions = await permissionService.SearchPermissionsAsync(options);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}