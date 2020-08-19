using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.App.Server.Services;
using Tellurian.Trains.Planning.Repositories;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocoSchedulesController : ControllerBase
    {
        public LocoSchedulesController(IRepository repository)
        {
            Repository = repository;
        }

        private readonly IRepository Repository;

        [HttpGet]
        public IActionResult GetItems([FromQuery] string? scheduleName) => this.GetScheduleItems(scheduleName, Repository.GetLocoSchedules);
    }
}
