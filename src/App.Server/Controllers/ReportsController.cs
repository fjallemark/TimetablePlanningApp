using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.App.Server.Extensions;
using Tellurian.Trains.Planning.App.Server.Services;

namespace Tellurian.Trains.Planning.App.Server.Controllers;

[Route("api/layouts/{id}/reports")]
[ApiController]
public class ReportsController(PrintedReportsService service) : ControllerBase
{
    private readonly PrintedReportsService Service = service;

    [HttpGet("blockdestinations")]
    public async Task<IActionResult> GetBlockDestinations(int id) => await this.GetScheduleItems(id, Service.GetBlockDestinationsAsync).ConfigureAwait(false);

    [HttpGet("driverduties")]
    public async Task<IActionResult> GetDriverDutiesBooklet(int id) => await this.GetScheduleItem(id, Service.GetDriverDutyBookletAsync).ConfigureAwait(false);
    [HttpGet("layout")]
    public async Task<IActionResult> GetLayout(int id) => await this.GetScheduleItem(id, Service.GetLayout).ConfigureAwait(false);


    [HttpGet("locoschedules")]
    public async Task<IActionResult> GetLocoSchedules(int id) => await this.GetScheduleItems(id, Service.GetLocoSchedulesAsync).ConfigureAwait(false);
    [HttpGet("shuntinglocos")]
    public async Task<IActionResult> GetShuntingLocos(int id) => await this.GetScheduleItems(id, Service.GetShuntingLocosAsync).ConfigureAwait(false);

    [HttpGet("stationduties")]
    public async Task<IActionResult> GetStationDutyBooklet(int id) => await this.GetScheduleItem(id, Service.GetStationDutyBookletAsync).ConfigureAwait(false);
    
    [HttpGet("stationstrainorder")]
    public async Task<IActionResult> GetStationsTrainOrder(int id) => await this.GetScheduleItem(id, Service.GetStationsTrainOrder).ConfigureAwait(false);

    [HttpGet("timetablestretches")]

    public async Task<IActionResult> GetTimetableStretches(int id, string? line)
    {
        if (id < 1) return BadRequest();
        var result = await Service.GetTimetableStretchesAsync(id, line).ConfigureAwait(false);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("trains")]
    public async Task<IActionResult> GetTrains(int id, string? @operator = null)
    {
        if (id < 1) return BadRequest();
        var result = await Service.GetTrainsAsync(id, @operator).ConfigureAwait(false);
        if (result is null) return NotFound();
        return Ok(result);

    }

    [HttpGet("trainstartlabels")]
    public async Task<IActionResult> GetTimetableDepartures(int id) => await this.GetScheduleItems(id, Service.GetTrainDeparturesAsync).ConfigureAwait(false);

    [HttpGet("trainsetschedules")]
    public async Task<IActionResult> GetLTrainsetSchedules(int id) => await this.GetScheduleItems(id, Service.GetTrainsetSchedulesAsync).ConfigureAwait(false);

}
