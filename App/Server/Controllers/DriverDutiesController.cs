using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.App.Server.Services;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverDutiesController : ControllerBase
    {
        public DriverDutiesController (DriverDutiesService service)
        {
            Service = service;
        }
        private readonly DriverDutiesService Service;

        [HttpGet]
        public IActionResult GetBooklet(string? scheduleName) => this.GetScheduleItem(scheduleName, Service.GetDriverDutyBooklet);
    }
}
