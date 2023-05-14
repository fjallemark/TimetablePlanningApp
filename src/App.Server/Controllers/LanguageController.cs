using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Server.Services;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/languages")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        [HttpGet("all/supported")]
        public IActionResult GetSupportedLanguages() => Ok(LanguageService.SupportedLanguages);

        [HttpGet("all/labels/waybills")]
        public IActionResult GetWaybillLabels() =>
            Ok(LanguageService.SupportedLanguages.Select(l => LanguageLabelsExtensions.CreateLabels(l, WaybillExtensions.LabelResourceKeys)));
    }
}
