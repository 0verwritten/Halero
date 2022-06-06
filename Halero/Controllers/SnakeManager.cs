using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Halero.Services.UserManagement;
using Halero.Models.UserManagement;
using Microsoft.Extensions.Primitives;
using Halero.Models.UserManagement.Forms;

namespace Halero.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class SnakeManagerController: ControllerBase
{
    public readonly ILogger<SnakeManagerController> _logger;
    public readonly IUserManager _userManager;

    public SnakeManagerController(ILogger<SnakeManagerController> logger, IUserManager userManager){
        _logger = logger;
        _userManager = userManager;
    } 

    [ActionName("get")]
    [HttpGet(Name = "GetWeatherForecast")]
    public string Get(){
        return "test";
    }

    

}