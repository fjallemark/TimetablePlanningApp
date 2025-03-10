﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Planning.App.Server.Extensions;

public static class ControllerBaseExtensions
{
    internal static async Task<IActionResult> GetScheduleItem<T>(this ControllerBase me, int? scheduleId, Func<int, Task<T>> repositoryAction)
    {
        if (scheduleId.IsBadRequest()) return me.BadRequest();
        var result = await repositoryAction(scheduleId.Value).ConfigureAwait(false);
        if (result.IsNotFound()) return me.NotFound();
        return me.Ok(result);
    }

    internal static async Task<IActionResult> GetScheduleItems<T>(this ControllerBase me, int? scheduleId, Func<int, Task<IEnumerable<T>>> repositoryAction)
    {
        if (scheduleId.IsBadRequest()) return me.BadRequest();
        var result = await repositoryAction(scheduleId.Value).ConfigureAwait(false);
        if (result.IsNotFound()) return me.NotFound();
        return me.Ok(result);
    }
    internal static async Task<IActionResult> GetScheduleItems<T>(this ControllerBase me, int? scheduleId, string? filter, Func<int, string?, Task<T>> repositoryAction)
    {
        if (scheduleId.IsBadRequest()) return me.BadRequest();
        var result = await repositoryAction(scheduleId.Value, filter).ConfigureAwait(false);
        if (result.IsNotFound()) return me.NotFound();
        return me.Ok(result);
    }

    internal static async Task<IActionResult> GetScheduleItems<T>(this ControllerBase me, int? scheduleId, string? filter, Func<int, string?, Task<IEnumerable<T>>> repositoryAction)
    {
        if (scheduleId.IsBadRequest()) return me.BadRequest();
        var result = await repositoryAction(scheduleId.Value, filter).ConfigureAwait(false);
        if (result.IsNotFound()) return me.NotFound();
        return me.Ok(result);
    }

    private static bool IsBadRequest([NotNullWhen(false)] this int? id) => id is null || id < 0;
    private static bool IsNotFound<T>([NotNullWhen(false)] this T? result) => result is null || result is IEnumerable<T> e && !e.Any();

}
