using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.Repositories;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainsetSchedulesController : ControllerBase
    {
        public TrainsetSchedulesController(IRepository repository)
        {
            Repository = repository;
        }

        private readonly IRepository Repository;

        [HttpGet]
        public IActionResult GetItems([FromQuery] string? name)
        {
            return Ok(Repository.GetTrainsetSchedules(name));
        }
    }
}
