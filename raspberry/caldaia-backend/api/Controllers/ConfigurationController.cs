using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> log;

    public ConfigurationController(
        ILogger<ConfigurationController> log
    )
    {
        this.log = log;
    }

    [HttpGet(Name = "GetRotexConfig")]
    public object GetRotexConfig() {
        return "";
    }

    
}