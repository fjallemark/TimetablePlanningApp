using System.Globalization;

namespace Tellurian.Trains.Planning.App.Contracts;

public class CallTime
{
    public string Time { get; set; } = string.Empty;
    public bool IsHidden { get; set; } = true;
    public bool IsStop { get; set; }
    public IList<Note> Notes { get; set; } = new List<Note>();

    public static CallTime Create(string time, params string[] notes) =>
        new () { Time = time, IsStop = true, IsHidden = false, Notes = notes.Select(n => new Note { Text = n }).ToArray() };
    public static CallTime Create(string time, bool isStop = true, bool isHidden = false, params string[] notes) =>
        new()
        { Time = time, IsStop = isStop, IsHidden = isHidden, Notes = notes.Select(n => new Note { Text = n }).ToArray() };

    public override string ToString() => IsHidden || Time is null ? string.Empty : Time;
}

public static class CallTimeExtensions
{
    public static double OffsetMinutes(this CallTime? me) =>
        me.AsTimeSpan().TotalMinutes;

    public static int OffsetIndex(this CallTime? me, TimeSpan offsetTime) =>
        (int)((me.AsTimeSpan() - offsetTime).TotalMinutes);


    public static TimeSpan AsTimeSpan(this CallAction? me) =>
       me is null || me.Time is null ? TimeSpan.Zero : TimeSpan.Parse(me.Time.Time, CultureInfo.InvariantCulture);

    public static TimeSpan AsTimeSpan(this CallTime? me) =>
        me is null ? TimeSpan.Zero : TimeSpan.Parse(me.Time, CultureInfo.InvariantCulture);

    public static CallTime FromTimeSpan(this TimeSpan value, bool isStop, bool isHidden, params string[] notes) =>
        CallTime.Create($"{value.Hours}:{value.Minutes}", isStop, isHidden, notes);
}
