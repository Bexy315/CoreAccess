using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.EntityFrameworkCore.Models;

namespace CoreAccess.WebAPI.Controllers;

[Route("api/applications")]
[CoreAuthorize(Roles = "CoreAccess.Admin")]
public class ApplicationController(IApplicationService applicationService, ILogger<ApplicationController> logger) : ControllerBase
{
    [HttpGet]
    [Produces(typeof(PagedResult<ApplicationDto>))]
    public async Task<IActionResult> GetApplications([FromQuery]ApplicationSearchOptions options, CancellationToken cancellationToken = default)
    {
        var applications = await applicationService.GetApplications(options, cancellationToken);
        
        return Ok(applications);
    }
    [HttpGet]
    [Route("{id}")]
    [Produces(typeof(ApplicationDetailDto))]
    public async Task<IActionResult> GetApplicationById([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest("Application ID cannot be null or empty.");

        var application = await applicationService.GetApplication(id, cancellationToken);
        if (application == null)
            return NotFound($"Application with ID {id} not found.");

        return Ok(application);
    }
    
    [HttpPut]
    [Route("{id}")]
    [Produces(typeof(ApplicationDetailDto))]
    public async Task<IActionResult> UpdateApplication([FromRoute] string id, [FromBody] ApplicationUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest("Application ID cannot be null or empty.");

        if (request == null)
            return BadRequest("Request body cannot be null.");

        try
        {
            await applicationService.UpdateApplicationAsync(id, request, cancellationToken);
            var updatedApplication = await applicationService.GetApplication(id, cancellationToken);
            return Ok(updatedApplication);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Application with ID {id} not found.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating application with ID {ApplicationId}", id);
            return StatusCode(500, "An error occurred while updating the application.");
        }
    }
    
}