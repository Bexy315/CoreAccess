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
    public async Task<IActionResult> GetPermissions([FromQuery] PermissionSearchOptions options)
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
    [HttpGet]
    [Route("{id}")]
    [Produces(typeof(PermissionDetailDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetPermissionById([FromRoute] string id,[FromQuery] bool includeRoles = false)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Permission ID cannot be null or empty.");

            var permission = await permissionService.GetPermissionByIdAsync(id, includeRoles);
            if (permission == null)
                return NotFound($"Permission with ID {id} not found.");

            return Ok(permission);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost]
    [Produces(typeof(PermissionDetailDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionCreateRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
                return BadRequest("Permission data is invalid.");

            var createdPermission = await permissionService.CreatePermissionAsync(request);
            return CreatedAtAction(nameof(GetPermissionById), new { id = createdPermission.Id }, createdPermission);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut]
    [Route("{id}")]
    [Produces(typeof(PermissionDetailDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> UpdatePermission([FromRoute] string id, [FromBody] PermissionUpdateRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(id) || request == null)
                return BadRequest("Permission ID and data cannot be null or empty.");

            var updatedPermission = await permissionService.UpdatePermissionAsync(id, request);
            return Ok(updatedPermission);
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
    public async Task<IActionResult> DeletePermission([FromRoute] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Permission ID cannot be null or empty.");

            await permissionService.DeletePermissionAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}