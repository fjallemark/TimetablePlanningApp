namespace Tellurian.Trains.Planning.App.Contracts;

public class Region
{
    public string Name { get; set; } = string.Empty;
    public string TextColor { get; set; } = "black";
    public string BackColor { get; set; } = "lightyellow";
    public static Region OriginDefault => new() { TextColor = "black", BackColor = "white" };
    public static Region DestinationDefault => new() { TextColor = "black", BackColor = "lightyellow" };
}
