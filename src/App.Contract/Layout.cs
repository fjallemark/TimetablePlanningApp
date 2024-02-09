namespace Tellurian.Trains.Planning.App.Contracts;
public class Layout
{
    public string Name { get; set; }=string.Empty;
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; } 
    public string? FontFamily { get; set; }
    public int MaxLocoDriversCount { get; set; }
    public int StartWeekdayId { get; set; }
}
