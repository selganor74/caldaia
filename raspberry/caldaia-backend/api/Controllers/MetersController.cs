using application.subSystems;
using domain;
using domain.meters;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/meters/")]
public class MetersController : ControllerBase
{
    private readonly Camino camino;
    private readonly CaldaiaMetano caldaia;
    private readonly Rotex rotex;
    private readonly Riscaldamento riscaldamento;
    private readonly ILogger<MetersController> log;
    private readonly List<Subsystem> allSubsystems = new List<Subsystem>();

    public MetersController(
        Camino camino,
        CaldaiaMetano caldaia,
        Rotex rotex,
        Riscaldamento riscaldamento,
        ILogger<MetersController> log)
    {
        this.camino = camino;
        this.caldaia = caldaia;
        this.rotex = rotex;
        this.riscaldamento = riscaldamento;
        this.log = log;

        this.allSubsystems.Add(camino);
        this.allSubsystems.Add(caldaia);
        this.allSubsystems.Add(rotex);
        this.allSubsystems.Add(riscaldamento); 
    }

    [HttpGet()]
    [Route("analog")] 
    public async Task<IEnumerable<string>> GetAllAnalogMeters() 
    {
        return allSubsystems.SelectMany(s => s.AnalogMeters).Select(s => s.Name);
    }


    [Route("analog/{name}")]
    [HttpGet]
    public async Task<AnalogMeter> GetAnalogMeterByName(string name)
    {
        return allSubsystems.SelectMany(s => s.AnalogMeters).Where(s => s.Name == name).FirstOrDefault();
    }

    [Route("analog/{name}/history/{fromDate}")]
    [HttpGet()]
    public async Task<IEnumerable<IMeasure>> GetAnalogMeterHistory(string name, DateTimeOffset fromDate)
    {
        var meter = allSubsystems
            .SelectMany(s => s.AnalogMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return null;
        
        // In CSharp     = 2022-12-12T22:20:06.2038858Z
        // In Javascript = 2022-12-12T22:20:06.203Z
        // per evitare di restituire due volte lo stesso timestamp dobbiamo "approssimare" al millisecondo
        return meter.history.Where(d => (d.UtcTimeStamp - TimeSpan.FromMilliseconds(1)) > fromDate);
    }
    
    [Route("analog/{name}/stats/{fromDate}")]
    [HttpGet()]
    public async Task<StatsDTO> GetAnalogStats(string name, DateTimeOffset fromDate) {
        var meter = allSubsystems
            .SelectMany(s => s.AnalogMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return null;
        
        return meter.GetStats(fromDate);
    }

    [HttpGet()]
    [Route("digital")]
    public async Task<IEnumerable<string>> GetAllDigitalMeters() 
    {
        return allSubsystems.SelectMany(s => s.DigitalMeters).Select(s => s.Name);
    }


    [Route("digital/{name}")]
    [HttpGet]
    public async Task<DigitalMeter> GetDigitalMeterByName(string name)
    {
        return allSubsystems.SelectMany(s => s.DigitalMeters).Where(s => s.Name == name).FirstOrDefault();
    }

    [Route("digital/{name}/history/{fromDate}")]
    [HttpGet]
    public async Task<IEnumerable<IMeasure>> GetDigitalMeterHistory(string name, DateTimeOffset fromDate)
    {
        var meter = allSubsystems
            .SelectMany(s => s.DigitalMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return null;

        // In CSharp     = 2022-12-12T22:20:06.2038858Z
        // In Javascript = 2022-12-12T22:20:06.203Z
        // per evitare di restituire due volte lo stesso timestamp dobbiamo "approssimare" al millisecondo
        return meter.history.Where(d => (d.UtcTimeStamp - TimeSpan.FromMilliseconds(1)) > fromDate);
    }


    [Route("digital/{name}/stats/{fromDate}")]
    [HttpGet()]
    public async Task<StatsDTO> GetDigitalStats(string name, DateTimeOffset fromDate) {
        var meter = allSubsystems
            .SelectMany(s => s.DigitalMeters)
            .Where(s => s.Name == name)
            .FirstOrDefault();

        if (meter == null)
            return null;
        
        return meter.GetStats(fromDate);
    }
}
