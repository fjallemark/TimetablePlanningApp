using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.App.Server.Services;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/layouts/{id}/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        public ReportsController(PrintedReportsService service)
        {
            Service = service;
        }
        private readonly PrintedReportsService Service;

        [HttpGet("driverduties")]
        public async Task<IActionResult> GetDriverDuties(int id) => await this.GetScheduleItems(id, Service.GetDriverDutiesAsync).ConfigureAwait(false);

        [HttpGet("locoschedules")]
        public async Task<IActionResult> GetLocoSchedules(int id) => await this.GetScheduleItems(id, Service.GetLocoSchedulesAsync).ConfigureAwait(false);

        [HttpGet("trainsetschedules")]
        public async Task<IActionResult> GetLTrainsetSchedules(int id) => await this.GetScheduleItems(id, Service.GetTrainsetSchedulesAsync).ConfigureAwait(false);

        [HttpGet("waybills")]
        public async Task<IActionResult> GetWaybills(int id) => await this.GetScheduleItems(id, Service.GetWaybillsAsync).ConfigureAwait(false);
    }
}
