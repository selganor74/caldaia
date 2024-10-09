using application.subSystems;
using domain;
using domain.meters;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/meters/")]
public class MetersController : ControllerBase
{
    private readonly ILogger<MetersController> log;
    private readonly List<Subsystem> allSubsystems = new List<Subsystem>();

    public MetersController(
        ILogger<MetersController> log)
    {
        this.log = log;
    }

    [HttpGet()]
    [Route("analog")]
    [Produces("application/json", Type = typeof(IEnumerable<string>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllAnalogMeters() 
    {
        var toReturn = allSubsystems.SelectMany(s => s.AnalogMeters).Select(s => s.Name);
        return Ok(toReturn);
    }


    [Route("analog/{name}")]
    [HttpGet]
    [Produces("application/json", Type = typeof(AnalogMeter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAnalogMeterByName(string name)
    {
        try
        {
            var toReturn = allSubsystems.SelectMany(s => s.AnalogMeters).Where(s => s.Name == name).Single();
            return Ok(toReturn);
        } catch
        {
            return NotFound();
        }
    }

    [Route("analog/{name}/history/{fromDate}")]
    [HttpGet()]
    [Produces("application/json", Type = typeof(IEnumerable<IMeasure>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAnalogMeterHistory(string name, DateTimeOffset fromDate)
    {
        var meter = allSubsystems
            .SelectMany(s => s.AnalogMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return NotFound();
        
        // In CSharp     = 2022-12-12T22:20:06.2038858Z
        // In Javascript = 2022-12-12T22:20:06.203Z
        // per evitare di restituire due volte lo stesso timestamp dobbiamo "approssimare" al millisecondo
        var toReturn = meter.history.Where(d => (d.UtcTimeStamp - TimeSpan.FromMilliseconds(1)) > fromDate).ToList();
        return Ok(toReturn);
    }
    
    [Route("analog/{name}/stats/{fromDate}")]
    [HttpGet()]
    [Produces("application/json", Type = typeof(StatsDTO))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAnalogStats(string name, DateTimeOffset fromDate) {
        var meter = allSubsystems
            .SelectMany(s => s.AnalogMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return NotFound();

        var toReturn = meter.GetStats(fromDate);
        return Ok(toReturn);
    }

    [HttpGet()]
    [Route("digital")]
    [Produces("application/json", Type = typeof(IEnumerable<string>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllDigitalMeters() 
    {
        var toReturn = allSubsystems.SelectMany(s => s.DigitalMeters).Select(s => s.Name);
        return Ok(toReturn);
    }


    [Route("digital/{name}")]
    [HttpGet]
    [Produces("application/json", Type = typeof(DigitalMeter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetDigitalMeterByName(string name)
    {
        var toReturn = allSubsystems.SelectMany(s => s.DigitalMeters).Where(s => s.Name == name).FirstOrDefault();
        return Ok(toReturn);
    }

    [Route("digital/{name}/history/{fromDate}")]
    [HttpGet]
    [Produces("application/json", Type = typeof(IEnumerable<IMeasure>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetDigitalMeterHistory(string name, DateTimeOffset fromDate)
    {
        var meter = allSubsystems
            .SelectMany(s => s.DigitalMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return NotFound();

        // In CSharp     = 2022-12-12T22:20:06.2038858Z
        // In Javascript = 2022-12-12T22:20:06.203Z
        // per evitare di restituire due volte lo stesso timestamp dobbiamo "approssimare" al millisecondo
        var toReturn = meter.history.Where(d => (d.UtcTimeStamp - TimeSpan.FromMilliseconds(1)) > fromDate);
        return Ok(toReturn);
    }


    [Route("digital/{name}/stats/{fromDate}")]
    [HttpGet()]
    [Produces("application/json", Type = typeof(StatsDTO))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetDigitalStats(string name, DateTimeOffset fromDate) {
        var meter = allSubsystems
            .SelectMany(s => s.DigitalMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return NotFound();
        
        var toReturn = meter.GetStats(fromDate);
        return Ok(toReturn);
    }
}
