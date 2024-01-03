﻿using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

internal static class TrackDestinationExtensions
{
    public static string FlexDirection(this TrainBlocking _, bool reverse = false) =>
        reverse ? "row-reverse" : "row";

    public static string Display(this TrainInfo me) =>
        me.OperationDays().IsDaily ?
        $"{me.OperatorName} {me.Prefix} {me.Number}".Trim() :
        $"{me.OperatorName} {me.Prefix} {me.Number} {me.OperationDays().ShortName}".Trim();
}
