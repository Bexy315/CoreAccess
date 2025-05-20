using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

public class RoleController(IRoleService roleService) : ControllerBase
{
    
}