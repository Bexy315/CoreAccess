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
        logger.LogInformation("Fetching applications with search: {Search}, page: {Page}, pageSize: {PageSize}", options.Search, options.Page, options.PageSize);

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
}