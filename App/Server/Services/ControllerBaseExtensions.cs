using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Tellurian.Trains.Planning.App.Server.Services
{
    public static class ControllerBaseExtensions
    {
        internal static IActionResult GetScheduleItem<T>(this ControllerBase me, string? scheduleName, Func<string, T?> repositoryAction) where T: class
        {
            if (string.IsNullOrWhiteSpace(scheduleName)) return me.BadRequest();
            var result = repositoryAction(scheduleName);
            if (result is null) return me.NotFound();
            return me.Ok(result);
        }

        internal static IActionResult GetScheduleItems<T>(this ControllerBase me, string? scheduleName, Func<string, IEnumerable<T>?> repositoryAction)
        {
            if (string.IsNullOrWhiteSpace(scheduleName)) return me.BadRequest();
            var result = repositoryAction(scheduleName);
            if (result is null) return me.NotFound();
            return me.Ok(result);
        }
    }
}
