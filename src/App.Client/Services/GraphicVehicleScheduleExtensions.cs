using System;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services;

#pragma warning disable IDE0060 // Remove unused parameter

public static class GraphicVehicleScheduleExtensions
{
    private static readonly GraphicVehicleScheduleOptions Options = new();
    public static int Height(this VehicleSchedule me) => Options.Height;
    public static int Width(this VehicleSchedule me) => Options.Width;

    public static int TimeX(this CallTime time, int startHour, int endHour) => 
        (int)Math.Round(Options.HeaderWidth + ((Options.Width - Options.HeaderWidth) * ((time.AsTimeSpan().TotalHours - startHour) / (endHour - startHour))), 0);
    
    public static CallTime MiddleTime(this TrainPart part)

    {
        var a = part.ToArrival.AsTimeSpan();
        var d = part.FromDeparture.AsTimeSpan();

        return (d + (a - d)/2).FromTimeSpan(true, false, Array.Empty<string>());
    }

    public static int DurationX(this TrainPart part, int startHour, int endHour) => 
        part.ToArrival.TimeX(startHour, endHour) - part.FromDeparture.TimeX(startHour, endHour);

    public static int TimeX(this CallAction? call, int startHour, int endHour) => 
        call is null ? 0 : TimeX(call.Time.AsTimeSpan().TotalHours, startHour, endHour);

    public static int TimeX(this double hours, int startHour, int endHour) => 
        (int)Math.Round(Options.HeaderWidth + ((Options.Width - Options.HeaderWidth) * (hours - startHour) / (endHour - startHour)));

    public static int TimeX(this int hours, int startHour, int endHour) => 
        TimeX((double)hours, startHour, endHour);

    public static int StationSignatureFontSize(this TrainPart part, int startHour, int endHour)
    {
        var x = part.DurationX(startHour, endHour);
        return x < 25 ? 6 : 8;
    }

    public static string Minute(this CallAction? me) =>
        me is null || me.Time is null || me.Time.Time.Length != 5 ? string.Empty : me.Time.Time.Substring(3, 2);

    public static string TrainColor(this TrainPart me) => me.Train is null ? "#000000" : me.Train.Color;
    public static string TrainTextColor(this TrainPart me) => me.Train is null ? "#FFFFFF" : me.Train.Color.TextColor() ?? "#FFFFFF";

}

public sealed record GraphicVehicleScheduleOptions
{
    public int Height { get; init; } = 40;
    public int Width { get; init; } = 1100;
    public int HeaderWidth { get; init; } = 100;
}
