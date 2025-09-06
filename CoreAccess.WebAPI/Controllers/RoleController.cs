using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/role")]
public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [Produces(typeof(PagedResult<RoleDto>))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetRoles([FromQuery] RoleSearchOptions options)
    {
        try
        {
            var result = await roleService.SearchRolesAsync(options);
            return Ok(result);
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet]
    [Route("{id}")]
    [Produces(typeof(PagedResult<RoleDetailDto>))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetRoleById([FromRoute]string id)
    {
        try
        {
            if(string.IsNullOrEmpty(id))
                return BadRequest("Role ID cannot be null or empty.");
            
            var result = await roleService.GetRoleByIdAsync(id);
            if(result == null)
                return NotFound($"Role with ID {id} not found.");
            
            return Ok(result);
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost]
    [Produces(typeof(RoleDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateRequest request)
    {
        try
        {
            if(request == null)
                return BadRequest("Role request cannot be null.");
            
            var result = await roleService.CreateRoleAsync(request);
            return Ok(result);
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPut]
    [Route("{id}")]
    [Produces(typeof(RoleDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> UpdateRole([FromRoute]string id, [FromBody] RoleUpdateRequest request)
    {
        try
        {
            if(string.IsNullOrEmpty(id))
                return BadRequest("Role ID cannot be null or empty.");
            
            var result = await roleService.UpdateRoleAsync(id, request);
            return Ok(result);
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpDelete]
    [Route("{id}")]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> DeleteRole([FromRoute]string id)
    {
        try
        {
            if(string.IsNullOrEmpty(id))
                return BadRequest("Role ID cannot be null or empty.");
            
            await roleService.DeleteRoleAsync(id);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPut]
    [Route("{roleId}/permissions/{permissionId}")]
    [Produces(typeof(RoleDetailDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> AddPermissionToRole([FromRoute]string roleId, [FromRoute]string permissionId)
    {
        try
        {
            if(string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(permissionId))
                return BadRequest("Role ID and Permission ID cannot be null or empty.");
            
            await roleService.AddPermissionToRoleAsync(roleId, permissionId);
            
            return Ok();
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}