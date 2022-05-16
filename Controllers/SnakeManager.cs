using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Halero.Controllers;

[ApiController]
[Route("/[controller]/[action]")]
class SnakeManagerController: ControllerBase
{
    public readonly ILogger<SnakeManagerController> _logger;

    public SnakeManagerController(ILogger<SnakeManagerController> logger){
        _logger = logger;
    } 

    // public string InitializeSession()
}