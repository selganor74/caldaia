using System.Data;
using application.services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly IRotexReader rotexReader;
    private readonly ILogger<ConfigurationController> log;

    public ConfigurationController(
        IRotexReader rotexReader,
        ILogger<ConfigurationController> log
    )
    {
        this.rotexReader = rotexReader;
        this.log = log;
    }

    [HttpGet(Name = "GetRotexConfig")]
    public object GetRotexConfig() {
        return rotexReader.GetRotexConfig();
    } 
}