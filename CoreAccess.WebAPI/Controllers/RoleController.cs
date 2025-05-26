using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/role")]
public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [CoreAuthorize]
    public async Task<IActionResult> GetRoles([FromQuery] CoreRoleSearchOptions options)
    {
        try
        {
            var result = await roleService.SearchRolesAsync(options);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    [CoreAuthorize]
    public async Task<IActionResult> CreateRole([FromBody] CoreRoleCreateRequest request)
    {
        try
        {
            var result = await roleService.CreateRoleAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut]
    [Route("/api/role/{id}")]
    [CoreAuthorize]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] CoreRoleUpdateRequest request)
    {
        try
        {
            var result = await roleService.UpdateRoleAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete]
    [Route("/api/role/{id}")]
    [CoreAuthorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        try
        {
            await roleService.DeleteRoleAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal server error");
        }
    }
}