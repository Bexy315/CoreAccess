using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [Route("api/roles")]
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
    [Route("api/roles")]
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
    [Route("api/roles/{id}")]
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
    [Route("api/roles/{id}")]
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