using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.Repositories;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaybillsController : ControllerBase
    {
        public WaybillsController(IRepository repository)
        {
            Repository = repository;
        }
        private readonly IRepository Repository;

        [HttpGet()]
        public IActionResult Get([FromQuery] string? name)
        {
            return Ok(Repository.GetWaybills(name));
        }
    }
}
