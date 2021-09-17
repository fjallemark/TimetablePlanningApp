namespace Tellurian.Trains.Planning.App.Contracts
{
    public class StationTrack
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsScheduledTrack { get; set; }
        public bool HasPlatform { get; set; }
    }
}
