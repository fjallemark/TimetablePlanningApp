namespace Tellurian.Trains.Planning.App.Shared
{
    public class Region
    {
        public string Name { get; set; } = string.Empty;
        public string TextColor { get; set; } = "black";
        public string BackColor { get; set; } = "lightyellow";
        public static Region OriginDefault => new Region { TextColor = "black", BackColor = "white" };
        public static Region DestinationDefault => new Region { TextColor = "black", BackColor = "lightyellow" };
    }
}
