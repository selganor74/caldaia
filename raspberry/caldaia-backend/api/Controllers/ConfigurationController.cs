using application;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/configuration")]
public class ConfigurationController : ControllerBase
{
    private readonly CaldaiaConfig configuration;

    public ConfigurationController(
        CaldaiaConfig configuration
        )
    {
        this.configuration = configuration;
    }

    [HttpGet(Name = "GetRotexConfig")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<CaldaiaConfig> GetRotexConfig()
    {
        return Ok(configuration);
    }


}