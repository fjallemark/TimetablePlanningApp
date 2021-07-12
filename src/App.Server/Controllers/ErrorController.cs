using Microsoft.AspNetCore.Mvc;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet()]
        public IActionResult Error() => NotFound();
    }
}
