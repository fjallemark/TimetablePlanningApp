using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellurian.Trains.Planning.App.Server.Services
{
    public static class ControllerBaseExtensions
    {
        internal static IActionResult GetScheduleItem<T>(this ControllerBase me, int? scheduleId, Func<int, T?> repositoryAction) where T: class
        {
            if (!scheduleId.HasValue) return me.BadRequest();
            var result = repositoryAction(scheduleId.Value);
            if (result is null) return me.NotFound();
            return me.Ok(result);
        }

        internal static async Task<IActionResult> GetScheduleItems<T>(this ControllerBase me, int? scheduleId, Func<int, Task<IEnumerable<T>>> repositoryAction)
        {
            if (!scheduleId.HasValue) return me.BadRequest();
            var result = await repositoryAction(scheduleId.Value).ConfigureAwait(false);
            if (result is null) return me.NotFound();
            return me.Ok(result);
        }
    }
}
