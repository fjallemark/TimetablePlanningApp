using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Server.Controllers
{
    [Route("api/languages")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        [HttpGet("all/supported")]
        public IActionResult GetSupportedLanguages() => Ok(Program.SupportedLanguages);

        [HttpGet("all/labels/waybills")]
        public IActionResult GetWaybillLabels() =>
            Ok(Program.SupportedLanguages.Select(l => LanguageLabelsExtensions.CreateLabels(l, WaybillExtensions.LabelResourceKeys)));
    }
}
